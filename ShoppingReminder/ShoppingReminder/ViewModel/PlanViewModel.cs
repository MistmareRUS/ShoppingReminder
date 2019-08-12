using ShoppingReminder.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShoppingReminder.ViewModel
{
    public class PlanViewModel
    {
        public Plan Plan { get; private set; }
        public PlanListViewModel ListVM { get; set; }


        public PlanViewModel()
        {
            Plan = new Plan();
        }

        public string Name
        {
            get
            {
                return Plan.Name;
            }
            set
            {
                Plan.Name = value;
            }
        }
        
        public int Id
        {
            get
            {
                return Plan.Id;
            }
            set
            {
                Plan.Id = value;
            }
        }
        public bool IsValid
        {
            get { return (!string.IsNullOrWhiteSpace(Name)); }
        }
    }
}
