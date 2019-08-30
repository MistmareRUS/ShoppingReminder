using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Ads;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ShoppingReminder.Ad;
using ShoppingReminder.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

//AdRequest.Builder.addTestDevice("31829191451A45D0C9BA5886FC352C30")

[assembly: ExportRenderer(typeof(AdMobView), typeof(AdMobViewRenderer))]
namespace ShoppingReminder.Droid.Renderers
{
    public class AdMobViewRenderer : ViewRenderer<AdMobView, AdView>
    {
        public AdMobViewRenderer(Context context) : base(context) { }
        protected override void OnElementChanged(ElementChangedEventArgs<AdMobView> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null && Control == null)
                SetNativeControl(CreateAdView());
        }
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == nameof(AdView.AdUnitId))
                Control.AdUnitId = Element.AdUnitId;
        }        

        private AdView CreateAdView()
        {
            var adView = new AdView(Context)
            {
                AdSize = AdSize.Banner,
                AdUnitId = Element.AdUnitId
            };

            adView.LayoutParameters = new LinearLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);

            adView.LoadAd(new AdRequest.Builder()
#if Debug
                                                 .AddTestDevice("31829191451A45D0C9BA5886FC352C30")//хонор
                                                 .AddTestDevice("0ADE3D52A292C689C77DFB0F7391684C")//беляш
                                                 .AddTestDevice("427589D7CAA65D4458800ECF9C484686")//хтс
#endif
                                                 .Build());//TODO: номера тестовых девайсов

            return adView;
        }
    }
}