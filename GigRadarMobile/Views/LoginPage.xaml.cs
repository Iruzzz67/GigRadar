using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        RoleUserRadio.IsChecked = true;
    }

    /// <summary>Sinkronkan pilihan role daftar (§25) dari RadioButton ke ViewModel.</summary>
    private void OnRoleChecked(object? sender, CheckedChangedEventArgs e)
    {
        if (!e.Value || sender is not RadioButton radio) return;

        _viewModel.SelectedRole = radio.Content switch
        {
            "Artist" => "Artist",
            "EO" => "EO",
            _ => "User"
        };
    }
}
