using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinClient.Interfaces;
using XamarinClient;

namespace XamarinClient.XAML
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    [Android.Runtime.Preserve(AllMembers = true)]
    public partial class LogoutPage : ContentPage
	{
        public LogoutPage ()
		{
			InitializeComponent ();
        }
        protected override  void OnAppearing()
        {
            new NavigationPage(new LoginPage());
            base.OnAppearing();
        }
    }
}
