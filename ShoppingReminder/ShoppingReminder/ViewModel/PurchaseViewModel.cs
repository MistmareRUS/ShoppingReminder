﻿using ShoppingReminder.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ShoppingReminder.ViewModel
{
    public class PurchaseViewModel
    {
        public Purchase Purchase { get; private set; }
        public PurchaseListViewModel ListVM { get; set; }
        public string VaiableName;

   
        public PurchaseViewModel(PurchaseListViewModel listVM)
        {
            Purchase = new Purchase();
        }

        public string Name
        {
            get
            {
                return Purchase.Name;
            }
            set
            {
                Purchase.Name = value;
            }
        }
        public double Count
        {
            get
            {
                return Purchase.Count;
            }
            set
            {
                Purchase.Count = value;
            }
        }
        public string Units
        {
            get
            {
                return Purchase.Units;
            }
            set
            {

                Purchase.Units = value;
            }
        }
        public bool Completed
        {
            get
            {
                return Purchase.Completed;
            }
            set
            {
                Purchase.Completed = value;
            }
        }

        public bool isValid
        {
            get
            {
                return ((!string.IsNullOrWhiteSpace(Name)));
                 
            }
        }
    }
}
