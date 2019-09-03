using ShoppingReminder.ViewModel;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingReminder.View
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class GroupListPage : ContentView
	{
        
        GroupListViewModel ListVM;
        public GroupListPage (GroupListViewModel vm)
		{
            
            InitializeComponent ();
            ListVM = vm;
            BindingContext = ListVM;
		}        
    }
}