using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingReminder.View
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HistoryListPage : ContentView
	{
		public HistoryListPage ()
		{
			InitializeComponent ();
		}
	}
}