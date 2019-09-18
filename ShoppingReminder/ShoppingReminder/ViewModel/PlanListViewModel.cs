﻿using ShoppingReminder.Model;
using ShoppingReminder.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace ShoppingReminder.ViewModel
{
    public class PlanListViewModel
    {
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
            if (string.IsNullOrEmpty(text.Text)|| PlanList.Any(p => p.Name.ToLower() == text.Text.ToLower()))
            {
                Main.DisplayAlert("Внимание!", "Введите название, которого еще нет в списке.", "Ок");
                return;
            }
            App.Database.SavePlanItem(new Plan(){ Name = text.Text });
            Back();
        }
        private async  void DeletePlanItem(object obj)
        {
            var confirm = await Main.DisplayAlert("Внимание!", "Удалить этот предмет из списка?", "Да", "Нет");
            if (confirm)
            {
                PlanViewModel temp = (PlanViewModel)obj;
                App.Database.DeletePlanItem(temp.Id);
                Back();
            }
           
        }
        private async void AddToCurrentPurchase(object obj)
        {
            PlanViewModel tempPlan = (PlanViewModel)obj;
            var dirs =Main.groups.GroupsList.Where(g => g.Name != "Без названия").Select(g => g.Name).ToArray();
            string[] directions = new string[dirs.Length + 2];
            directions[0] = "В активный список";
            for (int i = 1; i <= dirs.Length; i++)
            {
                directions[i] = dirs[i - 1];
            }
            directions[directions.Length - 1] = "Поделиться";
            var direct = await Main.DisplayActionSheet($"Переместить \"{tempPlan.Name}\" в ...", "Отмена", null, directions);                       
            if (direct == directions[0])//к активным
            {
                PurchaseViewModel purchase = new PurchaseViewModel() { Name = tempPlan.Name,ListVM = Main.activePurchases };

                if (App.CurrentPurchases.Any(p => p.Name.ToLower() == purchase.Name.ToLower()))
                {
                    await Main.DisplayAlert("Внимание!", $"\"{tempPlan.Name}\" уже имеется в списке.", "Ok");
                    return;
                }
                App.CurrentPurchases.Add(purchase);
                Main.activePurchases.Back();
                App.Database.DeletePlanItem(tempPlan.Id);
                Back();
                if (App.CurrentPurchases.Any(p => p.Completed))
                {
                    ((Tab)(Main.CompletedPurchasesStackLayout.Parent.Parent.Parent.Parent)).IsEnabled = true;
                }
                await Main.DisplayAlert("", $"\"{tempPlan.Name}\" добавлено к активному списку", "Ок");
            }
            else if(direct == directions[directions.Length - 1])//отправка по сети
            {
                try
                {
                    Main.SharePurchases(new Purchase[] { new Purchase { Name=tempPlan.Name} });
                }
                catch
                {
                    await Main.DisplayAlert("Внимание!", "Кажется, ваше устройство не поддерживает эту функцию.", "Ок");
                    return;
                }
                bool clear = await Main.DisplayAlert("Внимание!", $"Удалить \"{tempPlan.Name}\" из списка?", "Ок", "Отмена");
                if (clear)
                {
                    App.Database.DeletePlanItem(tempPlan.Id);
                    Back();
                }
            }
            else if(directions.Any(d => d == direct))//в группу
            {
                var grIndex = Main.groups.GroupsList.IndexOf(Main.groups.GroupsList.FirstOrDefault(g => g.Name == direct));
                if (Main.groups.GroupsList[grIndex].PurchasesList == null)
                {
                    Main.groups.GroupsList[grIndex].PurchasesList = new List<Purchase>();
                }
                else if (Main.groups.GroupsList[grIndex].PurchasesList.Any(p => p.Name.ToLower() == tempPlan.Name.ToLower()))
                {
                    await Main.DisplayAlert("Внимание!", $"\"{tempPlan.Name}\" уже есть в списке.", "Ok");
                    return;
                }
                Main.groups.GroupsList[grIndex].PurchasesList.Add(new Purchase() { Name = tempPlan.Name });
                App.Database.SaveGroupItem(Main.groups.GroupsList[grIndex].Group);
                App.Database.DeletePlanItem(tempPlan.Id);
                Back();
                Main.groups.Back();
            }
        }
    }
}
