using ShoppingReminder.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingReminder.View
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PurchaseListPage : ContentView
	{
        PurchaseListViewModel viewModel;
        public PurchaseListPage (PurchaseListViewModel vm)
		{
            InitializeComponent();
            viewModel = vm;
            BindingContext = viewModel;   
            
            if (App.CurrentPurchases.Count < 1)
            {
                CreateButtonCommand();
                emptyListstack.IsVisible = true;
            }
            else if (App.CurrentPurchases.All(p => p.Completed))
            {
                CreateButtonsCommand();
                allCompliteListStack.IsVisible = true;
            }
        }
        void CreateButtonCommand()
        {
            TapGestureRecognizer addNewTap = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 1,
                Command = viewModel.CreatePurchaseCommand
            };
            emptyListLabel.GestureRecognizers.Add(addNewTap);
        }
        void CreateButtonsCommand()
        {
            TapGestureRecognizer completeTap = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 1,
                Command = viewModel.CompletePurchaseCommand
            };
            TapGestureRecognizer photoTap = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 1,
                Command = viewModel.TakePhotoCommand
            };
            TapGestureRecognizer addNewTap = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 1,
                Command = viewModel.CreatePurchaseCommand
            };
            completeLbl.GestureRecognizers.Add(completeTap);
            photoLbl.GestureRecognizers.Add(photoTap);
            addNewLbl.GestureRecognizers.Add(addNewTap);
        }
	}
}