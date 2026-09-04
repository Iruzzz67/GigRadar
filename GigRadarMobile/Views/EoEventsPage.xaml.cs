using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class EoEventsPage : ContentPage
{
    public EoEventsPage(EoEventsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is EoEventsViewModel vm)
        {
            vm.LoadCommand.Execute(null);
        }
    }
}