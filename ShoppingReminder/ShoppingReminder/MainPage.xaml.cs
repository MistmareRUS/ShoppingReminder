﻿using ShoppingReminder.Model;
using ShoppingReminder.View;
using ShoppingReminder.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;


namespace ShoppingReminder
{
    public partial class MainPage : Shell
    {
        //свойства для доступа к элементам разметки
        public StackLayout CurrentPurchasesStackLayout { get; set; }
        public StackLayout CompletedPurchasesStackLayout { get; set; }
        public StackLayout HistoryStackLayout { get; set; }
        public PurchaseListViewModel activePurchases;
        public HistoryListViewModel history;

        public MainPage()
        {
            InitializeComponent();
            CurrentPurchasesStackLayout = CurrentStack;
            CompletedPurchasesStackLayout = CompletetCurrentStack;
            HistoryStackLayout = HistoryStack;

            activePurchases = new PurchaseListViewModel(this);
            history = new HistoryListViewModel(this);
            
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            App.Database.ClearPurchases();
            App.CurrentPurchases = new List<PurchaseViewModel>();
            activePurchases.Back();
        }

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            App.Database.ClearHistory();
            App.HistoryOfPurchase = new List<ListOfPurchase>();
            history.Back();
        }
    }
}
