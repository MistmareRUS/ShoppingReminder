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
        public IEnumerable<string> PhotoPath
        {
            get
            {
                return ListOfPurchase.Check.Split('&');
            }
        }
        public HistoryViewModel()
        {
            ListOfPurchase = new ListOfPurchase();
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
