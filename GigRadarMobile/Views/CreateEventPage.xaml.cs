using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class CreateEventPage : ContentPage
{
    private readonly CreateEventViewModel _viewModel;

    public CreateEventPage(CreateEventViewModel viewModel)
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
