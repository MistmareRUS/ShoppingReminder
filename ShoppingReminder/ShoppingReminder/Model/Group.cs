using ShoppingReminder.ViewModel;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShoppingReminder.Model
{
    public class Group
    {
        public int Id { get; set; }
        public List<Purchase> PurchasesList { get; set; }
        public string Name { get; set; }
        public string Progress
        {
            get
            {
                if (PurchasesList == null||PurchasesList.Count<1)
                {
                    return "-/-";
                }
                else
                {
                    var count = PurchasesList.Count;
                    var complete = PurchasesList.Count(p => p.Completed);
                    return $" {complete} / {count} ";
                }
            }
        }
    }
    [Table("Groups")]
    [Serializable]
    public class SerializedGroup
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public byte[] PurchasesList { get; set; }
        public string Name { get; set; }
    }
}
