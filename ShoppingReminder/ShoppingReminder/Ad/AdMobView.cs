using Xamarin.Forms;

namespace ShoppingReminder.Ad
{
    public class AdMobView : Xamarin.Forms.View
    {
        public static readonly BindableProperty AdUnitIdProperty = BindableProperty.Create(
                  nameof(AdUnitId),
                  typeof(string),
                  typeof(AdMobView),
                  string.Empty);

        public string AdUnitId
        {
            get => (string)GetValue(AdUnitIdProperty);
            set => SetValue(AdUnitIdProperty, value);
        }
    }
}
