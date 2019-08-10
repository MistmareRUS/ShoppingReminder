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
	public partial class CompletedPurchaseListPage : ContentView
	{
		public CompletedPurchaseListPage (PurchaseListViewModel vm)
		{
			InitializeComponent ();
            BindingContext = vm;
        }
	}
}