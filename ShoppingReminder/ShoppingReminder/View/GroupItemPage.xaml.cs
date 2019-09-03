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
	public partial class GroupItemPage : ContentView
	{       
        public GroupItemViewModel ViewModel { get; private set; }
		public GroupItemPage(GroupItemViewModel vm)
		{
			InitializeComponent ();

            string[] units = App.UnitsList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in units)
            {
                UnitsPicker.Items.Add(item);
            }

            ViewModel = vm; 
            BindingContext = ViewModel;

		}
        private void UnitsPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            ViewModel.Units = UnitsPicker.Items[UnitsPicker.SelectedIndex];
        }
    }
}