using LocationShareApp.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace LocationShareApp.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
        
        // 设置地图初始位置
        if (_viewModel.CurrentLocation != null)
        {
            var location = _viewModel.CurrentLocation;
            var mapSpan = MapSpan.FromCenterAndRadius(
                new Location(location.Latitude, location.Longitude),
                Distance.FromKilometers(5));
            MapView.MoveToRegion(mapSpan);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }
}