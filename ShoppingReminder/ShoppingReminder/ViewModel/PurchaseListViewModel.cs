using ShoppingReminder.Languages;
using ShoppingReminder.Model;
using ShoppingReminder.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace ShoppingReminder.ViewModel
{
    public class PurchaseListViewModel
    {
        public ICommand CreatePurchaseCommand { get; protected set; }
        public ICommand DeletePurchaseCommand { get; protected set; }
        public ICommand SavePurchaseCommand { get; protected set; }
        public ICommand MarkAsCompletedPurchaseCommand { get; protected set; }
        public ICommand UnmarkAsCompletedPurchaseCommand { get; protected set; }
        public ICommand BackCommand { get; protected set; }
        public ICommand CompletePurchaseCommand { get; protected set; }
        public ICommand ClearPurchaseCommand { get; protected set; }
        public ICommand UpPurchaseCommand { get; protected set; }
        public ICommand DownPurchaseCommand { get; protected set; }
        public ICommand ToPlansCommand { get; protected set; }
        public ICommand TakePhotoCommand { get; protected set; }
        public ICommand DeletePhotoCommand { get; protected set; }
        public ICommand DeletePhotosCommand { get; protected set; }
        public ICommand SharePurchasesCommand { get; protected set; }
        public IEnumerable<PurchaseViewModel> Purchases => App.CurrentPurchases.Where(p => !p.Completed);
        public IEnumerable<PurchaseViewModel> CompletedPurchases => App.CurrentPurchases.Where(p => p.Completed);
        public MainPage Main;

        public PurchaseListViewModel(MainPage mp)
        {
            Main = mp;
            CreatePurchaseCommand = new Command(CreatePurchase);
            DeletePurchaseCommand = new Command(DeletePurchase);
            SavePurchaseCommand = new Command(SavePurchase);
            MarkAsCompletedPurchaseCommand = new Command(MarkAsCompletedPurchase);
            BackCommand = new Command(Back);
            CompletePurchaseCommand = new Command(CompletePurchase);
            ClearPurchaseCommand = new Command(ClearPurchase);
            UpPurchaseCommand = new Command(UpPurchase);
            DownPurchaseCommand = new Command(DownPurchase);
            ToPlansCommand = new Command(ToPlans);
            TakePhotoCommand = new Command(TakePhoto);
            DeletePhotoCommand = new Command(DeletePhoto);
            DeletePhotosCommand = new Command(DeletePhotos);
            UnmarkAsCompletedPurchaseCommand = new Command(UnmarkAsCompletedPurchase);
            SharePurchasesCommand = new Command(SharePurchases);

            foreach (var item in App.CurrentPurchases)
            {
                item.ListVM = this;
            }
            Back();
        }

        private async void SharePurchases()
        {
            if (App.CurrentPurchases.Count > 0)
            {
                var confirm = await Main.DisplayAlert(Resource.Attention + "!",  Resource.ShareYourCurrentShoppingList+" ?", Resource.Yes, Resource.No);
                if (confirm)
                {
                    Purchase[] sendingPurchases = new Purchase[App.CurrentPurchases.Count];
                    for (int i = 0; i < App.CurrentPurchases.Count; i++)
                    {
                        sendingPurchases[i] = new Purchase { Name = App.CurrentPurchases[i].Name, Count = App.CurrentPurchases[i].Count, Units = App.CurrentPurchases[i].Units };
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
                    bool clear = await Main.DisplayAlert(Resource.Attention + "!", Resource.ClearCurrentShoppingList+"?", Resource.Yes, Resource.No);
                    if (clear)
                    {
                        ClearPurchase();
                    }
                }
            }
            else
            {
                Main.DisplayAlert(Resource.Attention + "!", Resource.ListIsEmpty, Resource.Ok);
                return;
            }
        }

        private void UnmarkAsCompletedPurchase(object obj)
        {
            PurchaseViewModel purchase = obj as PurchaseViewModel;
            if (purchase != null && purchase.isValid)
            {
                App.CurrentPurchases.FirstOrDefault(p => p.Name == purchase.Name).Completed = false;
                if (!App.CurrentPurchases.Any(p => p.Completed))
                {                   
                    var t = (Tab)(Main.CompletedPurchasesStackLayout.Parent.Parent.Parent.Parent);
                    t.IsEnabled = false;
                    var fi = (FlyoutItem)(t.Parent);
                    fi.CurrentItem = fi.Items[0];
                }
            }
            Back();
        }
        private async void DeletePhotos()
        {
            var confirm = await Main.DisplayAlert(Resource.Attention + "!", Resource.DeletePhotosAttachedToThisList+"?", Resource.Yes, Resource.No);
            if (confirm)
            {
                var fullPath = GetCurrentPhotoString();
                if (fullPath == null)
                    return;
                var pathes = fullPath.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in pathes)
                {
                    File.Delete(item);
                }
                App.Current.Properties["CurrentPhotos"] = null;
                Main.DisplayAlert(Resource.Attention + "!", Resource.PhotosHaveBeenDeleted, Resource.Ok);
                Back();
            }
        }
        private async void DeletePhoto(object obj)
        {
            var confirm= await Main.DisplayAlert("Внимание!", "Удалить фотографию?", "Да","Нет");
            if (confirm)
            {
                string path = (string)obj;
                File.Delete(path);
                var fullPath = GetCurrentPhotoString();
                var pathIndex = fullPath.IndexOf(path);
                var newFullPath = fullPath.Remove(pathIndex, path.Length + 1);
                App.Current.Properties["CurrentPhotos"] = newFullPath;
                Main.DisplayAlert("", "Фото удалено.", "Ок");
                Main.GetPhotos(newFullPath, Main.CurrentPurchasesStackLayout, BackCommand, DeletePhotoCommand, DeletePhotosCommand);
            }
        }       
        private async void TakePhoto()
        {
            var photoAction = await Main.DisplayActionSheet("Действия с фотографиями чеков...", "Отмена", null, "Добавить", "Просмотреть");
            if (photoAction== "Добавить")
            {
                Main.TakePhoto();
            }
            else if(photoAction== "Просмотреть")
            {
                string strPath = GetCurrentPhotoString();
                if (strPath == null)
                {
                    await Main.DisplayAlert("Внимание!", "К данной покупке не прикреплено ни одного фото.", "Ок");
                    return;
                }
                Main.GetPhotos(strPath, Main.CurrentPurchasesStackLayout,BackCommand,DeletePhotoCommand,DeletePhotosCommand);                
            }
        }
        public string GetCurrentPhotoString()
        {
            object savedPath;
            try
            {
                savedPath = App.Current.Properties["CurrentPhotos"];
                return (string)savedPath;
            }
            catch
            {
                return null;
            }
        }
        private async void ToPlans(object obj)
        {
            PurchaseViewModel tempPurchase = obj as PurchaseViewModel;
            if (string.IsNullOrEmpty(tempPurchase.Name))
            {
                await Main.DisplayAlert(Resource.Attention + "!", Resource.TitleCannotBeEmpty, Resource.Ok);
                return;
            }
            var dirs = Main.groups.GroupsList.Where(g => g.Name != Resource.WithoutTitle).Select(g => g.Name).ToArray();
            string[] directions = new string[dirs.Length + 2];
            for (int i = 0; i < dirs.Length; i++)
            {
                directions[i] = dirs[i];
            }
            directions[directions.Length - 2] = Resource.ToPlans;            
            directions[directions.Length - 1] = Resource.Share;

            var direct = await Main.DisplayActionSheet(Resource.Move+" \""+tempPurchase.Name+"\" " +Resource.to+"...", "Отмена", null, directions);            
            if (direct == directions[directions.Length - 2])//в планы
            {
                if (!Main.plan.PlanList.Any(p => p.Name.ToLower() == tempPurchase.Name.ToLower()))
                {
                    App.Database.SavePlanItem(new Plan() { Name = tempPurchase.Name });
                    Main.plan.Back();
                    App.CurrentPurchases.Remove(tempPurchase);
                    Main.DisplayAlert("", Resource.MovedToPlans, Resource.Ok);
                    Back();
                }
                else
                {
                    await Main.DisplayAlert("", "\""+tempPurchase.Name+"\" "+Resource.IsAlreadyInTheList, Resource.Ok);
                }
               
            }  
            else if(direct== directions[directions.Length - 1])//отправка по сети
            {
                try
                {
                    Main.SharePurchases(new Purchase[] { tempPurchase.Purchase });
                }
                catch
                {
                    await Main.DisplayAlert(Resource.Attention + "!", Resource.YourDeviceDoesNotSupportThisFeature, Resource.Ok);
                    return;
                }
                bool clear = await Main.DisplayAlert(Resource.Attention + "!", Resource.Delete+" \""+tempPurchase.Name+"\" "+Resource.FromTheList,Resource.Yes,Resource.No);
                if (clear)
                {
                    App.CurrentPurchases.Remove(tempPurchase);
                    Back();
                }
            }
            else if(directions.Any(d => d == direct))//в группу
            {
                var grIndex = Main.groups.GroupsList.IndexOf(Main.groups.GroupsList.FirstOrDefault(g => g.Name == direct));
                if (Main.groups.GroupsList[grIndex].PurchasesList == null)
                {
                    Main.groups.GroupsList[grIndex].PurchasesList = new List<Purchase>();
                }
                else if (Main.groups.GroupsList[grIndex].PurchasesList.Any(p => p.Name.ToLower() == tempPurchase.Name.ToLower()))
                {
                    await Main.DisplayAlert(Resource.Attention + "!", "\""+tempPurchase.Name+"\" "+ Resource.IsAlreadyInTheList, Resource.Ok);
                    return;
                }
                Main.groups.GroupsList[grIndex].PurchasesList.Add(new Purchase() { Name = tempPurchase.Name, Count = tempPurchase.Count, Completed = tempPurchase.Completed, Units = tempPurchase.Units });
                App.Database.SaveGroupItem(Main.groups.GroupsList[grIndex].Group);
                App.CurrentPurchases.Remove(tempPurchase);
                Main.groups.Back();
                Back();
            }         
        }
        private void UpPurchase(object obj)
        {
            var listIndex = App.CurrentPurchases.IndexOf(obj as PurchaseViewModel);
            for (int i = listIndex-1; i >= 0; i--)
            {
                if (!App.CurrentPurchases[i].Completed)
                {
                    Swap(listIndex,i);
                    break;
                }
            }
            Back();
        }
        private  void DownPurchase(object obj)
        {
            var listIndex = App.CurrentPurchases.IndexOf(obj as PurchaseViewModel);
            for (int i = listIndex + 1; i <App.CurrentPurchases.Count; i++)
            {
                if (!App.CurrentPurchases[i].Completed)
                {
                    Swap(listIndex, i);
                    break;
                }
            }
            Back();

        }
        private void Swap(int a,int b)
        {
            PurchaseViewModel temp = App.CurrentPurchases[a];
            App.CurrentPurchases[a] = App.CurrentPurchases[b];
            App.CurrentPurchases[b] = temp;
        }
        public void Back()
        {
            Main.CurrentPurchasesStackLayout.Children.Clear();
            Main.CompletedPurchasesStackLayout.Children.Clear();
            Main.CurrentPurchasesStackLayout.Children.Add(new PurchaseListPage(this));
            Main.CompletedPurchasesStackLayout.Children.Add(new CompletedPurchaseListPage(this));
        }
        private void CreatePurchase()
        {
            Main.CurrentPurchasesStackLayout.Children.Clear();
            var creatingPage = new PurchasePage(new PurchaseViewModel()
            {
                ListVM=this
            });
            Main.CurrentPurchasesStackLayout.Children.Add(creatingPage);
        }
        private async void ClearPurchase()
        {
            var confirm = await Main.DisplayAlert(Resource.Attention + "!", Resource.ClearCurrentShoppingList+"?", Resource.Yes, Resource.No);
            if (confirm)
            {
                App.CurrentPurchases = new List<PurchaseViewModel>();
                if (GetCurrentPhotoString()!=null)
                {
                    DeletePhotos();
                }
                Back();
            }
            
        }
        private async void CompletePurchase()
        {
            var confirm = await Main.DisplayAlert(Resource.Attention + "!",  Resource.CompleteThePurchase+"?", Resource.Yes , Resource.No);
            if (confirm)
            {
                
                if (App.CurrentPurchases.Count < 1)
                {
                    if (GetCurrentPhotoString() == null)
                    {
                        await Main.DisplayAlert(Resource.Attention + "!", Resource.ThereAreNoGoodsOrPhotosOfReceiptsToSaveSavingIsNotPossible, Resource.Ok);
                        return;
                    }
                    bool onlyPhoto = await Main.DisplayAlert(Resource.Attention + "!", Resource.ThereAreNoProductsInTheListSaveOnlyPhoto+"?", Resource.Yes, Resource.No);
                    if (!onlyPhoto) return;
                }                
                string notAllCompleted = "foo";
                string[] ways = new string[] { Resource.SaveOnlyCmpleted, Resource.SaveAll, Resource.SaveCompletedAndTransferTheRemainingToTheNextPurchase };
                if (App.CurrentPurchases.Any(p => !p.Completed))
                {
                    notAllCompleted = Resource.Cancel;//значение, на случай нажатия бэк-кнопки при диалоговом окошке
                    notAllCompleted = await Main.DisplayActionSheet( Resource.NotAllPurchasesAreCompletedWhichOnesToSave+"?", Resource.Cancel, null,ways);
                }
                if (notAllCompleted == null||notAllCompleted == Resource.Cancel)
                {
                    return;
                }
                while (App.HistoryOfPurchase.Count >= App.HistoryAbleToSaveCount)
                {
                    var haveToDeleteItem = (App.HistoryOfPurchase.FirstOrDefault(p => p.Date == App.HistoryOfPurchase.Min(h => h.Date)));
                    Main.DeletePhotosHelper(haveToDeleteItem.Check);
                    App.Database.DeleteHistoryItem(haveToDeleteItem.Id);
                    Main.history.Back();
                }
                Plugin.DialogKit.CrossDiaglogKit.GlobalSettings.DialogAffirmative = Resource.Ok;
                Plugin.DialogKit.CrossDiaglogKit.GlobalSettings.DialogNegative = Resource.DoNotIndicate;
                var shopName = await Plugin.DialogKit.CrossDiaglogKit.Current.GetInputTextAsync(Resource.Attention+"!", Resource.EnterStoreName+ ":");
                if (string.IsNullOrEmpty(shopName))
                {
                    shopName = Resource.NotIndicated;
                }
                var currentList = new ListOfPurchase { PurchasesList = new List<Purchase>(), Date = DateTime.Now, Check = GetCurrentPhotoString(), ShopName = shopName };
                App.Current.Properties["CurrentPhotos"] = null;
                if (notAllCompleted == ways[2])
                {
                    if (!App.CurrentPurchases.Any(p => p.Completed) && GetCurrentPhotoString() == null)
                    {
                        await Main.DisplayAlert(Resource.Attention + "!", Resource.ThereAreNoGoodsOrPhotosOfReceiptsToSaveSavingIsNotPossible, Resource.Ok);
                        return;
                    }
                    foreach (var item in CompletedPurchases)
                    {
                        var temp = new Purchase
                        {
                            Name = item.Name,
                            Count = item.Count,
                            Units = item.Units
                        };
                        currentList.PurchasesList.Add(temp);
                    }
                    App.CurrentPurchases.RemoveAll(p => p.Completed);
                }
                else
                {

                    if (notAllCompleted == Resource.SaveAll)
                    {
                        if (App.CurrentPurchases.Count < 1 && GetCurrentPhotoString() == null)
                        {
                            await Main.DisplayAlert(Resource.Attention + "!", Resource.ThereAreNoGoodsOrPhotosOfReceiptsToSaveSavingIsNotPossible, Resource.Ok);
                            return;
                        }
                        foreach (var item in App.CurrentPurchases)
                        {
                            var temp = new Purchase
                            {
                                Name = item.Name,
                                Count = item.Count,
                                Units = item.Units
                            };
                            currentList.PurchasesList.Add(temp);
                        }
                    }
                    else
                    {
                        if (!App.CurrentPurchases.Any(p => p.Completed) && GetCurrentPhotoString() == null)
                        {
                            await Main.DisplayAlert(Resource.Attention + "!", Resource.ThereAreNoGoodsOrPhotosOfReceiptsToSaveSavingIsNotPossible, Resource.Ok);
                            return;
                        }
                        foreach (var item in App.CurrentPurchases.Where(p => p.Completed))
                        {
                            var temp = new Purchase
                            {
                                Name = item.Name,
                                Count = item.Count,
                                Units = item.Units
                            };
                            currentList.PurchasesList.Add(temp);
                        }
                    }
                    App.CurrentPurchases.Clear();
                }
                App.Database.SaveHistoryItem(currentList);
                Main.history.Back();
                Back();
            }            
        }
        private void MarkAsCompletedPurchase(object obj)
        {
            PurchaseViewModel purchase = obj as PurchaseViewModel;
            if (purchase != null && purchase.isValid)
            {
                App.CurrentPurchases.FirstOrDefault(p => p.Name == purchase.Name).Completed = true;
                ((Tab)(Main.CompletedPurchasesStackLayout.Parent.Parent.Parent.Parent)).IsEnabled = true;
            }
            Back();
        }
        private async void DeletePurchase(object obj)
        {
            PurchaseViewModel purchase = obj as PurchaseViewModel;
            if (purchase != null)
            {
                var confirm = await Main.DisplayAlert(Resource.Attention + "!", Resource.Delete+" \""+purchase.Name+"\" "+Resource.FromTheList, Resource.Yes, Resource.No);
                if (confirm)
                {
                    App.CurrentPurchases.Remove(purchase);
                    Back();
                }
            }
        }
        private void SavePurchase(object obj)
        {
            PurchaseViewModel purchase = obj as PurchaseViewModel;
            if (purchase != null && purchase.isValid)
            {
                if (string.IsNullOrEmpty(purchase.VaiableName))
                {
                    if (App.CurrentPurchases.Any(p => p.Name.ToLower() == purchase.Name.ToLower()))
                    {
                        Main.DisplayAlert(Resource.Attention + "!", "\""+purchase.Name+"\" "+Resource.IsAlreadyInTheList , Resource.Ok);
                        return;
                    }
                    App.CurrentPurchases.Add(purchase);
                }
                else
                {
                    if(purchase.Name.Length < 1)
                    {
                        Main.DisplayAlert(Resource.Attention + "!", Resource.TitleCannotBeEmpty, Resource.Ok);
                        return;
                    }
                }
            }
            Back();
        }        
        PurchaseViewModel selectedPurchase; 
        public PurchaseViewModel SelectedPurchase
        {
            get
            {
                return selectedPurchase;
            }
            set
            {
                if (selectedPurchase != value)
                {
                    PurchaseViewModel tempPurchase = value;
                    tempPurchase.VaiableName = tempPurchase.Name;
                    selectedPurchase = null;
                    Main.CurrentPurchasesStackLayout.Children.Clear();
                    Main.CurrentPurchasesStackLayout.Children.Add(new PurchasePage(tempPurchase));
                }
            }
        }       
    }
}
