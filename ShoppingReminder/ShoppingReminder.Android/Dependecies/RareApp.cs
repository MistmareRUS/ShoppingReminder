
using Android.Content;
using Android.Net;
using Android.Widget;
using ShoppingReminder.Model;
using Xamarin.Forms;

[assembly: Dependency(typeof(ShoppingReminder.Droid.Dependecies.RateApp))]
namespace ShoppingReminder.Droid.Dependecies
{
    class RateApp : IRareApp
    {
        public void Rate()
        {
            Uri uri = Uri.Parse("market://details?id=" + ma.PackageName);
            Intent myAppLinkToMarket = new Intent(Intent.ActionView, uri);
            try
            {
                ma.StartActivity(myAppLinkToMarket);
            }
            catch
            {
                if(Java.Util.Locale.Default.ToString()=="ru_RU")
                    Toast.MakeText(ma, "Play market не доступен!",ToastLength.Long).Show();
                else
                    Toast.MakeText(ma, "Play market not available!",ToastLength.Long).Show();
            }
        }
        public static MainActivity ma;

    }
}