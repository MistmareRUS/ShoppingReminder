using ShoppingReminder.Model;
using ShoppingReminder.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace ShoppingReminder.ViewModel
{
    public class GroupViewModel
    {
        private ICommand saveGroupItemCommand;
        public ICommand SaveGroupItemCommand
        {
            get
            {
                return this.saveGroupItemCommand;
            }
            set
            {
                this.saveGroupItemCommand = value;
            }
        }
        private ICommand createGroupItemCommand;
        public ICommand CreateGroupItemCommand
        {
            get
            {
                return this.createGroupItemCommand;
            }
            set
            {
                this.createGroupItemCommand = value;
            }
        }
        private ICommand backCommand;
        public ICommand BackCommand
        {
            get
            {
                return this.backCommand;
            }
            set
            {
                this.backCommand = value;
            }
        }
        private ICommand clearGroupCommand;
        public ICommand ClearGroupCommand
        {
            get
            {
                return this.clearGroupCommand;
            }
            set
            {
                this.clearGroupCommand = value;
            }
        }
        private ICommand deleteGroupItemCommand;
        public ICommand DeleteGroupItemCommand
        {
            get
            {
                return this.deleteGroupItemCommand;
            }
            private set
            {
                this.deleteGroupItemCommand = value;
            }
        }
        private ICommand moveItemCommand;
        public ICommand MoveItemCommand
        {
            get
            {
                return this.moveItemCommand;
            }
            set
            {
                this.moveItemCommand = value;
            }
        }

        public bool needToSave;//флаг для новой группы, в которой нет ни одного товара.
        public Group Group { get; set; }
        private List<GroupItemViewModel> purchases;
        public List<GroupItemViewModel> Purchases
        {
            get
            {                
                return purchases;
            }
            set
            {
                purchases = value;
            }
        }
        public GroupListViewModel ListVM { get; set; }
        public GroupViewModel(GroupListViewModel glvm)
        {
            ListVM = glvm;
            SaveGroupItemCommand = new Command(SaveGroupItem);
            CreateGroupItemCommand = new Command(CreateGroupItem);
            ClearGroupCommand = new Command(ClearGroup);
            DeleteGroupItemCommand = new Command(DeleteGroupItem);
            MoveItemCommand = new Command(MoveItem);
        }
        
        private  async void ClearGroup(object obj)
        {
            var confirm = await ListVM.Main.DisplayAlert("Внимание!", "Очистить текущую группу покупок", "Да", "Нет");
            if (confirm)
            {
                GroupViewModel groupVM = obj as GroupViewModel;
                ListVM.ActiveGroup = groupVM;
                ListVM.ActiveGroup.PurchasesList=new List<Purchase>();
                ListVM.ActiveGroup.Purchases = new List<GroupItemViewModel>();
                App.Database.SaveGroupItem(ListVM.ActiveGroup.Group);
                ListVM.BackToList();
            }
            
        }
        private async void MoveItem(object obj)
        {
            var item = (GroupItemViewModel)obj;
            if (string.IsNullOrEmpty(item.Name))
            {
                await ListVM.Main.DisplayAlert("Внимание!", "Заполните название.", "Ок");
                return;
            }
            var dirs = ListVM.GroupsList.Where(g => g.Name != ListVM.ActiveGroup.Name && g.Name != "Без названия").Select(g => g.Name).ToArray();
            string[] directions = new string[dirs.Length + 3];
            directions[0] = "В активный список";
            for (int i = 1; i <= dirs.Length; i++)
            {
                directions[i] = dirs[i - 1];
            }
            directions[directions.Length - 2] = "В планы";
            directions[directions.Length - 1] = "Отправить";

            var direct = await ListVM.Main.DisplayActionSheet($"Переместить \"{item.Name}\" в ...", "Отмена", null, directions);
            if (direct == directions[directions.Length - 2])//в планы
            {
                if (!ListVM.Main.plan.PlanList.Any(p=>p.Name.ToLower()==item.Name.ToLower()))
                {
                    App.Database.SavePlanItem(new Plan() { Name = item.Name });
                    ListVM.Main.plan.Back();
                    DeleteItem(item);
                    await ListVM.Main.DisplayAlert("", "Перемещено в планы", "Ок");
                }
                else
                {
                    await ListVM.Main.DisplayAlert("", $"{item.Name} уже есть в списке планов.", "Ок");
                }
            }
            else if (direct == directions[directions.Length - 1])//отправка по сети
            {
                try
                {
                    ListVM.Main.SharePurchases(new Purchase[] { new Purchase {Name=item.Name,Count=item.Count,Units=item.Units }  });
                }
                catch
                {
                    await ListVM.Main.DisplayAlert("Внимание!", "Кажется, ваше устройство не поддерживает эту функцию.", "Ок");
                    return;
                }
                bool clear = await ListVM.Main.DisplayAlert("Внимание!", $"Удалить \"{item.Name}\" из списка?", "Ок", "Отмена");
                if (clear)
                {
                    DeleteItem(item);
                }
            }
            else if (direct == directions[0])//к активным
            {
                PurchaseViewModel purchase = new PurchaseViewModel() { Name = item.Name, Count = item.Count, Units = item.Units, Completed = item.Completed,ListVM=ListVM.Main.activePurchases };

                if (App.CurrentPurchases.Any(p => p.Name.ToLower() == purchase.Name.ToLower()))
                {
                    await ListVM.Main.DisplayAlert("Внимание!", "Такой элемент уже имеется в списке.", "Ok");
                    return;
                }
                App.CurrentPurchases.Add(purchase);
                ListVM.Main.activePurchases.Back();
                
                DeleteItem(item);

                if (App.CurrentPurchases.Any(p => p.Completed))
                {
                    ((Tab)(ListVM.Main.CompletedPurchasesStackLayout.Parent.Parent.Parent.Parent)).IsEnabled = true;
                }
                await ListVM.Main.DisplayAlert("", "Добавлено к активному списку", "Ок");
            }
            else if(directions.Any(d=>d==direct))
            {
                var grIndex = ListVM.GroupsList.IndexOf(ListVM.GroupsList.FirstOrDefault(g => g.Name == direct));
                if (ListVM.GroupsList[grIndex].PurchasesList==null)
                {
                    ListVM.GroupsList[grIndex].PurchasesList = new List<Purchase>();
                }
                else if (ListVM.GroupsList[grIndex].PurchasesList. Any(p => p.Name.ToLower() == item.Name.ToLower())    )
                {
                    await ListVM.Main.DisplayAlert("Внимание!", "Такой элемент уже имеется в списке.", "Ok");
                    return;
                }
                ListVM.GroupsList[grIndex].PurchasesList.Add(new Purchase() { Name = item.Name, Count = item.Count, Completed = item.Completed, Units = item.Units });
                App.Database.SaveGroupItem(ListVM.GroupsList[grIndex].Group);
                DeleteItem(item);
                ListVM.Main.DisplayAlert("", $"Перемещено в группу {direct}", "Ок");
                ListVM.BackToList();
            }
        }

        private async void DeleteGroupItem(object obj)
        {
            var confirm = await ListVM.Main.DisplayAlert("Внимание!", "Удалить из списка?", "Да", "Нет");
            if (confirm)
            {
                var item = (GroupItemViewModel)obj;
                DeleteItem(item);
            }
            
            
        }
        void DeleteItem(GroupItemViewModel item)
        {
            var purch = ListVM.ActiveGroup.Group.PurchasesList!=null? ListVM.ActiveGroup.Group.PurchasesList.FirstOrDefault(p => p.Name == item.Name):null;
            var givm = ListVM.ActiveGroup.Purchases!=null? ListVM.ActiveGroup.Purchases.FirstOrDefault(p => p.Name == item.Name):null;
            if (purch!=null&&givm!=null)
            {
                ListVM.ActiveGroup.Group.PurchasesList.Remove(purch);
                ListVM.ActiveGroup.Purchases.Remove(givm);
                App.Database.SaveGroupItem(ListVM.ActiveGroup.Group);
            }
            ListVM.BackToList();
        }


        public void NameChange(string newName)
        {
            ListVM.ActiveGroup.Name = newName;
            if (ListVM.ActiveGroup.Id != 0)
            {
                App.Database.SaveGroupItem(ListVM.ActiveGroup.Group);
            }
            else
            {

                App.Database.SaveGroupItem(ListVM.ActiveGroup.Group);
                var allGroups= App.Database.GetGroupItems();
                ListVM.ActiveGroup.Id = allGroups[allGroups.Count - 1].Id;
                ListVM.GroupsList.Add(ListVM.ActiveGroup);
            }
        }

        private void SaveGroupItem(object obj)
        {
            Group = ListVM.ActiveGroup.Group;
            GroupItemViewModel groupItem = obj as GroupItemViewModel;            
            if(groupItem!=null && groupItem.isValid)
            {
                if (string.IsNullOrEmpty(groupItem.VaiableName))
                {
                    if (Group.PurchasesList == null)
                    {
                        Group.PurchasesList = new List<Purchase>();
                        Purchases = new List<GroupItemViewModel>();
                    }
                    if (Group.PurchasesList.Any(p => p.Name.ToLower() == groupItem.Name.ToLower()))
                    {
                        ListVM.Main.DisplayAlert("Внимание!", "Такой элемент уже имеется в списке.", "Ok");
                        return;
                    }
                    var purch = new Purchase
                    {
                        Id = groupItem.Purchase.Id,
                        Name = groupItem.Purchase.Name,
                        Count = groupItem.Purchase.Count,
                        Units = groupItem.Purchase.Units,
                        Completed = groupItem.Purchase.Completed
                    };
                    Group.PurchasesList.Add(purch);//добавляет покупку в группу
                    Purchases.Add(ListVM.PurchasesToGIVMConverter(purch));//Добавляет IVM в текущий список GVM
                }
                else
                {
                    if (groupItem.Name.Length < 1)
                    {
                        ListVM.Main.DisplayAlert("Внимание!", "Название не может быть пустым", "Ok");
                        return;
                    }
                    var purch = Group.PurchasesList.FirstOrDefault(p => p.Name == groupItem.VaiableName);
                    Group.PurchasesList[Group.PurchasesList.IndexOf(purch)] = groupItem.Purchase;
                }
                
                App.Database.SaveGroupItem(Group);//сохраняет эту группу в БД
                if (needToSave)
                {
                    ListVM.Back();
                    ListVM.ActiveGroup = ListVM.GroupsList[ListVM.GroupsList.Count - 1];
                    ListVM.ActiveGroup.Purchases = new List<GroupItemViewModel>();
                    foreach (var groupIVM in ListVM.ActiveGroup.Group.PurchasesList)
                    {
                        ListVM.ActiveGroup.Purchases.Add(ListVM.PurchasesToGIVMConverter(groupIVM));
                    }
                }
            }
            ListVM.BackToList();
        }        
        public void CompleteToggled(GroupItemViewModel givm)
        {
            Group = ListVM.ActiveGroup.Group;
            var purch = Group.PurchasesList.FirstOrDefault(p => p.Name == givm.VaiableName);
            Group.PurchasesList[Group.PurchasesList.IndexOf(purch)] = givm.Purchase;
            App.Database.SaveGroupItem(Group);
        }

        private void CreateGroupItem(object obj)
        {
            ListVM.Main.GroupStackLayout.Children.Clear();
            ListVM.Main.GroupStackLayout.Children.Add(new GroupItemPage(new GroupItemViewModel() { ListVM=this}));             
        }
        GroupItemViewModel selectedGroupItem;
        public GroupItemViewModel SelectedGroupItem
        {
            get
            {
                return selectedGroupItem;
            }
            set
            {
                if (selectedGroupItem != value)
                {
                    GroupItemViewModel tempItem = value;
                    tempItem.VaiableName = tempItem.Name;
                    selectedGroupItem = null;
                    ListVM.Main.GroupStackLayout.Children.Clear();
                    ListVM.Main.GroupStackLayout.Children.Add(new GroupItemPage(tempItem));
                }
            }
        }

        public int Id
        {
            get
            {
                return Group.Id;
            }
            set
            {
                Group.Id = value;
            }
        }
        public List<Purchase> PurchasesList
        {
            get
            {
                return Group.PurchasesList;
            }
            set
            {
                Group.PurchasesList = value;
            }
        }
        public string Name
        {
            get
            {
                return Group.Name;
            }
            set
            {
                Group.Name = value;
            }
        }
        public string Progress
        {
            get
            {
                return Group.Progress;
            }
        }

    }
}
