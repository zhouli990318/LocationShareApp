using LocationShareApp.ViewModels;

namespace LocationShareApp.Views;

public partial class ProfilePage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public ProfilePage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // 确保 ViewModel 已初始化
        if (_viewModel.CurrentUser == null)
        {
            await _viewModel.InitializeAsync();
        }
    }
}
