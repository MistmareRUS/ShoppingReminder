using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using ShoppingReminder.Model;
using ShoppingReminder.Themes;
using ShoppingReminder.View;
using ShoppingReminder.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Plugin.InputKit.Shared.Controls;


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
        public StackLayout SettingsStackLayout { get; set; }
        //вьюмодели для обращению к каждой из страниц приложения
        public PurchaseListViewModel activePurchases;
        public HistoryListViewModel history;
        public PlanListViewModel plan;
        public SettingsViewModel settings;

        public MainPage()
        {
            InitializeComponent();
            
            CurrentPurchasesStackLayout = CurrentStack;
            CompletedPurchasesStackLayout = CompletetCurrentStack;
            HistoryStackLayout = HistoryStack;
            PlanStackLayout = PlanStack;
            PhotoStackLayout = PhotoStack;
            SettingsStackLayout = SettingsStack;
            NameLabel.FontFamily = Device.RuntimePlatform == Device.Android ? "jakobextractt.ttf#JacobExtraCTT" : "Assets/jakobextractt.ttf#JacobExtraCTT";

            activePurchases = new PurchaseListViewModel(this);
            plan = new PlanListViewModel(this);
            history = new HistoryListViewModel(this);
            settings = new SettingsViewModel(this);            
        }
        protected override bool OnBackButtonPressed()
        {
            if (Shell.Current.CurrentItem.Title == "Текущая покупка")
            {
                activePurchases.Back();
            }
            else if (Shell.Current.CurrentItem.Title == "Запланированное")
            {
                plan.Back();
            }
            else if (Shell.Current.CurrentItem.Title == "История покупок")
            {
                history.Back();
            }
            else if (Shell.Current.CurrentItem.Title == "Настройки")
            {
                settings.Back();
            }
            else
            {
                DisplayAlert("Необратотанная страница", Shell.Current.CurrentItem.Title, "Ok");
            }
            return true;
            //return base.OnBackButtonPressed();
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
                    Name = $"{DateTime.Now.ToString("yyyy.MM.dd_hh.mm.ss")}.jpg"
                });
                if (file == null)
                    return;
                var ph=activePurchases.GetCurrentPhotoString();
                if (ph == null)
                {
                    App.Current.Properties["CurrentPhotos"] = file.Path + "&";
                }
                else
                {
                    App.Current.Properties["CurrentPhotos"] += file.Path + "&";
                }
            }
            else
            {
                await DisplayAlert("Внимание!", "Камера не доступна.", "Ок");
                return;
            }
        }
        public void GetPhotos(string source, StackLayout layout, ICommand back, ICommand deletePhoto, ICommand deletePhotos)
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
                Margin = 5,

                ItemTemplate = new DataTemplate(() =>
                {
                    var stackForTemplate = new StackLayout();
                    var img = new Image();
                    img.SetBinding(Image.SourceProperty, "ImgSrc");
                    stackForTemplate.Children.Add(img);
                    return new ViewCell { View = stackForTemplate };
                })
            };
            lv.ItemSelected += (s, e) =>
            {
                var tempSrc = ((Photo)((ListView)s).SelectedItem).ImgSrc;
                var wv = new MyWebView()
                {
                    WidthRequest = 1000,
                    HeightRequest = 1000,
                    Margin=5,
                    Source = "File:///" + tempSrc,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                    
                };
                Xamarin.Forms.PlatformConfiguration.AndroidSpecific.WebView.SetDisplayZoomControls(wv, true);
                Xamarin.Forms.PlatformConfiguration.AndroidSpecific.WebView.SetEnableZoomControls(wv, true);
                layout.Children.Clear();
                layout.Children.Add(wv);

                var BackBtn = new Button { Text = "все фото"};
                BackBtn.SetDynamicResource(Button.StyleProperty, "ListBtn");
                BackBtn.Clicked += (_s, _e) =>
                {
                    GetPhotos(source, layout, back, deletePhoto, deletePhotos);
                };
                var DelBtn = new Button() { Command = deletePhoto, CommandParameter = tempSrc, Text = "удалить" };
                DelBtn.SetDynamicResource(Button.StyleProperty, "DeleteBtn");
                var btnStack = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.End,
                    Orientation = StackOrientation.Horizontal,
                    Children =
                    {
                        BackBtn,
                        DelBtn
                    }
                };
                layout.Children.Add(btnStack);
            };

            layout.Children.Clear();
            layout.Children.Add(lv);
            
            var delBtn= new Button() { Command = deletePhotos, CommandParameter = source, Text = "удалить все" };
            delBtn.SetDynamicResource(Button.StyleProperty, "DeleteBtn");
            var listBtnStack = new StackLayout
            {
                HorizontalOptions = LayoutOptions.End,
                Orientation = StackOrientation.Horizontal,
                Children ={
                    delBtn
                }
            };
            var countLbl = new Label { Text = " Всего фото: " + sourses.Length.ToString()+" ", VerticalTextAlignment = TextAlignment.End, VerticalOptions=LayoutOptions.End, HorizontalOptions=LayoutOptions.EndAndExpand};
            countLbl.SetDynamicResource(Label.StyleProperty, "Discription");
            countLbl.SetDynamicResource(Label.TextColorProperty, "MainColor");
            layout.Children.Add(new StackLayout {
                Children =
                {
                    countLbl,
                    listBtnStack
                },
                Orientation=StackOrientation.Horizontal,
                HorizontalOptions=LayoutOptions.FillAndExpand,
                VerticalOptions=LayoutOptions.Center
            });
        }

        
      
        public void DeletePhotosHelper(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                return;
            }
            var pathes = fullPath.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in pathes)
            {
                File.Delete(item);
            }
        }

        private  void SideMenuActivate(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = true; 
        }
        
    }
    
    
}