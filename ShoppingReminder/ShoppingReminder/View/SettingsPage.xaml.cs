using ShoppingReminder.Themes;
using ShoppingReminder.ViewModel;
using System;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingReminder.View
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SettingsPage : ContentView
	{
        SettingsViewModel viewModel;
        public SettingsPage(SettingsViewModel vm)
        {
            InitializeComponent();
            viewModel = vm;

            BindingContext = viewModel;
        }
        private void ThemePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            Picker picker = sender as Picker;
            Theme theme = (Theme)picker.SelectedItem;
            viewModel.SetTheme(theme);
        }        
    }
    class EnumPicker : Picker
    {
        public static readonly BindableProperty EnumTypeProperty =
            BindableProperty.Create(nameof(EnumType), typeof(Type), typeof(EnumPicker),
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                    EnumPicker picker = (EnumPicker)bindable;

                    if (oldValue != null)
                    {
                        picker.ItemsSource = null;
                    }
                    if (newValue != null)
                    {
                        if (!((Type)newValue).GetTypeInfo().IsEnum)
                            throw new ArgumentException("EnumPicker: EnumType property must be enumeration type");

                        picker.ItemsSource = Enum.GetValues((Type)newValue);
                    }
                });

        public Type EnumType
        {
            set => SetValue(EnumTypeProperty, value);
            get => (Type)GetValue(EnumTypeProperty);
        }
    }
}