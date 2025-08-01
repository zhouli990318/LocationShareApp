using LocationShareApp.ViewModels;

namespace LocationShareApp.Views;

public partial class SplashPage : ContentPage
{
    public SplashPage(SplashViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}