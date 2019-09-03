using SQLite;
using System;
using System.Collections.Generic;

namespace ShoppingReminder.Model
{
    [Serializable]
    public class ListOfPurchase
    {
        public int Id { get; set; }
        public List<Purchase> PurchasesList { get; set; }
        public DateTime Date { get; set; }
        public string Check { get; set; }

        public string ShopName { get; set; }
    }
    [Table("History")]
    [Serializable]
    public class SerializedHistoryItem
    {        
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public byte[] PurchasesList { get; set; }
        public DateTime Date { get; set; }
        public string Check { get; set; }        
        public string ShopName { get; set; }
    }

}
