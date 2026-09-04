using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class EoProfilePage : ContentPage
{
    private readonly EoProfileViewModel _viewModel;

    public EoProfilePage(EoProfileViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}