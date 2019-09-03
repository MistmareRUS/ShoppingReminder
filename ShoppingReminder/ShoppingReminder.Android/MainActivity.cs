using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Plugin.CurrentActivity;

[assembly: UsesFeature("android.hardware.camera", Required = false)]
[assembly: UsesFeature("android.hardware.camera.autofocus", Required = false)]
namespace ShoppingReminder.Droid
{
    [Activity(Label = "Shopping Reminder", Icon = "@drawable/icon100", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug("---------------------main activity", " on create - start");
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            BarStyle.ma = this;
            Log.Debug("---------------------main activity", " on create - 1");
            base.OnCreate(savedInstanceState);
            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);//плагин для диалоговых окон с вводом текста
            Android.Gms.Ads.MobileAds.Initialize(ApplicationContext, "ca-app-pub-5542764698208489~2621881069");// мой ID рекламы
            CrossCurrentActivity.Current.Init(this, savedInstanceState);//камера
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Log.Debug("---------------------main activity", " on create - 2");
            LoadApplication(new App());
            Log.Debug("---------------------main activity", " on create - end");
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        public override void OnBackPressed()
        {
            if (Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed))
            {
                // Do something if there are some pages in the `PopupStack`
                Log.Debug("---------------------main activity", " back true");

            }
            else
            {
                // Do something if there are not any pages in the `PopupStack`
                Log.Debug("---------------------main activity", " back false");

            }
        }
    }
}