using ShoppingReminder.Renderers;
using ShoppingReminder.ViewModel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingReminder.View
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class GroupPage : ContentView
	{
        public GroupViewModel ListVM { get; private set; }

        public GroupPage (GroupViewModel vm)
		{
            InitializeComponent();
            ListVM = vm; 
            BindingContext = ListVM;
        }

        private void NameEntry_Unfocused(object sender, FocusEventArgs e)
        {
            var entry = (SelectableEntry)sender;
            if (entry.Text != string.Empty && entry.Text != ListVM.ListVM.ActiveGroup.Name)
            {
                ListVM.NameChange(entry.Text);
            }
            else
            {
                entry.Text = ListVM.ListVM.ActiveGroup.Name;
            }
        }

        private void CompleteCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var sw = (CheckBox)sender;
            var context = (GroupItemViewModel)sw.BindingContext;
            context.VaiableName = context.Name;
            ListVM.CompleteToggled(context);
        }
    }
}