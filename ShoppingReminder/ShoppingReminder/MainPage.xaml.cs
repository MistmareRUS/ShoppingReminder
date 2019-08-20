using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using ShoppingReminder.Model;
using ShoppingReminder.View;
using ShoppingReminder.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;


namespace ShoppingReminder
{
    public partial class MainPage : Shell
    {
        //свойства для доступа к элементам разметки
        public StackLayout CurrentPurchasesStackLayout { get; set; }
        public StackLayout CompletedPurchasesStackLayout { get; set; }
        public StackLayout HistoryStackLayout { get; set; }
        public StackLayout PlanStackLayout { get; set; }
        public StackLayout PhotoStackLayout { get; set; }
        public PurchaseListViewModel activePurchases;
        public HistoryListViewModel history;
        public PlanListViewModel plan;

        public MainPage()
        {
            InitializeComponent();

            CurrentPurchasesStackLayout = CurrentStack;
            CompletedPurchasesStackLayout = CompletetCurrentStack;
            HistoryStackLayout = HistoryStack;
            PlanStackLayout = PlanStack;
            PhotoStackLayout = PhotoStack;

            activePurchases = new PurchaseListViewModel(this);
            plan = new PlanListViewModel(this);
            history = new HistoryListViewModel(this);
        }

        public async void TakePhoto()
        {
            if (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported)
            {
                var cameraStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
                var storageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
                if (cameraStatus != PermissionStatus.Granted || storageStatus != PermissionStatus.Granted)
                {
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera, Permission.Storage });
                    cameraStatus = results[Permission.Camera];
                    storageStatus = results[Permission.Storage];
                }
                if (cameraStatus != PermissionStatus.Granted || storageStatus != PermissionStatus.Granted)
                {
                    await DisplayAlert("Внимание!", "Доступ отклонен.", "Ок");
                    return;
                }

                MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    SaveToAlbum = false,
                    Directory = "ShoppingReminder",
                    Name = $"{DateTime.Now.ToString("dd.MM.yyyy_hh.mm.ss")}.jpg"//TODO: изменить формат
                });
                if (file == null)
                    return;
                App.Current.Properties["CurrentPhotos"] += file.Path+"&";
            }
            else
            {
                await DisplayAlert("Внимание!", "Камера не доступна.", "Ок");
                return;
            }
        }
        public void GetPhotos(string source, StackLayout layout,ICommand back,ICommand deletePhoto,ICommand deletePhotos)
        {
            string[] pathes = source.Split('&');
            var sourses = new Photo[pathes.Length - 1];
            for (int i = 0; i < pathes.Length - 1; i++)
            {
                var temp = new Photo
                {
                    ImgSrc = pathes[i]
                };
                sourses[i] = temp;
            }
            ListView lv = new ListView()
            {
                HasUnevenRows = true,
                ItemsSource = sourses,
                Margin = 0,
               
                ItemTemplate = new DataTemplate(() =>
                {
                    var stackForTemplate = new StackLayout();
                    var img = new Image();
                    img.SetBinding(Image.SourceProperty, "ImgSrc");                    
                    stackForTemplate.Children.Add(img);
                    return new ViewCell { View = stackForTemplate };
                })
            };
            lv.ItemSelected += (s,e)=> 
            {
                var tempSrc = ((Photo)((ListView)s).SelectedItem).ImgSrc;
                StackLayout sl = new StackLayout();
                WebView wv = new WebView();
                wv.HeightRequest = 500;
                wv.WidthRequest = 500;
                //TODO: настроить вьюшку

                var htmlSrs = new HtmlWebViewSource();
                htmlSrs.Html = "<img src=\"" + tempSrc + "\"/>";

                wv.Source = htmlSrs;
                sl.Children.Add(wv);
                layout.Children.Clear();
                layout.Children.Add(sl);

                var btnBack = new Button { Text = "<" };
                btnBack.Clicked += (_s,_e)=> 
                {
                    GetPhotos(source, layout, back, deletePhoto, deletePhotos);
                };

                var btnStack = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.End,
                    Orientation = StackOrientation.Horizontal,
                    Children =
                    {
                        new Button() { Command = deletePhoto, CommandParameter=tempSrc, Text = "X" },
                        btnBack
                    }
                };
                layout.Children.Add(btnStack);
            };
            
            layout.Children.Clear();
            layout.Children.Add(lv);
            layout.Children.Add(new StackLayout
            {
                HorizontalOptions = LayoutOptions.End,
                Orientation = StackOrientation.Horizontal,
                Children ={
                    new Button() { Command=deletePhotos, CommandParameter=source, Text = "X" },
                    new Button() { Command = back, Text = "<" }
                }
            });
        }
        
        private void Button_Clicked(object sender, EventArgs e)
        {
            App.Database.ClearPurchases();
            App.CurrentPurchases = new List<PurchaseViewModel>();
            activePurchases.Back();
        }

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            App.Database.ClearHistory();//не удаляет фотки
            App.HistoryOfPurchase = App.Database.GetHistoryItems();
            history.Back();
        }

        private void Button_Clicked_2(object sender, EventArgs e)
        {
            App.Database.ClearPlans();
            App.LoadPlansFromDB();
            plan.Back();
        }

        private void Button_Clicked_3(object sender, EventArgs e)
        {
            App.Current.Properties["CurrentPhotos"] = null;//удалить из хранилища
        }

        private async void Button_Clicked_4(object sender, EventArgs e)
        {
            DirectoryInfo dir = new DirectoryInfo(@"/storage/emulated/0/Android/data/com.companyname.ShoppingReminder/files/Pictures/ShoppingReminder");
            var files = dir.GetFiles();
            var confirm= await  DisplayAlert(files.Length.ToString(), "Удалить файлы?", "Да","Нет");
            if (confirm)
            {
                foreach (var item in files)
                {
                    File.Delete(item.FullName);
                }
                return;
            }
            var l = new ListView { ItemsSource = dir.GetFiles() };
            SettingsStack.Children.Add(l);
        }
    }
}
