using Android.Content;
using AndroidX.Core.App;
using Android;
using Android.Content.PM;

namespace LocationShareApp.Platforms.Android.Services
{
    public class AndroidSpecificService
    {
        public static bool CheckLocationPermissions()
        {
            var context = Platform.CurrentActivity ?? Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            if (context == null) return false;

            var fineLocationPermission = ActivityCompat.CheckSelfPermission(context, Manifest.Permission.AccessFineLocation);
            var coarseLocationPermission = ActivityCompat.CheckSelfPermission(context, Manifest.Permission.AccessCoarseLocation);
            
            return fineLocationPermission == Permission.Granted && coarseLocationPermission == Permission.Granted;
        }

        public static bool CheckBackgroundLocationPermission()
        {
            var context = Platform.CurrentActivity ?? Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            if (context == null) return false;

            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.Q)
            {
                var backgroundLocationPermission = ActivityCompat.CheckSelfPermission(context, Manifest.Permission.AccessBackgroundLocation);
                return backgroundLocationPermission == Permission.Granted;
            }
            
            return true; // 在Android 10以下版本不需要后台位置权限
        }

        public static void RequestLocationPermissions()
        {
            var activity = Platform.CurrentActivity as AndroidX.AppCompat.App.AppCompatActivity;
            if (activity == null) return;

            var permissions = new[]
            {
                Manifest.Permission.AccessFineLocation,
                Manifest.Permission.AccessCoarseLocation
            };

            ActivityCompat.RequestPermissions(activity, permissions, 1001);
        }

        public static void RequestBackgroundLocationPermission()
        {
            var activity = Platform.CurrentActivity as AndroidX.AppCompat.App.AppCompatActivity;
            if (activity == null) return;

            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.Q)
            {
                var permissions = new[] { Manifest.Permission.AccessBackgroundLocation };
                ActivityCompat.RequestPermissions(activity, permissions, 1002);
            }
        }

        public static void OpenAppSettings()
        {
            var context = Platform.CurrentActivity ?? Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            if (context == null) return;

            var intent = new Intent(global::Android.Provider.Settings.ActionApplicationDetailsSettings);
            var uri = global::Android.Net.Uri.FromParts("package", context.PackageName, null);
            intent.SetData(uri);
            context.StartActivity(intent);
        }

        public static bool IsBatteryOptimizationIgnored()
        {
            var context = Platform.CurrentActivity ?? Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            if (context == null) return false;

            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.M)
            {
                var powerManager = context.GetSystemService(Context.PowerService) as global::Android.OS.PowerManager;
                return powerManager?.IsIgnoringBatteryOptimizations(context.PackageName) ?? false;
            }
            
            return true;
        }

        public static void RequestIgnoreBatteryOptimization()
        {
            var context = Platform.CurrentActivity ?? Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            if (context == null) return;

            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.M)
            {
                var intent = new Intent(global::Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations);
                var uri = global::Android.Net.Uri.Parse($"package:{context.PackageName}");
                intent.SetData(uri);
                context.StartActivity(intent);
            }
        }
    }
}