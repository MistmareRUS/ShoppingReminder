﻿using ShoppingReminder.Model;
using ShoppingReminder.View;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace ShoppingReminder.ViewModel
{
    public class HistoryListViewModel
    {
        public ICommand DeleteHistoryItemCommand { get; private set; }
        public ICommand BackCommand { get; private set; }
        public ICommand GetPhotosCommand { get; protected set; }
        public ICommand DeletePhotosCommand { get; protected set; }
        public ICommand DeletePhotoCommand { get; protected set; }
        public List<HistoryViewModel> PurchaseList => App.HistoryOfPurchase.OrderByDescending(h=>h.Date).ToList();
        public MainPage Main;
        public HistoryListViewModel(MainPage mp)
        {
            Main = mp;
            BackCommand = new Command(Back);
            DeleteHistoryItemCommand = new Command(DeleteHistoryItem);
            GetPhotosCommand = new Command(GetPhotos);
            DeletePhotoCommand = new Command(DeletePhoto);
            DeletePhotosCommand = new Command(DeletePhotos);
            foreach (var item in App.HistoryOfPurchase)
            {
                item.ListVM = this;
            }
            Back();
        }

        private async void DeletePhotos(object obj)
        {
            var confirm = await Main.DisplayAlert("Внимание!", "Удалить фотографии прикрепленные к данному сохранению?", "Да", "Нет");
            if (confirm)
            {
                var fullPath = obj as string;
                if (fullPath == null)
                {
                    Main.DisplayAlert("Внимание!", "Нет фотографий.", "Ок");
                    return;
                }
                Main.DeletePhotosHelper(fullPath);
                HistoryViewModel tempHVM = PurchaseList.FirstOrDefault(p => p.Check == fullPath);
                ListOfPurchase tempList = new ListOfPurchase
                {
                    Id = tempHVM.Id,
                    Date = tempHVM.Date,
                    Check = "",
                    PurchasesList = tempHVM.PurchasesList
                };
                App.Database.SaveHistoryItem(tempList);
                Main.DisplayAlert("Внимание!", "Фотографии были удалены.", "Ок");
                Back();
            }
        }
        private async void DeletePhoto(object obj)
        {
            var confirm = await Main.DisplayAlert("Внимание!", "Удалить фотографию?", "Да", "Нет");
            if (confirm)
            {
                string path = (string)obj;
                File.Delete(path);
                HistoryViewModel tempHVM = PurchaseList.FirstOrDefault(p => p.Check.Contains(path));
                var fullPath = tempHVM.Check;
                var pathIndex = fullPath.IndexOf(path);
                var newFullPath = fullPath.Remove(pathIndex, path.Length + 1);

                ListOfPurchase tempList = new ListOfPurchase
                {
                    Id = tempHVM.Id,
                    Date = tempHVM.Date,
                    Check = newFullPath,
                    PurchasesList = tempHVM.PurchasesList
                };
                App.Database.SaveHistoryItem(tempList);
                Main.DisplayAlert("Внимание!", "Фотографии были удалены.", "Ок");
                Back();
            }
        }
        public void GetPhotos(object obj)
        {
            string path = (string)obj;
            if (string.IsNullOrEmpty(path))
                return;
            Main.GetPhotos(path, Main.PhotoStackLayout, BackCommand, DeletePhotoCommand, DeletePhotosCommand);
        }
        public void Back()
        {
            App.HistoryOfPurchase = App.Database.GetHistoryItems();
            foreach (var item in App.HistoryOfPurchase)
            {
                item.ListVM = this;
            }
            Main.PhotoStackLayout.Children.Clear();
            var t = (Tab)(Main.PhotoStackLayout.Parent.Parent.Parent.Parent);
            t.IsEnabled = false;
            var fi = (FlyoutItem)(t.Parent);
            fi.CurrentItem = fi.Items[0];
            Main.HistoryStackLayout.Children.Clear();
            Main.HistoryStackLayout.Children.Add(new HistoryListPage(this));
        }        
        private async void DeleteHistoryItem(object obj)
        {
            var confirm = await Main.DisplayAlert("Внимание!", "Удалить данный список из истории покупок?", "Да", "Нет");
            if (confirm)
            {
                var index = (int)obj;
                var item = App.HistoryOfPurchase.FirstOrDefault(h => h.Id == index);
                Main.DeletePhotosHelper(item.Check);
                var result = App.Database.DeleteHistoryItem(index);
                if (result == 0)
                {
                    Main.DisplayAlert("Внимание!", "Произошла ошибка при удалении", "ОК");
                }
                Back();
            }
            
        }
        HistoryViewModel selectedPurchaseList;
        public HistoryViewModel SelectedPurchaseList
        {
            get
            {
                return selectedPurchaseList;
            }
            set
            {
                if (selectedPurchaseList != value)
                {
                    HistoryViewModel tempHistory = value;
                    tempHistory.ListVM = new HistoryListViewModel(Main);
                    selectedPurchaseList = null;
                    Main.PhotoStackLayout.Children.Clear();
                    var t = (Tab)(Main.PhotoStackLayout.Parent.Parent.Parent.Parent);
                    t.IsEnabled = true;
                    if (string.IsNullOrEmpty(tempHistory.Check))
                    {
                        var lbl = new Label
                        {
                            Text = "К данной покупке не прикреплено ни одного фото.",
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center,
                            HorizontalOptions = LayoutOptions.CenterAndExpand
                        };
                        lbl.SetDynamicResource(Label.StyleProperty, "Discription");
                        Main.PhotoStackLayout.Children.Add(lbl);                        
                    }
                    else
                    {
                        GetPhotos(tempHistory.Check);
                    }
                    Main.HistoryStackLayout.Children.Clear();
                    Main.HistoryStackLayout.Children.Add(new HistoryItemPage(tempHistory));
                }
            }
        }        
    }    
}
