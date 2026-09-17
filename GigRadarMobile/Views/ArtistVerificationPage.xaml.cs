using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class ArtistVerificationPage : ContentPage
{
    public ArtistVerificationPage(ArtistVerificationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as ArtistVerificationViewModel)?.LoadCommand.Execute(null);
    }
}
