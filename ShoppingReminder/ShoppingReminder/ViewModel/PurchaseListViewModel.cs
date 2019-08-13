using ShoppingReminder.Model;
using ShoppingReminder.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace ShoppingReminder.ViewModel
{
    public class PurchaseListViewModel
    {
        public IEnumerable<PurchaseViewModel> Purchases => App.CurrentPurchases.Where(p => !p.Completed);
        public IEnumerable<PurchaseViewModel> CompletedPurchases => App.CurrentPurchases.Where(p => p.Completed);
        public MainPage Main;

        public PurchaseListViewModel(MainPage mp)
        {
            Main = mp;
            CreatePurchaseCommand = new Command(CreatePurchase);
            DeletePurchaseCommand = new Command(DeletePurchase);
            SavePurchaseCommand = new Command(SavePurchase);
            MarkAsCompletedPurchaseCommand = new Command(MarkAsCompletedPurchase);
            BackCommand = new Command(Back);
            CompletePurchaseCommand = new Command(CompletePurchase);
            ClearPurchaseCommand = new Command(ClearPurchase);
            UpPurchaseCommand = new Command(UpPurchase);
            DownPurchaseCommand = new Command(DownPurchase);
            ToPlansCommand = new Command(ToPlans);

            foreach (var item in App.CurrentPurchases)
            {
                item.ListVM = this;
            }

            Back();
        }

        private async void ToPlans(object obj)
        {
            var confirm = await Main.DisplayAlert("Внимание", "Переместить в планы?", "Да", "Нет");
            if (!confirm)
            {
                return;
            }
            PurchaseViewModel temp = obj as PurchaseViewModel;
            if (temp != null)
            {
                App.Database.SavePlanItem(new Plan() { Name = temp.Name });
                Main.plan.Back();
                App.CurrentPurchases.Remove(temp);
                Back();
            }            
        }

        private void UpPurchase(object obj)
        {
            var listIndex = App.CurrentPurchases.IndexOf(obj as PurchaseViewModel);
            for (int i = listIndex-1; i >= 0; i--)
            {
                if (!App.CurrentPurchases[i].Completed)
                {
                    Swap(listIndex,i);
                    break;
                }
            }
            Back();
        }
        private  void DownPurchase(object obj)
        {
            var listIndex = App.CurrentPurchases.IndexOf(obj as PurchaseViewModel);
            for (int i = listIndex + 1; i <App.CurrentPurchases.Count; i++)
            {
                if (!App.CurrentPurchases[i].Completed)
                {
                    Swap(listIndex, i);
                    break;
                }
            }
            Back();

        }
        private void Swap(int a,int b)
        {
            PurchaseViewModel temp;
            temp = App.CurrentPurchases[a];
            App.CurrentPurchases[a] = App.CurrentPurchases[b];
            App.CurrentPurchases[b] = temp;
        }


        public ICommand CreatePurchaseCommand { get; protected set; }
        public ICommand DeletePurchaseCommand { get; protected set; }
        public ICommand SavePurchaseCommand { get; protected set; }
        public ICommand MarkAsCompletedPurchaseCommand { get; protected set; }
        public ICommand BackCommand { get; protected set; }
        public ICommand CompletePurchaseCommand { get; protected set; }
        public ICommand ClearPurchaseCommand { get; protected set; }
        public ICommand UpPurchaseCommand { get; protected set; }
        public ICommand DownPurchaseCommand { get; protected set; }
        public ICommand ToPlansCommand { get; protected set; }
        //добавить/открыть фотки+
        public void Back()
        {
            Main.CurrentPurchasesStackLayout.Children.Clear();
            Main.CompletedPurchasesStackLayout.Children.Clear();
            Main.CurrentPurchasesStackLayout.Children.Add(new PurchaseListPage(this));
            Main.CompletedPurchasesStackLayout.Children.Add(new CompletedPurchaseListPage(this));
        }

        private void CreatePurchase()
        {
            Main.CurrentPurchasesStackLayout.Children.Clear();
            var creatingPage = new PurchasePage(new PurchaseViewModel()
            {
                ListVM=this
            });
            Main.CurrentPurchasesStackLayout.Children.Add(creatingPage);
        }
        private async void ClearPurchase()
        {
            var confirm = await Main.DisplayAlert("Внимание", "Очистить список покупок?", "Да", "Нет");
            if (!confirm)
            {
                return;
            }
            App.CurrentPurchases = new List<PurchaseViewModel>();
            Back();
        }
        private async void CompletePurchase()
        {
            var confirm = await Main.DisplayAlert("Внимание", "Завершить покупку?", "Да", "Нет");
            if (!confirm)
            {
                return;
            }
            var currentList = new ListOfPurchase { PurchasesList = new List<Purchase>(), Date = DateTime.Now, Check = new byte[0] };//TODO:пустой массив?
            foreach (var item in App.CurrentPurchases)
            {
                //TODO:все или только завершенные?
                var temp = new Purchase();
                temp.Name = item.Name;
                temp.Count = item.Count;
                temp.Units = item.Units;                
                currentList.PurchasesList.Add(temp);
            }
            App.Database.SaveHistoryItem(currentList);            
            App.CurrentPurchases = new List<PurchaseViewModel>();
            Main.history.Back();
            Back();
        }
        private void MarkAsCompletedPurchase(object obj)
        {
            PurchaseViewModel purchase = obj as PurchaseViewModel;
            if (purchase != null && purchase.isValid)
            {
                App.CurrentPurchases.FirstOrDefault(p => p.Name == purchase.Name).Completed = true;
            }
            Back();
        }
        private void DeletePurchase(object obj)
        {
            PurchaseViewModel purchase = obj as PurchaseViewModel;
            if (purchase != null)
            {
                App.CurrentPurchases.Remove(purchase);
            }
            Back();
        }

        private void SavePurchase(object obj)
        {
            PurchaseViewModel purchase = obj as PurchaseViewModel;
            if (purchase != null && purchase.isValid)
            {
                if (string.IsNullOrEmpty(purchase.VaiableName))
                {
                    App.CurrentPurchases.Add(purchase);
                }
                else
                {
                    var temp=App.CurrentPurchases.FirstOrDefault(p => p.Name == purchase.VaiableName);
                    //temp = purchase;//TODO: зачем?
                }
            }
            Back();
        }
        
        PurchaseViewModel selectedPurchase; 
        public PurchaseViewModel SelectedPurchase
        {
            get
            {
                return selectedPurchase;
            }
            set
            {
                if (selectedPurchase != value)
                {
                    PurchaseViewModel tempPurchase = value;
                    tempPurchase.VaiableName = tempPurchase.Name;
                    selectedPurchase = null;
                    Main.CurrentPurchasesStackLayout.Children.Clear();
                    Main.CurrentPurchasesStackLayout.Children.Add(new PurchasePage(tempPurchase));
                }
            }
        }       
    }
}
