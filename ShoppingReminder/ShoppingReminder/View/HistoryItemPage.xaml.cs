using ShoppingReminder.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingReminder.View
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HistoryItemPage : ContentView
	{
        public HistoryViewModel ViewModel { get; private set; }

        public HistoryItemPage (HistoryViewModel vm)
		{
			InitializeComponent ( );
            ViewModel = vm;
            BindingContext = ViewModel;
            listView.ItemSelected += (object sender, SelectedItemChangedEventArgs e) => {
                if (e.SelectedItem == null) return;
                ((ListView)sender).SelectedItem = null;
            };
        }
	}
}