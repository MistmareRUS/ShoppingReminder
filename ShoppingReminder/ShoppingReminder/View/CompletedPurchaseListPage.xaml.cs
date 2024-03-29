﻿using ShoppingReminder.ViewModel;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingReminder.View
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CompletedPurchaseListPage : ContentView
    {
        public CompletedPurchaseListPage(PurchaseListViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;

            purchaseList.ItemSelected += (object sender, SelectedItemChangedEventArgs e) => {
                if (e.SelectedItem == null) return;
                ((ListView)sender).SelectedItem = null;
            };

            if (App.CurrentPurchases.Count < 1||!App.CurrentPurchases.Any(p=>p.Completed))
            {               
                ((Tab)(vm.Main.CompletedPurchasesStackLayout.Parent.Parent.Parent.Parent)).IsEnabled = false;
            }
        }
    }
}