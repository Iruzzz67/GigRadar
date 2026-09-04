using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class EoAnalyticsPage : ContentPage
{
    public EoAnalyticsPage(EoAnalyticsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is EoAnalyticsViewModel vm)
        {
            vm.LoadCommand.Execute(null);
        }
    }
}