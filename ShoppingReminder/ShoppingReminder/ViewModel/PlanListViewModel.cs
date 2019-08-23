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
        private ICommand addToCurrentPurchaseCommand;
        public ICommand AddToCurrentPurchaseCommand
        {
            get
            {
                return this.addToCurrentPurchaseCommand;
            }
            set
            {
                this.addToCurrentPurchaseCommand = value; 
            }
        }
        private ICommand deletePlanItemCommand;
        public ICommand DeletePlanItemCommand
        {
            get
            {
                return this.deletePlanItemCommand;
            }
            set
            {
                this.deletePlanItemCommand = value;
            }
        }
        private ICommand backCommand;
        public ICommand BackCommand
        {
            get
            {
                return this.backCommand;
            }
            set
            {
                this.backCommand = value;
            }
        }
        private ICommand createCommand;
        public ICommand CreateCommand
        {
            get
            {
                return this.createCommand;
            }
            set
            {
                this.createCommand = value;
            }
        }

        public void Back()
        {
            App.LoadPlansFromDB();
            foreach (var item in App.Plans)
            {
                item.ListVM = this;
            }
            Main.PlanStackLayout.Children.Clear();
            Main.PlanStackLayout.Children.Add(new PlanListPage(this));
        }

        private void Create(object obj)
        {
            var text = (Entry)obj;
            if (string.IsNullOrEmpty(text.Text))
            {
                return;
            }
            App.Database.SavePlanItem(new Plan(){ Name = text.Text });
            Back();
        }

        private async  void DeletePlanItem(object obj)
        {
            var confirm = await Main.DisplayAlert("Внимание!", "Удалить этот предмет из списка?", "Да", "Нет");
            if (!confirm)
            {
                return;
            }
            PlanViewModel temp = (PlanViewModel)obj;
            App.Database.DeletePlanItem(temp.Id);
            Back();
        }

        private void AddToCurrentPurchase(object obj)
        {
            PlanViewModel temp = (PlanViewModel)obj;
            PurchaseViewModel purchase = new PurchaseViewModel()
            {
                Name = temp.Name,
                ListVM = Main.activePurchases
            };
            App.CurrentPurchases.Add(purchase);
            App.Database.DeletePlanItem(temp.Id);
            Main.activePurchases.Back();
            Main.plan.Back();

        }
    }
}
