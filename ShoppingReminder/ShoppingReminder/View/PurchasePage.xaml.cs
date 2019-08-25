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
	public partial class PurchasePage : ContentView
	{
        public PurchaseViewModel ViewModel { get; private set; }
        
		public PurchasePage (PurchaseViewModel vm)
		{
			InitializeComponent ();
            UnitsPicker.Items.Add("Шт");//Обязательно выше привязки. Иначе не отображается в имеющихся элементах.
            UnitsPicker.Items.Add("Кг");
            UnitsPicker.Items.Add("Упак");
            UnitsPicker.Items.Add("л");

            ViewModel = vm;
            BindingContext = ViewModel;

        }
        private void UnitsPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            ViewModel.Units = UnitsPicker.Items[UnitsPicker.SelectedIndex];            
        }
    }
}