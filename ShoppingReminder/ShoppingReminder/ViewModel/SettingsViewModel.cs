﻿using ShoppingReminder.Themes;
using ShoppingReminder.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace ShoppingReminder.ViewModel
{
    public class SettingsViewModel
    {
        public MainPage Main;

        
        public SettingsViewModel(MainPage mp)
        {
            Main = mp;
            ClearHistoryCommand = new Command(ClearHistory);
            ClearPlansCommand = new Command(ClearPlans);
            ClearPhotosCommand = new Command(ClearPhotos);

            var themeNumber = GetUserSavedTheme();
            if (themeNumber != null)
            {
                SetTheme((Theme)themeNumber);
            }
            Back();
        }

        private async void ClearPhotos()
        {
            DirectoryInfo dir = new DirectoryInfo(@"/storage/emulated/0/Android/data/com.companyname.ShoppingReminder/files/Pictures/ShoppingReminder");//TODO: изменить адрес при смене компании.
            var files = dir.GetFiles();

            var confirm =await Main.DisplayAlert("Внимание!",string.Format( $"Удалить все фотографии? Всего : {files.Length}."), "Да", "Нет");
            if (!confirm)
            {
                return;
            }           
            foreach (var item in files)
            {
                File.Delete(item.FullName);
            }
            App.Current.Properties["CurrentPhotos"] = null;
            for (int i = 0; i < App.HistoryOfPurchase.Count; i++)
            {
                var tempHVM = App.HistoryOfPurchase[i];
                var tempLOP= App.Database.GetHistoryItem(tempHVM.Id);
                tempLOP.Check = "";
                App.Database.SaveHistoryItem(tempLOP);
            }
            Main.history.Back();
            Main.activePurchases.Back();
        }

        private async void ClearPlans()
        {
            var confirm =await Main.DisplayAlert("Внимание!", "Очистить список запланированных покупок?", "Да", "Нет");
            if (!confirm)
            {
                return;
            }
            App.Database.ClearPlans();
            App.LoadPlansFromDB();
            Main.plan.Back();
        }

        private async void ClearHistory()
        {
            var confirm = await Main.DisplayAlert("Внимание!", "Очистить списки истории?", "Да", "Нет");
            if (!confirm)
            {
                return;
            }            
            foreach (var item in App.HistoryOfPurchase)
            {
                Main.DeletePhotosHelper(item.Check);
                App.Database.DeleteHistoryItem(item.Id);
            }
            App.HistoryOfPurchase.Clear();
            Main.history.Back();
        }

        public ICommand ClearHistoryCommand { get; private set; }
        public ICommand ClearPhotosCommand { get; private set; }
        public ICommand ClearPlansCommand { get; private set; }

        public void Back()
        {
            Main.SettingsStackLayout.Children.Clear();
            Main.SettingsStackLayout.Children.Add(new SettingsPage(this));
        }        
        int? GetUserSavedTheme()
        {
            object savedPath;
            try
            {
                savedPath = App.Current.Properties["CurrentTheme"];
                return (int)savedPath;
            }
            catch
            {
                return null;
            }
        }
        public void SetTheme(Theme theme)
        {
            ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            if (mergedDictionaries != null)
            {
                mergedDictionaries.Clear();

                switch (theme)
                {
                    //TODO:Новые темы сюда и в перечисление.
                    //передавать цвет как BGColor
                    case Theme.Dark:
                        mergedDictionaries.Add(new DarkTheme());
                        bool dbPath0 = DependencyService.Get<IBarStyle>().SetColor("#206072");
                        break;
                    case Theme.Default:
                    default:
                        mergedDictionaries.Add(new DefaultTheme());
                        bool dbPath1 = DependencyService.Get<IBarStyle>().SetColor("#067898");
                        break;
                }
                App.Current.Properties["CurrentTheme"] = (int)theme;
            }
        }

    }
}