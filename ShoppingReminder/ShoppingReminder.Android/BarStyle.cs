using Android.Views;
using Plugin.Settings;
using ShoppingReminder.Droid;
using ShoppingReminder.Themes;
using Xamarin.Forms;

[assembly: Dependency(typeof(BarStyle)) ]
namespace ShoppingReminder.Droid
{
    class BarStyle : IBarStyle
    {        
        public  void SetColor(string color)
        {            
            if(Android.OS.Build.VERSION.SdkInt>= Android.OS.BuildVersionCodes.Lollipop)
            {
                ma.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                ma.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                ma.Window.SetStatusBarColor( Android.Graphics.Color.ParseColor(color));
            }
            if (color== "#B9A8D5")
            {
                CrossSettings.Current.AddOrUpdateValue("ThemeId", Resource.Style.MainTheme_Lavender);
            }
            else
            {
                CrossSettings.Current.AddOrUpdateValue("ThemeId", Resource.Style.MainTheme_Standart);
            }
        }
        public static MainActivity ma;
    }
}