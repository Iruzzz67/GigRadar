using GigRadarMobile.Helpers;
using GigRadarMobile.ViewModels;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace GigRadarMobile.Views;

public partial class RadarPage : ContentPage
{
    private readonly RadarViewModel _viewModel;

    public RadarPage(RadarViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnPageAppearing();
        UpdateMapPins();
    }

    private void UpdateMapPins()
    {
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(RadarViewModel.CenterLat) || e.PropertyName == nameof(RadarViewModel.RadiusKm) || e.PropertyName == nameof(RadarViewModel.Events))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var center = new Location(_viewModel.CenterLat, _viewModel.CenterLng);
                    var radius = Distance.FromKilometers(_viewModel.RadiusKm);
                    RadarMap.MoveToRegion(MapSpan.FromCenterAndRadius(center, radius));
                });
            }
        };
        
        // Initial move
        var initialCenter = new Location(_viewModel.CenterLat, _viewModel.CenterLng);
        var initialRadius = Distance.FromKilometers(_viewModel.RadiusKm);
        RadarMap.MoveToRegion(MapSpan.FromCenterAndRadius(initialCenter, initialRadius));
    }
}