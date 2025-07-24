using CoreLocation;
using UIKit;
using Foundation;

namespace LocationShareApp.Platforms.iOS.Services
{
    public class iOSSpecificService
    {
        public static bool CheckLocationPermissions()
        {
            var status = CLLocationManager.Status;
            return status == CLAuthorizationStatus.AuthorizedWhenInUse || 
                   status == CLAuthorizationStatus.AuthorizedAlways;
        }

        public static bool CheckBackgroundLocationPermission()
        {
            var status = CLLocationManager.Status;
            return status == CLAuthorizationStatus.AuthorizedAlways;
        }

        public static void RequestLocationPermissions()
        {
            var locationManager = new CLLocationManager();
            locationManager.RequestWhenInUseAuthorization();
        }

        public static void RequestBackgroundLocationPermission()
        {
            var locationManager = new CLLocationManager();
            locationManager.RequestAlwaysAuthorization();
        }

        public static void OpenAppSettings()
        {
            var settingsUrl = new NSUrl(UIApplication.OpenSettingsUrlString);
            if (UIApplication.SharedApplication.CanOpenUrl(settingsUrl))
            {
                UIApplication.SharedApplication.OpenUrl(settingsUrl);
            }
        }

        public static bool IsBackgroundAppRefreshEnabled()
        {
            return UIApplication.SharedApplication.BackgroundRefreshStatus == UIBackgroundRefreshStatus.Available;
        }

        public static void ShowBackgroundAppRefreshAlert()
        {
            var alert = UIAlertController.Create(
                "后台应用刷新",
                "为了保持位置同步，请在设置中启用后台应用刷新功能",
                UIAlertControllerStyle.Alert);

            alert.AddAction(UIAlertAction.Create("设置", UIAlertActionStyle.Default, _ => OpenAppSettings()));
            alert.AddAction(UIAlertAction.Create("取消", UIAlertActionStyle.Cancel, null));

            var viewController = Platform.GetCurrentUIViewController();
            viewController?.PresentViewController(alert, true, null);
        }

        public static void ConfigureBackgroundModes()
        {
            // iOS后台模式配置已在Info.plist中设置
            // 这里可以添加运行时的后台任务配置
        }
    }
}