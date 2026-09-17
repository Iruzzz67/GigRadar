using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class OrganizerVerificationPage : ContentPage
{
    public OrganizerVerificationPage(OrganizerVerificationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as OrganizerVerificationViewModel)?.LoadCommand.Execute(null);
    }
}
