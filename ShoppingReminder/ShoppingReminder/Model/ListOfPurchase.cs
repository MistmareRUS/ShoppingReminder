using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace ShoppingReminder.Model
{
    [Serializable]
    public class ListOfPurchase
    {
        public int Id { get; set; }
        public List<Purchase> PurchasesList { get; set; }
        public DateTime Date { get; set; }
        public byte[] Check { get; set; }
    }
    [Table("History")]
    [Serializable]
    public class SerializedHistoryItem
    {        
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public byte[] PurchasesList { get; set; }
        public DateTime Date { get; set; }
        public byte[] Check { get; set; }        
    }

}
