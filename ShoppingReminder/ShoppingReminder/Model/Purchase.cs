using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShoppingReminder.Model
{
    [Table("Purchases")]
    [Serializable]
    public class Purchase
    {
        [PrimaryKey, AutoIncrement,Column("_id")]
        public int Id { get; set; }

        public string Name { get; set; }
        public double Count { get; set; }
        public string Units { get; set; }
        public bool Completed { get; set; }
    }

}
