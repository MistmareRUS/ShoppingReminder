using ShoppingReminder.Model;
using ShoppingReminder.View;

namespace ShoppingReminder.ViewModel
{
    public class GroupItemViewModel
    {
        public Purchase Purchase { get; set; }
        public GroupViewModel ListVM { get; set; }
        public string VaiableName;
        public GroupItemViewModel()
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
        public int Id
        {
            get
            {
                return Purchase.Id;
            }
            set
            {
                Purchase.Id = value;
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
