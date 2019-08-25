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
        
        public  bool SetColor(string color)
        {            
            ma.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            ma.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            ma.Window.SetStatusBarColor( Android.Graphics.Color.ParseColor(color));
            return true;
        }
        public static MainActivity ma;
    }
}