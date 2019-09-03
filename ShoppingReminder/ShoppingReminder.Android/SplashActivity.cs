using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace ShoppingReminder.Droid
{
    [Activity(Theme ="@style/Theme.Splash",MainLauncher =false,NoHistory =true)]
    public class SplashActivity:Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //
        }
        protected override void OnResume()
        {
            base.OnResume();
            Task startupwork = new Task(() => { StartActivity(new Intent(Application.Context, typeof(MainActivity))); });
            startupwork.Start();
        }
    }
}