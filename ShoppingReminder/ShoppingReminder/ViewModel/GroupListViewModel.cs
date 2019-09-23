using ShoppingReminder.Languages;
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
                var confirm = await Main.DisplayAlert(Resource.Attention+"!", Resource.ShareAGroup+" "+gvm.Name+"?", Resource.Yes, Resource.No);
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
                        await Main.DisplayAlert(Resource.Attention + "!", Resource.YourDeviceDoesNotSupportThisFeature, Resource.Ok);
                        return;
                    }
                    bool clear = await Main.DisplayAlert(Resource.Attention + "!", Resource.Delete+" "+gvm.Name+" "+ Resource.FromTheListOfGroups + "?", Resource.Yes, Resource.No);
                    if (clear)
                    {
                        App.Database.DeleteGroupItem(gvm.Id);
                        Back();
                    }

                }
            }
            else
            {
                await Main.DisplayAlert(Resource.Attention + "!", Resource.ListIsEmpty, Resource.Ok);
                return;
            }
        }

        private void CreateGroup(object obj)
        {
            Main.GroupStackLayout.Children.Clear();
            var gvm = new GroupViewModel(this)
            {
                Group = new Group() { Name = Resource.WithoutTitle },
                needToSave = true
            };
            ActiveGroup = gvm;
            Main.GroupStackLayout.Children.Add(new GroupPage(gvm));
        }

        private async void ActivateGroup(object obj)
        {
            var confirm = await Main.DisplayAlert(Resource.Attention + "!", Resource.MakeThitGroupAnActiveList+"?", Resource.Yes, Resource.No);
            if (confirm)
            {
                var group = obj as GroupViewModel;                
                if (group == null || group.PurchasesList == null||group.PurchasesList.Count < 1)
                {
                    await Main.DisplayAlert(Resource.Attention + "!", Resource.TheListIsEmptyTheOperationWasCanceled, Resource.Ok);
                    return;
                }
                if (App.CurrentPurchases.Count > 0)
                {
                    string[] ways = new string[] { Resource.ReplaceWithThisList, Resource.ReplaceWithThisListAndSaveTheActiveListInANewGroup, Resource.AddToCurrentList };
                    var varians = await Main.DisplayActionSheet(Resource.ThereAreItemsInTheActiveList, Resource.Cancel, null, ways);
                    if (varians == ways[0])
                    {
                        App.CurrentPurchases = new List<PurchaseViewModel>();
                        ActivateHelper(group);
                    }
                    else if (varians == ways[1])
                    {
                        Plugin.DialogKit.CrossDiaglogKit.GlobalSettings.DialogAffirmative = Resource.Ok;
                        Plugin.DialogKit.CrossDiaglogKit.GlobalSettings.DialogNegative = Resource.DoNotIndicate;
                        var groupName = await Plugin.DialogKit.CrossDiaglogKit.Current.GetInputTextAsync(Resource.Attention + "!", Resource.EnterANameForTheGoup+":");
                        var newGroup = new Group { Name = groupName == null ? DateTime.Now.ToString("dd MMM yyyy - HH:mm") : groupName, PurchasesList = new List<Purchase>() };
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
                    else if (varians == ways[2])
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
            string areInCurrent = string.Empty;
            foreach (var item in group.PurchasesList)
            {
                if (!App.CurrentPurchases.Any(p => p.Name.ToLower() == item.Name.ToLower()))
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
                else
                {
                    areInCurrent += "\n[" + item.Name + "] ";
                }
            }
            App.SaveCurrentPurchasesToDB();
            App.Database.DeleteGroupItem(group.Id);
            Main.activePurchases.Back();
            if (!string.IsNullOrEmpty(areInCurrent))
            {
                Main.DisplayAlert(Resource.Attention + "!", Resource.Products + areInCurrent + "\n"+Resource.WereAlreadyOnTheList, Resource.Ok);
            }
            else
            {
                Main.DisplayAlert(Resource.Attention + "!", Resource.AddedToCurrentList, Resource.Ok);
            }
            if (App.CurrentPurchases.Any(p => p.Completed))
            {
                ((Tab)(Main.CompletedPurchasesStackLayout.Parent.Parent.Parent.Parent)).IsEnabled = true;
            }
            Back();
        }

        private async  void DeleteGroup(object obj)
        {
            var confirm = await Main.DisplayAlert(Resource.Attention + "!", Resource.DeleteThisProductGroup, Resource.Yes, Resource.No);
            if (confirm)
            {
                var index = (int)obj;
                var result = App.Database.DeleteGroupItem(index);
                if (result == 0)
                {
                    await Main.DisplayAlert(Resource.Attention + "!", Resource.AnErrorOccurredWhileUninstalling, Resource.Ok);
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
