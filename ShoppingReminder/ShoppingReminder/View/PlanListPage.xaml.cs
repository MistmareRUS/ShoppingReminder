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

		}

        //private void Button_Clicked(object sender, EventArgs e)
        //{
        //    App.Plans.Add(new Plan { Name = NewPlanEntry.Text });//TODO: сохранять в БД и обновлять в App.
        //    ViewModel.Back();            
        //}
    }
}