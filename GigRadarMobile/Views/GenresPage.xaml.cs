using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class GenresPage : ContentPage
{
    public GenresPage(AdminGenresViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as AdminGenresViewModel)?.LoadCommand.Execute(null);
    }
}
