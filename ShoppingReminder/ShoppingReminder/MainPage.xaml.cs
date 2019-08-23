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
        //вьюмодели для обращению к каждой из страниц приложения
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
                    Name = $"{DateTime.Now.ToString("yyyy.MM.dd_hh.mm.ss")}.jpg"
                });
                if (file == null)
                    return;
                App.Current.Properties["CurrentPhotos"] += file.Path + "&";
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
                Margin = 10,

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
                    Margin=10,
                    Source = "File:///" + tempSrc,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                    
                };
                Xamarin.Forms.PlatformConfiguration.AndroidSpecific.WebView.SetDisplayZoomControls(wv, true);
                Xamarin.Forms.PlatformConfiguration.AndroidSpecific.WebView.SetEnableZoomControls(wv, true);
                layout.Children.Clear();
                layout.Children.Add(wv);

                var BackBtn = new Button { Text = "назад"};
                BackBtn.SetDynamicResource(Button.StyleProperty, "BackBtn");
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
                        DelBtn,
                        BackBtn
                    }
                };
                layout.Children.Add(btnStack);
            };

            layout.Children.Clear();
            layout.Children.Add(lv);
            var backBtn = new Button() { Command = back, Text = "назад" };
            backBtn.SetDynamicResource(Button.StyleProperty, "BackBtn");
            var delBtn= new Button() { Command = deletePhotos, CommandParameter = source, Text = "удалить все" };
            delBtn.SetDynamicResource(Button.StyleProperty, "DeleteBtn");
            var listBtnStack = new StackLayout
            {
                HorizontalOptions = LayoutOptions.EndAndExpand,
                Orientation = StackOrientation.Horizontal,
                Children ={
                    delBtn,
                    backBtn
                }
            };
            var countLbl = new Label { Text = "Прикрепленных фото: " + sourses.Length.ToString(), VerticalTextAlignment = TextAlignment.Center };
            countLbl.SetDynamicResource(Label.StyleProperty, "Discription");
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
            DirectoryInfo dir = new DirectoryInfo(@"/storage/emulated/0/Android/data/com.companyname.ShoppingReminder/files/Pictures/ShoppingReminder");//TODO: изменить адрес при смене компании.
            var files = dir.GetFiles();
            var confirm = await DisplayAlert(files.Length.ToString(), "Удалить файлы?", "Да", "Нет");
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

        private void Button_Clicked_5(object sender, EventArgs e)
        {            
            ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            DisplayAlert(mergedDictionaries.Count.ToString(), App.theme.ToString(), "Ok");
            if (mergedDictionaries != null)
            {
                mergedDictionaries.Clear();
                if (App.theme == 0)
                {
                        mergedDictionaries.Add(new DefaultTheme());
                        App.theme = Theme.Dark;
                }
                else
                {
                        mergedDictionaries.Add(new DarkTheme());
                    App.theme = Theme.Default;
                }
            }
        }
        public void DeletePhotosHelper(string fullPath)
        {
            var pathes = fullPath.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in pathes)
            {
                File.Delete(item);
            }
        }
    }
    
    
}