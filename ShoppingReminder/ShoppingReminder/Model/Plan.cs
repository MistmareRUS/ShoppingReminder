using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShoppingReminder.Model
{
    [Table("Plans")]
    public class Plan
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
