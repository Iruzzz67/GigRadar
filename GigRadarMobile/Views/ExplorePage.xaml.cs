using GigRadarMobile.Helpers;
using GigRadarMobile.ViewModels;
using Microsoft.Maui.Dispatching;

namespace GigRadarMobile.Views;

public partial class ExplorePage : ContentPage
{
    private readonly ExploreViewModel _viewModel;
    private IDispatcherTimer? _sweepTimer;

    public ExplorePage(ExploreViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        // Nyala/mati sweep saat berpindah mode tampilan (List/Grid/Radar).
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ExploreViewModel.IsRadarView))
                RestartSweep();
        };

        // Optimasi: timer sweep dihentikan saat halaman tidak lagi dipakai
        // agar tidak ada loop invalidate yang berjalan di latar belakang.
        Unloaded += (_, _) => StopSweep();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Radar.ReducedMotion = ReducedMotion.IsEnabled;
        _ = _viewModel.OnPageAppearingAsync();
        RestartSweep();
    }

    protected override void OnDisappearing()
    {
        StopSweep();
        base.OnDisappearing();
    }

    private void RestartSweep()
    {
        StopSweep();

        // Animasi radar hanya saat mode Radar aktif + halaman terlihat + reduced motion off.
        if (!_viewModel.IsRadarView || _viewModel.Radar.ReducedMotion)
        {
            ExploreRadarView.Invalidate();
            return;
        }

        _sweepTimer = Dispatcher.CreateTimer();
        _sweepTimer.Interval = TimeSpan.FromMilliseconds(40);
        _sweepTimer.Tick += (_, _) =>
        {
            // Guard ganda: berhenti total bila user sudah keluar dari mode Radar
            // (sebelumnya timer terus jalan hanya melewatkan tick).
            if (!_viewModel.IsRadarView)
            {
                StopSweep();
                return;
            }
            _viewModel.Radar.SweepDeg = (_viewModel.Radar.SweepDeg + 5) % 360;
            ExploreRadarView.Invalidate();
        };
        _sweepTimer.Start();
    }

    private void StopSweep()
    {
        _sweepTimer?.Stop();
        _sweepTimer = null;
    }

    private void OnRadarStartInteraction(object? sender, TouchEventArgs e)
    {
        if (e.Touches.Length == 0) return;
        var touch = e.Touches[0];
        _viewModel.SelectNodeAt(touch.X, touch.Y);
        ExploreRadarView.Invalidate();
    }
}