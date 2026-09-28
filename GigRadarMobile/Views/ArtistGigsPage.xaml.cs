using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class ArtistGigsPage : ContentPage
{
    public ArtistGigsPage(ArtistGigsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ArtistGigsViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
