using ShoppingReminder.Model;
using ShoppingReminder.View;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace ShoppingReminder.ViewModel
{
    public class PlanListViewModel
    {
        public IEnumerable<PlanViewModel> PlanList => App.Plans;
        public MainPage Main;
        public PlanListViewModel(MainPage mp)
        {
            Main = mp;
            AddToCurrentPurchaseCommand = new Command(AddToCurrentPurchase);
            DeletePlanItemCommand = new Command(DeletePlanItem);
            BackCommand = new Command(Back);
            CreateCommand = new Command(Create);
            foreach (var item in App.Plans)
            {
                item.ListVM = this;
            }
            Back();
        }
        public ICommand AddToCurrentPurchaseCommand;
        public ICommand DeletePlanItemCommand;
        public ICommand BackCommand;
        public ICommand CreateCommand;

        public void Back()
        {
            Main.PlanStackLayout.Children.Clear();
            Main.PlanStackLayout.Children.Add(new PlanListPage(this));
        }

        private void Create(object obj)
        {
            Main.DisplayAlert("", "типо создать...", "X");
        }

        private void DeletePlanItem(object obj)
        {
            Main.DisplayAlert("", "типо удалить...", "X");
        }

        private void AddToCurrentPurchase(object obj)
        {
            Main.DisplayAlert("", "типо добавить...", "X");
        }

        PlanViewModel selectedPurchase;
        public PlanViewModel SelectedPlan
        {
            get
            {
                return selectedPurchase;
            }
            set
            {
                if (selectedPurchase != value)
                {
                    selectedPurchase = null;
                }
            }
        }

    }
}
