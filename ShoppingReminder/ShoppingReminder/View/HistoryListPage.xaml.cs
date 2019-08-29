using ShoppingReminder.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingReminder.View
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HistoryListPage : ContentView
	{        
		public HistoryListPage (HistoryListViewModel vm )
		{
			InitializeComponent ();
            BindingContext = vm;
        }
    }
}