using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class ArtistDetailPage : ContentPage
{
    public ArtistDetailPage(ArtistDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
