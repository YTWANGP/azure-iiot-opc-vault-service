using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.IIoT.OpcUa.Api.Vault;
using Microsoft.Azure.IIoT.OpcUa.Api.Vault.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinClient.XAML
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    [Android.Runtime.Preserve(AllMembers = true)]
    public partial class ApplicationDetailPage : ContentPage
	{
        private IOpcVault _opcVaultServiceClient { get; }
        public ApplicationDetailPage ()
		{
			InitializeComponent ();
		}

        public ApplicationDetailPage(IOpcVault opcVaultServiceClient)
        {
            this._opcVaultServiceClient = opcVaultServiceClient;
            InitializeComponent();
            
        }

        protected override void OnAppearing()
        {
            DiscoveryUrlsPicker.SelectedIndex = 0;
            base.OnAppearing();
        }
        async void OnNewRequest(object sender, EventArgs e)
        {
            var application = (ApplicationRecordApiModel)BindingContext;
            var connectPage = new ConnectPage(this._opcVaultServiceClient);
            connectPage.BindingContext = application;
            await Navigation.PushAsync(connectPage);
        }
    }
}
