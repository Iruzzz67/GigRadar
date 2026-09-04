using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class EventDetailPage : ContentPage
{
    public EventDetailPage(EventDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
