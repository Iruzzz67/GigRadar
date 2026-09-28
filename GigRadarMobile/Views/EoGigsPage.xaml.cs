using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class EoGigsPage : ContentPage
{
    public EoGigsPage(EoGigsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is EoGigsViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
