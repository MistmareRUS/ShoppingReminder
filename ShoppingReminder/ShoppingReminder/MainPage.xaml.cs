using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using ShoppingReminder.Model;
using ShoppingReminder.ViewModel;
using System;
using System.IO;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Text;
using System.Collections.Generic;
using System.Linq;

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
        public StackLayout GroupStackLayout { get; set; }
        //вьюмодели для обращению к каждой из страниц приложения
        public PurchaseListViewModel activePurchases;
        public HistoryListViewModel history;
        public PlanListViewModel plan;
        public SettingsViewModel settings;
        public GroupListViewModel groups;

        private ICommand rateAppCommand;
        public ICommand RateAppCommand
        {
            get { return this.rateAppCommand; }
            set
            {
                this.rateAppCommand = value;
            }
        }
        private ICommand receivePurcasesCommand;
        public ICommand ReceivePurcasesCommand
        {
            get
            {
                return this.receivePurcasesCommand;
            }
            set
            {
                this.receivePurcasesCommand = value;
            }
        }

        public MainPage()
        {
            InitializeComponent();
            RateAppCommand = new Command(RateApp);
            ReceivePurcasesCommand = new Command(ReceivePurcases);
            BindingContext = this;
            CurrentPurchasesStackLayout = CurrentStack;
            CompletedPurchasesStackLayout = CompletetCurrentStack;
            HistoryStackLayout = HistoryStack;
            PlanStackLayout = PlanStack;
            PhotoStackLayout = PhotoStack;
            SettingsStackLayout = SettingsStack;
            GroupStackLayout = GroupStack;
            NameLabel.FontFamily = Device.RuntimePlatform == Device.Android ? "jakobextractt.ttf#JacobExtraCTT" : "Assets/jakobextractt.ttf#JacobExtraCTT";//убрать в XAML
            activePurchases = new PurchaseListViewModel(this);
            plan = new PlanListViewModel(this);
            history = new HistoryListViewModel(this);
            settings = new SettingsViewModel(this);
            groups = new GroupListViewModel(this);
        }

        private async void ReceivePurcases()
        {

            bool hasText=false;
            string boofer=string.Empty;
            try//на случай не поддержания Essential
            {
                hasText= Clipboard.HasText;
                if(hasText)
                    boofer = await Clipboard.GetTextAsync();
            }
            catch
            {
                Plugin.DialogKit.CrossDiaglogKit.GlobalSettings.DialogAffirmative = "Ок";
                Plugin.DialogKit.CrossDiaglogKit.GlobalSettings.DialogNegative = "Отмена";
                boofer = await Plugin.DialogKit.CrossDiaglogKit.Current.GetInputTextAsync("Внимание!", "Вставьте полученое от другого пользователя скопированное сообщение:");
                if (!string.IsNullOrEmpty(boofer))
                    hasText = true;
            }

            if (hasText)
            {
                if (boofer.StartsWith("*ShoppingReminder:*"))
                {
                    var dirs = groups.GroupsList.Where(g => g.Name != "Без названия").Select(g => g.Name).ToArray();
                    string[] directions = new string[dirs.Length + 4];
                    directions[0] = "В активный список";
                    for (int i = 1; i <= dirs.Length; i++)
                    {
                        directions[i] = dirs[i - 1];
                    }
                    directions[directions.Length - 3] = "Новая группа";
                    directions[directions.Length - 2] = "В планы";
                    directions[directions.Length - 1] = "Переслать";
                    var direct = await DisplayActionSheet("В памяти содержится список. Куда его отправить?", "Отмена",null,directions);
                    Purchase[] purchaseArray = PurchasePasre(boofer);
                    if (purchaseArray != null)
                    {
                        if (direct == directions[directions.Length - 2])//в планы
                        {
                            foreach (var item in purchaseArray)
                            {
                                App.Database.SavePlanItem(new Plan() { Name = item.Name });
                            }
                            plan.Back();
                            await DisplayAlert("", "Добавлено к планам", "Ок");
                        }
                        else if (direct == directions[directions.Length - 1])//отправка по сети
                        {
                            try
                            {
                                SharePurchases(purchaseArray);
                            }
                            catch
                            {
                                await DisplayAlert("Внимание!", "Кажется, ваше устройство не поддерживает эту функцию.", "Ок");
                                return;
                            }                            
                        }
                        else if (direct == directions[0])//к активным  //TODO: проверка таких же имен
                        {
                            if (App.CurrentPurchases.Count > 0)
                            {
                                var varians = await DisplayActionSheet("В активном списке есть элементы.", "Отмена", null, new string[] {"Заменить этим списком"
                                                                                                                           , "Заменить этим списком, а активный сохранить в новую группу"
                                                                                                                           , "Добавить к текущему" });
                                if (varians == "Заменить этим списком")
                                {
                                    App.CurrentPurchases = new List<PurchaseViewModel>();
                                    ActivateHelper(purchaseArray);
                                }
                                else if (varians == "Заменить этим списком, а активный сохранить в новую группу")
                                {
                                    Plugin.DialogKit.CrossDiaglogKit.GlobalSettings.DialogAffirmative = "Ок";
                                    Plugin.DialogKit.CrossDiaglogKit.GlobalSettings.DialogNegative = "Не указывать";
                                    var groupName = await Plugin.DialogKit.CrossDiaglogKit.Current.GetInputTextAsync("Внимание!", "Введите название для группы:");
                                    var newGroup = new Model.Group { Name = groupName == null ? DateTime.Now.ToString("dd MMM yyyy - HH:mm") : groupName, PurchasesList = new List<Purchase>() };
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
                                    groups.Back();
                                    App.CurrentPurchases = new List<PurchaseViewModel>();
                                    ActivateHelper(purchaseArray);
                                }
                                else if (varians == "Добавить к текущему")
                                {
                                    ActivateHelper(purchaseArray);
                                }
                            }
                            else
                            {
                                ActivateHelper(purchaseArray);
                            }
                        }
                        else if (dirs.Any(d => d == direct))//в группу
                        {
                            var grIndex = groups.GroupsList.IndexOf(groups.GroupsList.FirstOrDefault(g => g.Name == direct));
                            if (groups.GroupsList[grIndex].PurchasesList == null)
                            {
                                groups.GroupsList[grIndex].PurchasesList = new List<Purchase>();
                            }
                            string areInCurrent = string.Empty;
                            foreach (var item in purchaseArray)
                            {
                                if (groups.GroupsList[grIndex].PurchasesList.Any(p => p.Name.ToLower() == direct.ToLower()))
                                {
                                    areInCurrent+= "[" + item.Name + "] ";
                                }
                                else
                                {
                                    groups.GroupsList[grIndex].PurchasesList.Add(item);
                                }
                            }
                            if (!string.IsNullOrEmpty(areInCurrent))
                            {
                                await DisplayAlert("Внимание!", "Товары " + areInCurrent + " уже были в списке.", "Ок");
                            }
                            App.Database.SaveGroupItem(groups.GroupsList[grIndex].Group);
                            await DisplayAlert("", $"Перемещено в группу {direct}", "Ок");
                            groups.BackToList();
                        }
                        else if (direct=="Новая группа")
                        {
                            Plugin.DialogKit.CrossDiaglogKit.GlobalSettings.DialogAffirmative = "Ок";
                            Plugin.DialogKit.CrossDiaglogKit.GlobalSettings.DialogNegative = "Не указывать";
                            var groupName = await Plugin.DialogKit.CrossDiaglogKit.Current.GetInputTextAsync("Внимание!", "Введите название для группы:");
                            var newGroup = new Model.Group { Name = groupName == null ? DateTime.Now.ToString("dd MMM yyyy - HH:mm") : groupName, PurchasesList = new List<Purchase>() };
                            foreach (var item in purchaseArray)
                            {
                                newGroup.PurchasesList.Add(item);
                            }
                            App.Database.SaveGroupItem(newGroup);
                            groups.Back();
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Внимание!", "В памяти нет подходящего списка. Откройте присланное сообщение и скопируйте его в буфер обмена.", "Ок");
                }
            }
            else
            {
                await DisplayAlert("Внимание!", "Нет подходящего списка. Откройте присланное сообщение и скопируйте его в буфер обмена.", "Ок");
            }
            //TODO:очистить буфер.
            //await Clipboard.SetTextAsync(null);
        }

        private void ActivateHelper(Purchase[] purchases)
        {
            string areInCurrent=string.Empty;
            foreach (var item in purchases)
            {
                if (!App.CurrentPurchases.Any(p => p.Name == item.Name))
                {
                    App.CurrentPurchases.Add(new PurchaseViewModel()
                    {
                        Name = item.Name,
                        Count = item.Count,
                        Units = item.Units,
                        Completed = item.Completed,
                        ListVM = activePurchases
                    });
                }
                else
                {
                    areInCurrent +="["+ item.Name+"] ";
                }
            }
            App.SaveCurrentPurchasesToDB();
            activePurchases.Back();
            if (!string.IsNullOrEmpty(areInCurrent))
            {
                DisplayAlert("Внимание!","Товары "+areInCurrent+" уже были в списке.", "Ок");
            }
            if (App.CurrentPurchases.Any(p => p.Completed))
            {
                ((Tab)(CompletedPurchasesStackLayout.Parent.Parent.Parent.Parent)).IsEnabled = true;
            }
        }

        private Purchase[] PurchasePasre(string receiveString)
        {
            if (!string.IsNullOrEmpty(receiveString))//проверка на пустоту
            {
                List<string> purchasesString = new List<string>();
                int startIndex = 0;
                while (true)
                {
                    var tempOpen = receiveString.IndexOf('[', startIndex);
                    if (tempOpen != -1)
                    {
                        var tempClose = receiveString.IndexOf(']', tempOpen);
                        if (tempOpen != -1)
                        {
                            purchasesString.Add(receiveString.Substring(tempOpen+2, tempClose - tempOpen-2));//2- сам символ и пробел возле него
                            startIndex = tempClose;
                            continue;
                        }
                    }
                    break;
                }
                if (purchasesString.Count < 1)
                    return null;
                //Теперь разбить полученные строки на покупки
                Purchase[] purchases = new Purchase[purchasesString.Count];
                for (int i = 0; i < purchasesString.Count; i++)
                {
                    Purchase p = new Purchase();
                    int mayBeCountPartIndex = purchasesString[i].LastIndexOf("- ");
                    if (mayBeCountPartIndex != -1)
                    {
                        string mayBeCountString = purchasesString[i].Substring(mayBeCountPartIndex+2);
                        string[] mayBeCountPart = mayBeCountString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (mayBeCountPart.Length == 1|| mayBeCountPart.Length == 2)
                        {
                            bool digit = float.TryParse(mayBeCountPart[0], out float count);
                            if (digit)
                            {
                                p.Name = purchasesString[i].Substring(0, mayBeCountPartIndex-1);
                                p.Count = Math.Round(count, 3);
                                if(mayBeCountPart.Length == 2)
                                {
                                    p.Units = mayBeCountPart[1];
                                }
                                purchases[i] = p;
                                continue;
                            }
                        }                        
                    }                    
                    p.Name = purchasesString[i].Trim();
                    purchases[i] = p;
                }
                return purchases;
            }
            else
            {
                return null;
            }
        }

        private async   void RateApp()
        {
            var confirm=await DisplayAlert("Внимание!", "Вы можете оценить приложение. Это поможет его продвижению.", "Оценить","Позже");
            if (confirm)
            {
                DependencyService.Get<IRareApp>().Rate();
            }
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
            else if (Shell.Current.CurrentItem.Title == "Группы")
            {
                if (GroupStackLayout.Children[0].ClassId == "ItemPage")
                {
                    groups.BackToList();
                }
                else
                {
                    groups.Back();
                }
            }
            else
            {
                DisplayAlert("Необратотанная страница", Shell.Current.CurrentItem.Title, "Ok");
            }
            return true;
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
            string[] pathes = source.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            var sourses = new Photo[pathes.Length];
            for (int i = 0; i < pathes.Length; i++)
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
                    var img = new Image();
                    img.SetBinding(Image.SourceProperty, "ImgSrc");
                    return new ViewCell { View = new StackLayout { Children = { img } } };
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
                Xamarin.Forms.PlatformConfiguration.AndroidSpecific.WebView.SetDisplayZoomControls(wv, true);//TODO: не отображается
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
        public async void SharePurchases(Purchase[] purchases )
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine("*ShoppingReminder:*");
            foreach (var item in purchases)
            {
                message.AppendLine( $"[ {item.Name}{(item.Count == 0 ? "" :" - "+ item.Count.ToString()+(string.IsNullOrEmpty(item.Units)?"":" "+ item.Units))} ]");
            }
            message.Append("_Это сообщение можно использовать в приложении ShoppingRemnder, скопировав его в специальную форму._");

            await Share.RequestAsync(new ShareTextRequest
            {                
                Text=message.ToString(),
                Subject="Нужно купить!"
            });
        }
    }
    
    
}