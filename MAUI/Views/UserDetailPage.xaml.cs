using LocationShareApp.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace LocationShareApp.Views;

public partial class UserDetailPage : ContentPage
{
    private readonly UserDetailViewModel _viewModel;

    public UserDetailPage(UserDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        await _viewModel.InitializeAsync();
        // 等待数据加载完成后更新地图
        await Task.Delay(1000);
        UpdateTrackMap();
    }

    private void UpdateTrackMap()
    {
        if (_viewModel.LocationHistory?.Any() == true)
        {
            var locations = _viewModel.LocationHistory.ToList();
            var firstLocation = locations.First();
            
            // 设置地图中心点
            var mapSpan = MapSpan.FromCenterAndRadius(
                new Location(firstLocation.Latitude, firstLocation.Longitude),
                Distance.FromKilometers(2));
            TrackMap.MoveToRegion(mapSpan);

            // 添加位置标记
            foreach (var location in locations.Take(10)) // 只显示最近10个位置点
            {
                var pin = new Pin
                {
                    Label = location.Timestamp.ToString("HH:mm"),
                    Address = location.Address,
                    Type = PinType.Place,
                    Location = new Location(location.Latitude, location.Longitude)
                };
                TrackMap.Pins.Add(pin);
            }
        }
    }
}