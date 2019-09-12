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
    public class GroupListViewModel
    {
        private ICommand deleteGroupCommand;
        public ICommand DeleteGroupCommand
        { get
            {
                return this.deleteGroupCommand;
            }
            set
            {
                this.deleteGroupCommand = value;
            }
        }
        private ICommand activateGroupCommand;
        public ICommand ActivateGroupCommand
        {
            get
            {
                return this.activateGroupCommand;
            }
            set
            {
                this.activateGroupCommand = value;
            }
        }
        private ICommand createGroupCommand;
        public ICommand CreateGroupCommand
        {
            get
            {
                return this.createGroupCommand;
            }
            set
            {
                this.createGroupCommand = value;
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
        private ICommand sharePurchasesCommand;
        public ICommand SharePurchasesCommand
        {
            get
            {
                return this.sharePurchasesCommand;
            }
            set
            {
                this.sharePurchasesCommand = value;
            }
        }


        public List<GroupViewModel> GroupsList => App.Groups;
        public GroupViewModel ActiveGroup { get; set; }
        public MainPage Main;
        public GroupListViewModel(MainPage mp)
        {
            Main = mp;
            DeleteGroupCommand = new Command(DeleteGroup);
            ActivateGroupCommand = new Command(ActivateGroup);
            CreateGroupCommand = new Command(CreateGroup);
            BackCommand = new Command(Back);
            SharePurchasesCommand = new Command(SharePurchases);
            Back();
        }

        private async void SharePurchases(object obj)
        {
            var gvm = obj as GroupViewModel;
            if (gvm.PurchasesList != null && gvm.PurchasesList.Count>0)
            {
                var confirm = await Main.DisplayAlert("Внимание!", $"Поделиться группой \"{gvm.Name}\"?", "Да", "Нет");
                if (confirm)
                {
                    Purchase[] sendingPurchases = new Purchase[gvm.PurchasesList.Count];
                    for (int i = 0; i < gvm.PurchasesList.Count; i++)
                    {
                        sendingPurchases[i] = new Purchase { Name = gvm.PurchasesList[i].Name, Count = gvm.PurchasesList[i].Count, Units = gvm.PurchasesList[i].Units };
                    }
                    try
                    {
                        Main.SharePurchases(sendingPurchases);
                    }
                    catch
                    {
                        await Main.DisplayAlert("Внимание!", "Кажется, ваше устройство не поддерживает эту функцию.", "Ок");
                        return;
                    }
                    bool clear = await Main.DisplayAlert("Внимание!", $"Удалить \"{gvm.Name}\" из списка групп?", "Ок", "Отмена");
                    if (clear)
                    {
                        App.Database.DeleteGroupItem(gvm.Id);
                        Back();
                    }

                }
            }
            else
            {
                Main.DisplayAlert("Внимание!", "Список товаров пуст", "Ок");
                return;
            }
        }

        private void CreateGroup(object obj)
        {
            Main.GroupStackLayout.Children.Clear();
            var gvm = new GroupViewModel(this)
            {
                Group = new Group() { Name = "Без названия" },
                needToSave = true
            };
            ActiveGroup = gvm;
            Main.GroupStackLayout.Children.Add(new GroupPage(gvm));
        }

        private async void ActivateGroup(object obj)
        {
            var confirm = await Main.DisplayAlert("Внимание!", "Сделать данную группу активным списком?", "Да", "Нет");
            if (confirm)
            {
                var group = obj as GroupViewModel;                
                if (group == null || group.PurchasesList == null||group.PurchasesList.Count < 1)
                {
                    await Main.DisplayAlert("Внимание!", "Список пуст. Оперция отменена.", "ОК");
                    return;
                }
                if (App.CurrentPurchases.Count > 0)
                {
                    var varians = await Main.DisplayActionSheet("В активном списке есть элементы.", "Отмена", null, new string[] {"Заменить этим списком"
                                                                                                                           , "Заменить этим списком, а активный сохранить в новую группу"
                                                                                                                           , "Добавить к текущему" });
                    if (varians == "Заменить этим списком")
                    {
                        App.CurrentPurchases = new List<PurchaseViewModel>();
                        ActivateHelper(group);
                    }
                    else if (varians == "Заменить этим списком, а активный сохранить в новую группу")
                    {
                        Plugin.DialogKit.CrossDiaglogKit.GlobalSettings.DialogAffirmative = "Ок";
                        Plugin.DialogKit.CrossDiaglogKit.GlobalSettings.DialogNegative = "Не указывать";
                        var groupName = await Plugin.DialogKit.CrossDiaglogKit.Current.GetInputTextAsync("Внимание!", "Введите название для группы:");
                        var newGroup = new Group { Name = groupName == null ? "Сохраненный список" : groupName, PurchasesList = new List<Purchase>() };
                        foreach (var item in App.CurrentPurchases)
                        {
                            newGroup.PurchasesList.Add(new Purchase
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Count = item.Count,
                                Units = item.Units,
                                Completed = item.Completed
                            });
                        }
                        App.Database.SaveGroupItem(newGroup);
                        App.CurrentPurchases = new List<PurchaseViewModel>();
                        ActivateHelper(group);
                    }
                    else if (varians == "Добавить к текущему")
                    {
                        ActivateHelper(group);
                    }
                }
                else
                {
                    ActivateHelper(group);
                }
            }
        }
        void ActivateHelper(GroupViewModel group)
        {
            foreach (var item in group.PurchasesList)
            {
                App.CurrentPurchases.Add(new PurchaseViewModel()
                {
                    Name = item.Name,
                    Count = item.Count,
                    Units = item.Units,
                    Completed = item.Completed,
                    ListVM = Main.activePurchases
                });
            }
            App.SaveCurrentPurchasesToDB();
            App.Database.DeleteGroupItem(group.Id);
            Main.activePurchases.Back();
            if (App.CurrentPurchases.Any(p => p.Completed))
            {
                ((Tab)(Main.CompletedPurchasesStackLayout.Parent.Parent.Parent.Parent)).IsEnabled = true;
            }
            Back();
        }

        private async  void DeleteGroup(object obj)
        {
            var confirm = await Main.DisplayAlert("Внимание!", "Удалить данную группу товаров?", "Да", "Нет");
            if (confirm)
            {
                var index = (int)obj;
                var result = App.Database.DeleteGroupItem(index);
                if (result == 0)
                {
                    await Main.DisplayAlert("Внимание!", "Произошла ошибка при удалении", "ОК");
                }
                Back();
            }            
        }

        public void Back()
        {
            App.Groups = App.Database.GetGroupItems();
            foreach (var item in App.Groups)
            {
                item.ListVM = this;
            }
            Main.GroupStackLayout.Children.Clear();
            Main.GroupStackLayout.Children.Add(new GroupListPage(this));
        }
        GroupViewModel selectedGroup;
        public GroupViewModel SelectedGroup
        {
            get
            {
                return selectedGroup;
            }
            set
            {
                if (selectedGroup != value)
                {
                    GroupViewModel tempGroup = value;
                    ActiveGroup = tempGroup;
                    if (tempGroup.Group != null && tempGroup.Group.PurchasesList != null)
                    {
                        tempGroup.Purchases = new List<GroupItemViewModel>();
                        foreach (var item in tempGroup.Group.PurchasesList)
                        {                            
                           tempGroup.Purchases.Add(PurchasesToGIVMConverter(item));
                        }
                    }
                    selectedGroup = null;
                    BackToList();
                }
            }
        }
        public GroupItemViewModel PurchasesToGIVMConverter(Purchase purchase)
        {
            return new GroupItemViewModel()
            {
                Name = purchase.Name,
                Units = purchase.Units,
                Count = purchase.Count,
                Completed = purchase.Completed,
                Id = purchase.Id,
                ListVM = ActiveGroup
            };
        }
        public void BackToList()
        {
            Main.GroupStackLayout.Children.Clear();
            Main.GroupStackLayout.Children.Add(new GroupPage(ActiveGroup));
        }
    }
}
