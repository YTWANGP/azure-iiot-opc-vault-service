using Xamarin.Forms;

namespace XamarinClient.XAML
{
    [Android.Runtime.Preserve(AllMembers = true)]
    public partial class HomePage : ContentPage
	{
        AuthenticateResponse Response = null;
        public HomePage ()
		{
			InitializeComponent ();
        }

        public HomePage(AuthenticateResponse response)
        {
            Response = response;
            InitializeComponent();
        }
    }
}

