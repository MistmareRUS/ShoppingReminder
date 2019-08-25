using ShoppingReminder.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShoppingReminder.ViewModel
{
    public class HistoryViewModel
    {
        public ListOfPurchase ListOfPurchase { get; set; }
        public HistoryListViewModel ListVM { get; set; }

        public HistoryViewModel()
        {
            ListOfPurchase = new ListOfPurchase();
        }

        public string Check
        {
            get
            {
                return ListOfPurchase.Check;
            }
            set
            {
                ListOfPurchase.Check = value;
            }
        }
        public string ShopName
        {
            get
            {
                return ListOfPurchase.ShopName;
            }
            set
            {
                ListOfPurchase.ShopName = value;
            }
        }
        public DateTime Date
        {
            get
            {
                return ListOfPurchase.Date;
            }
            set
            {
                ListOfPurchase.Date = value;
            }
        }
        public List<Purchase> PurchasesList
        {
            get
            {
                return ListOfPurchase.PurchasesList;
            }
            set
            {
                ListOfPurchase.PurchasesList = value;
            }
        }        
        public int Id
        {
            get
            {
                return ListOfPurchase.Id;
            }
            set
            {
                ListOfPurchase.Id = value;
            }
        }
    }
}
