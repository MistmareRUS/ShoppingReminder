using ShoppingReminder.Model;
using ShoppingReminder.Repository;
using ShoppingReminder.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace ShoppingReminder
{
    public partial class App : Application
    {
        public const string DATABASE_NAME = "Purchase.db";
        public static PurchaseRepository database;
        public static PurchaseRepository Database
        {
            get
            {
                if (database == null)
                {
                    database = new PurchaseRepository(DATABASE_NAME);
                }
                return database;
            }
        }

        public static List<PurchaseViewModel> CurrentPurchases { get; set; }
        public static List<PlanViewModel> Plans { get; set; }
        public static List<HistoryViewModel> HistoryOfPurchase { get; set; }
        MainPage MP;

        public App()
        {
            InitializeComponent();
            CurrentPurchases = new List<PurchaseViewModel>();
            Plans = new List<PlanViewModel>();

            LoadCurrentPurchasesFromDB();
            LoadPlansFromDB();
            HistoryOfPurchase = Database.GetHistoryItems();
            MainPage = MP = new MainPage();
        }

        protected override void OnStart()
        {

        }

        protected override void OnSleep()
        {
            SaveCurrentPurchasesToDB();            
        }
        protected override void OnResume()
        {
            LoadCurrentPurchasesFromDB();
            foreach (var item in CurrentPurchases)
            {
                item.ListVM = MP.activePurchases;
            }
            MP.activePurchases.Back();
        }
        void SaveCurrentPurchasesToDB()
        {
            Database.ClearPurchases();
            foreach (var item in CurrentPurchases)
            {
                Purchase temp = new Purchase()
                {
                    Name = item.Name,
                    Count = item.Count,
                    Units = item.Units,
                    Completed = item.Completed

                };
                Database.SavePurchaseItem(temp);
            }
            CurrentPurchases.Clear();
        }
        void  LoadCurrentPurchasesFromDB()
        {
            var purchases = Database.GetPurchaseItems();
            CurrentPurchases.Clear();
            foreach (var item in purchases)
            {
                PurchaseViewModel temp = new PurchaseViewModel()
                {
                    Name = item.Name,
                    Count = item.Count,
                    Units = item.Units,
                    Completed = item.Completed
                };
                CurrentPurchases.Add(temp);
            }
        }
        public static void LoadPlansFromDB()
        {
            var plans = Database.GetPlanItems();
            Plans.Clear();
            foreach (var item in plans)
            {
                PlanViewModel temp = new PlanViewModel()
                {
                    Name = item.Name,
                    Id=item.Id
                };
                Plans.Add(temp);
            }
        }
        
    }
}
