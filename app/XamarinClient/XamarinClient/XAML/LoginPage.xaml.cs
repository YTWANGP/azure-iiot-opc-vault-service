using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinClient.Interfaces;
using Microsoft.Identity.Client;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Xamarin.Auth;
using System.Diagnostics;
using Microsoft.Azure.IIoT.OpcUa.Api.Vault;
using Opc.Ua.Gds.Server.OpcVault;
using XamarinClient.Configuration;
using System.Threading;

namespace XamarinClient.XAML
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginPage : ContentPage
    { 
        public LoginPage ()
		{
			InitializeComponent ();
        }

        async void OnLoginButtonClicked(object sender, EventArgs args)
        {
            //var response = await DependencyService.Get<IAuthService>().Authenticate(Settings.TenantId, Settings.graphResourceUri, Settings.clientId, Settings.returnUri, Settings.ClientSecret);

            var opcVaultOptions = new OpcVaultApiOptions();
            var azureADOptions = new OpcVaultAzureADOptions();

            using (var cts = new CancellationTokenSource())
            {
                var Settings = await ConfigurationManager.Instance.GetAsync(cts.Token);

                opcVaultOptions.BaseAddress = Settings.AppServiceURL;
                opcVaultOptions.ResourceId = Settings.graphResourceUri;
                azureADOptions.ClientId = Settings.clientId;
                azureADOptions.ClientSecret = Settings.ClientSecret;
                azureADOptions.Authority = Settings.commonAuthority;
                azureADOptions.TenantId = Settings.TenantId;

            }

            

            var serviceClient = new OpcVaultLoginCredentials(opcVaultOptions, azureADOptions);
            IOpcVault opcVaultServiceClient = new Microsoft.Azure.IIoT.OpcUa.Api.Vault.OpcVault(new Uri(opcVaultOptions.BaseAddress), serviceClient);
            //var opcVaultHandler = new OpcVaultClientHandler(opcVaultServiceClient);
            //Application.Current.MainPage = new XamarinClient.XAML.BasePage(opcVaultServiceClient);
            Application.Current.MainPage = new BasePage(opcVaultServiceClient);

        }
       

    }
}
