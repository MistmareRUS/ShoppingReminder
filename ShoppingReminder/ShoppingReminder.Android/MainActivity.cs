using Android.App;
using Android.Content.PM;
using Android.OS;
using Plugin.CurrentActivity;
using Plugin.Settings;

[assembly: UsesFeature("android.hardware.camera", Required = false)]
[assembly: UsesFeature("android.hardware.camera.autofocus", Required = false)]
namespace ShoppingReminder.Droid
{
    [Activity(Label = "Shopping Reminder", Icon = "@drawable/icon100",  MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            BarStyle.ma = this;
            SetTheme(CrossSettings.Current.GetValueOrDefault("ThemeId", Resource.Style.MainTheme_Standart));  
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);
            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);//плагин для диалоговых окон с вводом текста
            Android.Gms.Ads.MobileAds.Initialize(ApplicationContext, "ca-app-pub-5542764698208489~2621881069");// мой ID рекламы
            CrossCurrentActivity.Current.Init(this, savedInstanceState);//камера
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
            
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        public override void OnBackPressed()
        {
            if (Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed))
            {
            }
            else
            {
            }
        }
    }
}