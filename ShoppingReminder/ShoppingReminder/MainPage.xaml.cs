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
                    SaveToAlbum = true,
                    Directory = "ShoppingReminder",
                    Name = $"{DateTime.Now.ToString("dd.MM.yyyy_hh.mm.ss")}.jpg"
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
        public ListView GetPhotos(string source, StackLayout layout)
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
                //SelectedItem=sel,
                SelectionMode=ListViewSelectionMode.Single,
                ItemsSource = sourses,
                Margin = 3,                

                #region webView
                // в этом варианте вверх/вниз мотает список, а лево/право двигает картинку
                //ItemTemplate = new DataTemplate(() =>
                //{
                //    var wv = new WebView();
                //    wv.Margin = 3;
                //    wv.HeightRequest = 300;
                //    wv.WidthRequest = 300;

                //    //wv.SetBinding(WebView.SourceProperty, "ImgSrc");

                //    var htmlSrs = new HtmlWebViewSource();
                //    htmlSrs.SetBinding(HtmlWebViewSource.HtmlProperty, "ImgSrc");

                //    wv.Source = htmlSrs;

                //    var s = new StackLayout();
                //    s.Children.Add(wv);

                //    return new ViewCell { View = s };
                //})
                #endregion

                ItemTemplate = new DataTemplate(() =>
                {
                    var stack = new StackLayout();
                    var img = new Image();
                    img.SetBinding(Image.SourceProperty, "ImgSrc");
                    stack.Children.Add(img);
                    return new ViewCell { View = stack };
                })                

            };
            lv.ItemSelected += (s,e)=> 
            {
                layout.Children.Clear();
                var tempSrc = ((Photo)((ListView)s).SelectedItem).ImgSrc;
                StackLayout sl = new StackLayout();
                WebView wv = new WebView();
                wv.HeightRequest = 300;
                wv.WidthRequest = 300;

                var htmlSrs = new HtmlWebViewSource();
                htmlSrs.Html = "<img src=\"" + tempSrc + "\"/>";

                wv.Source = htmlSrs;
                sl.Children.Add(wv);
                var btn = new Button { Text = "<" };
                btn.Clicked += (_s,_e)=> {layout.Children[0]= GetPhotos(source, layout); };
                sl.Children.Add(new Button { Text="X"});
                sl.Children.Add(btn);

                layout.Children.Add(sl);
            };
            return lv;
        }

        

        private void Button_Clicked(object sender, EventArgs e)
        {
            App.Database.ClearPurchases();
            App.CurrentPurchases = new List<PurchaseViewModel>();
            activePurchases.Back();
        }

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            App.Database.ClearHistory();
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
            App.Current.Properties["CurrentPhotos"] = null;
        }
    }
}
