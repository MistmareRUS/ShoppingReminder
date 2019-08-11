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
        public MainPage Main;
        public ListOfPurchase PurchaseList { get; set; }
        public HistoryListViewModel(MainPage mp)
        {
            Main = mp;
            BackCommand = new Command(Back);
            DeleteHistoryItemCommand = new Command(DeleteHistoryItem);
            ClearHistoryCommand = new Command(ClearHistory);

            Back();
        }

        private async void ClearHistory()
        {
            var confirm = await Main.DisplayAlert("Внимание", "Очистить созраненную историю?", "Да", "Нет");
            if (!confirm)
            {
                return;
            }
            App.HistoryOfPurchase = new List<ListOfPurchase>();
            App.Database.ClearHistory();
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

        public ICommand DeleteHistoryItemCommand { get; private set; }
        public ICommand BackCommand { get; private set; }
        public ICommand ClearHistoryCommand { get; protected set; }

        public void Back()
        {
            App.HistoryOfPurchase = App.Database.GetHistoryItems();
            Main.HistoryStackLayout.Children.Clear();
            Main.HistoryStackLayout.Children.Add(new HistoryListPage(this));
        }
        ListOfPurchase selectedPurchaseList;
        public ListOfPurchase SelectedPurchaseList
        {
            get
            {
                return selectedPurchaseList;
            }
            set
            {
                if (selectedPurchaseList != value)
                {
                    PurchaseList = value;
                    selectedPurchaseList = null;
                    Main.HistoryStackLayout.Children.Clear();
                    Main.HistoryStackLayout.Children.Add(new HistoryItemPage(this));
                }
            }
        }
    }    
}
