using ShoppingReminder.Model;
using ShoppingReminder.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using Plugin.InputKit.Shared.Controls;

namespace ShoppingReminder.ViewModel
{
    public class PurchaseListViewModel
    {
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

            foreach (var item in App.CurrentPurchases)
            {
                item.ListVM = this;
            }
            Back();
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
            var confirm = await Main.DisplayAlert("Внимание!", "Удалить текущие фотографии?", "Да","Нет");
            if (!confirm)
                return;
            var fullPath = GetCurrentPhotoString();
            if (fullPath == null)
                return;
            var pathes = fullPath.Split(new char[]{'&'},StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in pathes)
            {
                File.Delete(item);
            }
            App.Current.Properties["CurrentPhotos"] = null;
            await Main.DisplayAlert("Внимание!", "Фотографии были удалены.", "Ок");
            Back();
        }

        private async void DeletePhoto(object obj)
        {
            var confirm= await Main.DisplayAlert("Внимание!", "Удалить фотографию?", "Да","Нет");
            if (!confirm)
                return;
            string path = (string)obj;
            File.Delete(path);
            var fullPath = GetCurrentPhotoString();
            var pathIndex = fullPath.IndexOf(path);
            var newFullPath = fullPath.Remove(pathIndex, path.Length + 1);
            App.Current.Properties["CurrentPhotos"] = newFullPath;
            await Main.DisplayAlert("", "Фото удалено.", "Ок");
            Main.GetPhotos(newFullPath, Main.CurrentPurchasesStackLayout, BackCommand, DeletePhotoCommand, DeletePhotosCommand);
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
            var confirm = await Main.DisplayAlert("Внимание", "Переместить в планы?", "Да", "Нет");
            if (!confirm)
            {
                return;
            }
            PurchaseViewModel temp = obj as PurchaseViewModel;
            if (temp != null)
            {
                App.Database.SavePlanItem(new Plan() { Name = temp.Name });
                Main.plan.Back();
                App.CurrentPurchases.Remove(temp);
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
            PurchaseViewModel temp;
            temp = App.CurrentPurchases[a];
            App.CurrentPurchases[a] = App.CurrentPurchases[b];
            App.CurrentPurchases[b] = temp;
        }


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
            var confirm = await Main.DisplayAlert("Внимание", "Очистить список покупок?", "Да", "Нет");
            if (!confirm)
            {
                return;
            }
            App.CurrentPurchases = new List<PurchaseViewModel>();
            DeletePhotos();
            Back();
        }
        private async void CompletePurchase()
        {
            var confirm = await Main.DisplayAlert("Внимание!", "Завершить покупку?", "Да", "Нет");
            if (!confirm)
            {
                return;
            }
            if (App.CurrentPurchases.Count < 1)
            {
                if (GetCurrentPhotoString() == null)
                {
                    await Main.DisplayAlert("Внимание!", "Нет ни товаров, ни фотографий чеков. Сохранение невозможно.", "Ок");
                    return;
                }
                confirm = await Main.DisplayAlert("Внимание!", "В списке нет ни одного товара. Сохранить только фото?", "Да", "Нет");
            }
            if (!confirm)
            {
                return;
            }
            string notAllCompleted=string.Empty;
            if (App.CurrentPurchases.Any(p => !p.Completed))
            {
                notAllCompleted = await Main.DisplayActionSheet("Не все покупки отмечены как завершенные. Какие из них сохранить?", "Отмена",null,
                                                                    "Сохранить только завершенные","Сохранить все","Сохранить завершенные, а оставшиеся перенести на следующую покупку.");
            }
            if (notAllCompleted=="Отмена")
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

            Plugin.DialogKit.CrossDiaglogKit.GlobalSettings.DialogAffirmative = "Ок";
            Plugin.DialogKit.CrossDiaglogKit.GlobalSettings.DialogNegative = "Не указывать";
            var shopName = await Plugin.DialogKit.CrossDiaglogKit.Current.GetInputTextAsync("Внимание!", "Введите название магазина:");

            var currentList = new ListOfPurchase { PurchasesList = new List<Purchase>(), Date = DateTime.Now, Check= GetCurrentPhotoString(),ShopName=shopName??"не указано" };


            App.Current.Properties["CurrentPhotos"] = null;


            if(notAllCompleted == "Сохранить завершенные, а оставшиеся перенести на следующую покупку.")
            {
                if(!App.CurrentPurchases.Any(p=>p.Completed)&&GetCurrentPhotoString()==null)
                {
                    await Main.DisplayAlert("Внимание!", "Нет ни товаров, ни фотографий чеков. Сохранение не возможно.", "Ок");
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
                if (notAllCompleted== "Сохранить все")
                {
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
                    foreach (var item in App.CurrentPurchases.Where(p=>p.Completed))
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
            var confirm = await Main.DisplayAlert("Внимание!", "Удалить этот элемент из списка?", "Да", "Нет");
            if (!confirm)
            {
                return;
            }
            PurchaseViewModel purchase = obj as PurchaseViewModel;
            if (purchase != null)
            {
                App.CurrentPurchases.Remove(purchase);
            }
            Back();
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
                        Main.DisplayAlert("Внимание!", "Такой элемент уже имеется в списке.", "Ok");
                        return;
                    }
                    App.CurrentPurchases.Add(purchase);
                }
                else
                {
                    if(purchase.Name.Length < 1)
                    {
                        Main.DisplayAlert("Внимание!", "Название не может быть пустым.", "Ok");
                        return;
                    }
                    var temp=App.CurrentPurchases.FirstOrDefault(p => p.Name == purchase.VaiableName);
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
