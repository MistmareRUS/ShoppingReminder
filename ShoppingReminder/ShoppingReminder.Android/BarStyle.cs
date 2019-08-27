using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
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
        }
        public static MainActivity ma;
    }
}