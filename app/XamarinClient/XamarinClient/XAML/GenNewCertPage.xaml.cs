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
    public partial class GenNewCertPage : ContentPage
    {
        private IOpcVault _opcVaultServiceClient { get; }
        public GenNewCertPage()
        {
            InitializeComponent();
        }

        public GenNewCertPage(IOpcVault opcVaultServiceClient, string ApplicationUri, string ApplicationName)
        {
            InitializeComponent();
            this._opcVaultServiceClient = opcVaultServiceClient;
            ApplicationUriEntry.Text = ApplicationUri;
            ApplicationUriEntry.InputTransparent = true;
            ApplicationNameEntry.Text = ApplicationName;
            ApplicationNameEntry.InputTransparent = true;   
        }

        async void OnGenNewCert(object sender, EventArgs e)
        {
            var request = (CreateSigningRequestApiModel)this.BindingContext;
            string id = await StartSigningAsync(request);
            if (id != null)
            {
                var mdp = Application.Current.MainPage as BasePage;
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("CertificateRequest Created", "", "OK");
                mdp.Detail = new NavigationPage((new CSRPage(this._opcVaultServiceClient)));
            }
        }

        public async Task<string> StartSigningAsync(
            CreateSigningRequestApiModel request)
        {
            if (request.CertificateRequest != null)
            {
                string id;
                try
                {
                    id = await this._opcVaultServiceClient.CreateSigningRequestAsync(request);
                    return id;
                }
                catch (Exception ee)
                {
                    await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Failed to create Signing Request.", "Exception message: " + ee.Message, "Dismiss");
                }
                return null;
            }
            return null;
        }
    }
}
