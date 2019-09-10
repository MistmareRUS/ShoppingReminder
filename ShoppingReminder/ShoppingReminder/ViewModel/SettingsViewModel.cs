using ShoppingReminder.Themes;
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
        public ICommand ClearHistoryCommand { get; private set; }
        public ICommand ClearPhotosCommand { get; private set; }
        public ICommand ClearPlansCommand { get; private set; }
        public ICommand ClearGroupsCommand { get; private set; }
        public MainPage Main;        
        public SettingsViewModel(MainPage mp)
        {
            Main = mp;
            ClearHistoryCommand = new Command(ClearHistory);
            ClearPlansCommand = new Command(ClearPlans);
            ClearPhotosCommand = new Command(ClearPhotos);
            ClearGroupsCommand = new Command(ClearGroups);
            var themeNumber = GetUserSavedTheme();
            if (themeNumber != null)
            {
                SetTheme((Theme)themeNumber);
            }
            else
            {
                SetTheme(Theme.Standart);
            }
            Back();
        }

        private async void ClearGroups()
        {
            var confirm = await Main.DisplayAlert("Внимание!", "Очистить списки групп?", "Да", "Нет");
            if (confirm)
            {
                App.Database.ClearGroups();
                Main.groups.Back();
            }
        }

        private async void ClearPhotos()
        {
            DirectoryInfo dir = new DirectoryInfo(@"/storage/emulated/0/Android/data/com.ArtMaryGames.ShoppingReminder/files/Pictures/ShoppingReminder");
            FileInfo[] files;
            try
            {
                files = dir.GetFiles();                 
            }
            catch
            {
                await Main.DisplayAlert("Внимание!", "Фотографий не обнаружено.", "Ок");
                return;
            }
            int photoCount = files.Length > 0 ? files.Length : 0;
            var confirm =await Main.DisplayAlert("Внимание!",string.Format( $"Удалить все фотографии? Всего : {photoCount-1}."), "Да", "Нет");//1 вспомогательный файл. не фото.
            if (confirm)
            {
                foreach (var item in files)
                {
                    File.Delete(item.FullName);
                }
                App.Current.Properties["CurrentPhotos"] = null;
                for (int i = 0; i < App.HistoryOfPurchase.Count; i++)
                {
                    var tempHVM = App.HistoryOfPurchase[i];
                    var tempLOP = App.Database.GetHistoryItem(tempHVM.Id);
                    tempLOP.Check = "";
                    App.Database.SaveHistoryItem(tempLOP);
                }
                Main.history.Back();
                Main.activePurchases.Back();
            }            
        }
        private async void ClearPlans()
        {
            var confirm =await Main.DisplayAlert("Внимание!", "Очистить список запланированных покупок?", "Да", "Нет");
            if (confirm)
            {
                App.Database.ClearPlans();
                App.LoadPlansFromDB();
                Main.plan.Back();
            }            
        }
        private async void ClearHistory()
        {
            var confirm = await Main.DisplayAlert("Внимание!", "Очистить списки истории?", "Да", "Нет");
            if (confirm)
            {
                foreach (var item in App.HistoryOfPurchase)
                {
                    Main.DeletePhotosHelper(item.Check);
                    App.Database.DeleteHistoryItem(item.Id);
                }
                App.HistoryOfPurchase.Clear();
                Main.history.Back();
            }            
        }
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
                    //а так же добавлять новый If в Android.BarStyle
                    case Theme.Lavender:
                        mergedDictionaries.Add(new LavenderTheme());
                        DependencyService.Get<IBarStyle>().SetColor("#B9A8D5");
                        break;
                    case Theme.StandartDark:
                        mergedDictionaries.Add(new DarkTheme());
                        DependencyService.Get<IBarStyle>().SetColor("#206072");
                        break;
                    case Theme.Standart:
                    default:
                        mergedDictionaries.Add(new DefaultTheme());
                        DependencyService.Get<IBarStyle>().SetColor("#067898");
                        break;
                }
                Main.DisplayAlert("Внимание!", "Для полного применения темы необходимо перезапустить приложение.", "Ок.");
                App.Current.Properties["CurrentTheme"] = (int)theme;                
            }
        }

    }
}
