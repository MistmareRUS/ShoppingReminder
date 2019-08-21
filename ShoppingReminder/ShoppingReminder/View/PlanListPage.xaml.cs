using ShoppingReminder.Model;
using ShoppingReminder.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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