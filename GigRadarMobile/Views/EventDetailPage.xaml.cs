using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class EventDetailPage : ContentPage
{
    public EventDetailPage(EventDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Menyegarkan data sekaligus mencatat view di server (§11).
        (BindingContext as EventDetailViewModel)?.RefreshCommand.Execute(null);
    }
}
