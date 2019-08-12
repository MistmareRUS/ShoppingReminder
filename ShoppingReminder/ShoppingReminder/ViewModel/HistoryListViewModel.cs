using ShoppingReminder.Model;
using ShoppingReminder.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace ShoppingReminder.ViewModel
{
    public class HistoryListViewModel
    {
        public List<HistoryViewModel> PurchaseList => App.HistoryOfPurchase;
        public MainPage Main;
        public HistoryListViewModel(MainPage mp)
        {
            Main = mp;
            BackCommand = new Command(Back);
            DeleteHistoryItemCommand = new Command(DeleteHistoryItem);
            ClearHistoryCommand = new Command(ClearHistory);

            foreach (var item in App.HistoryOfPurchase)
            {
                item.ListVM = this;
            }

            Back();
        }

        public ICommand DeleteHistoryItemCommand { get; private set; }
        public ICommand BackCommand { get; private set; }
        public ICommand ClearHistoryCommand { get; protected set; }

        public void Back()
        {
            App.HistoryOfPurchase = App.Database.GetHistoryItems();
            Main.HistoryStackLayout.Children.Clear();
            Main.HistoryStackLayout.Children.Add(new HistoryListPage(this));
        }
        private async void ClearHistory()
        {
            var confirm = await Main.DisplayAlert("Внимание", "Очистить сохраненную историю?", "Да", "Нет");
            if (!confirm)
            {
                return;
            }
            App.Database.ClearHistory();
            App.HistoryOfPurchase = App.Database.GetHistoryItems();
            Back();
        }

        private async void DeleteHistoryItem(object obj)
        {
            var confirm = await Main.DisplayAlert("Внимание!", "Удалить данный список из истории покупок?", "Да", "Нет");
            if (!confirm)
            {
                return;
            }            
            var result=App.Database.DeleteHistoryItem((int)obj);
            if (result == 0)
            {
                await Main.DisplayAlert("Внимание!","Произошла ошибка при удалении","ОК");
            }
            Back();

        }


        HistoryViewModel selectedPurchaseList;
        public HistoryViewModel SelectedPurchaseList
        {
            get
            {
                return selectedPurchaseList;
            }
            set
            {
                if (selectedPurchaseList != value)
                {
                    HistoryViewModel tempHistory = value;
                    tempHistory.ListVM = new HistoryListViewModel(Main);
                    selectedPurchaseList = null;
                    Main.HistoryStackLayout.Children.Clear();
                    Main.HistoryStackLayout.Children.Add(new HistoryItemPage(tempHistory));
                }
            }
        }        
    }    
}
