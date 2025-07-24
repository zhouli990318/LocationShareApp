using LocationShareApp.ViewModels;

namespace LocationShareApp.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}