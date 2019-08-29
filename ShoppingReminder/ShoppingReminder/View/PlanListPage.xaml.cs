using ShoppingReminder.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingReminder.View
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PlanListPage : ContentView
	{
        public PlanListPage (PlanListViewModel vm)
		{
			InitializeComponent ();
            BindingContext = vm;
            listView.ItemSelected += (object sender, SelectedItemChangedEventArgs e) => {
                if (e.SelectedItem == null) return;
                ((ListView)sender).SelectedItem = null; 
            };
        }        
    }
}