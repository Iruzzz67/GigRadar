using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadProfileCommand.ExecuteAsync(null);
    }

    private void OnEditClicked(object? sender, EventArgs e)
    {
        _viewModel.IsEditing = !_viewModel.IsEditing;
    }
}
