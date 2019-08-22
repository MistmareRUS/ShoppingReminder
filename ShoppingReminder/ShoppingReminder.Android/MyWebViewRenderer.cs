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
using ShoppingReminder;
using ShoppingReminder.Droid;
using ShoppingReminder.Model;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(MyWebView), typeof(MyWebViewRenderer))]
namespace ShoppingReminder.Droid
{

        class MyWebViewRenderer : WebViewRenderer
        {
            public MyWebViewRenderer(Context context) : base(context)
            {

            }
            public override bool DispatchTouchEvent(MotionEvent e)
            {
                Parent.RequestDisallowInterceptTouchEvent(true);
                return base.DispatchTouchEvent(e);
            }
            protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
            {
                base.OnElementChanged(e);

                if (Control != null)
                {
                    Control.Settings.BuiltInZoomControls = true;
                    Control.Settings.DisplayZoomControls = false;

                    Control.Settings.LoadWithOverviewMode = true;
                    Control.Settings.UseWideViewPort = true;
                }
            }
    }
}
