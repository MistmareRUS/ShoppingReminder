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
        public static List<ListOfPurchase> HistoryOfPurchase { get; set; }
        public App()
        {
            Debug.WriteLine("к-тор!!!!!!!!!!!!!!!!!!!              !!!!!!!!!!!!!!!!!!!!!!!!!");
            InitializeComponent();
            CurrentPurchases = new List<PurchaseViewModel>();
            HistoryOfPurchase = Database.GetHistoryItems();

            LoadCurrentPurchasesFromDB();
            
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            Debug.WriteLine("старт!!!!!!!!!!!!!!!!!!!              !!!!!!!!!!!!!!!!!!!!!!!!!");
        }

        protected override void OnSleep()
        {
            Debug.WriteLine("слип!!!!!!!!!!!!!!!!!!!              !!!!!!!!!!!!!!!!!!!!!!!!!");
            SaveCurrentPurchasesToDB();
            
        }
        protected override void OnResume()
        {
            Debug.WriteLine("ресум!!!!!!!!!!!!!!!!!!!              !!!!!!!!!!!!!!!!!!!!!!!!!");
            

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
            CurrentPurchases = new List<PurchaseViewModel>();
        }
        void  LoadCurrentPurchasesFromDB()
        {
            var purchases = Database.GetPurchaseItems();
            CurrentPurchases.Clear();
            foreach (var item in purchases)
            {
                PurchaseViewModel temp = new PurchaseViewModel(null)
                {
                    Name = item.Name,
                    Count = item.Count,
                    Units = item.Units,
                    Completed = item.Completed
                };
                CurrentPurchases.Add(temp);
            }
        }
        
    }
}
