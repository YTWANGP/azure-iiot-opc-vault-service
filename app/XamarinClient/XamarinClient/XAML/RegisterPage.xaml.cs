using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Microsoft.Azure.IIoT.OpcUa.Api.Vault;
using Microsoft.Azure.IIoT.OpcUa.Api.Vault.Models;
using Opc.Ua.Configuration;
using Opc.Ua;
using Opc.Ua.Gds.Client;

namespace XamarinClient.XAML
{
	public partial class RegisterPage : ContentPage
	{
        private IOpcVault _opcVaultServiceClient { get; }
        public IUserIdentity AdminUser { get; private set; }
        private ServerPushConfigurationClient GDSClient;
        public static bool AutoAccept = true;
        public RegisterPage()
		{
			InitializeComponent ();
		}

        public RegisterPage(IOpcVault opcVaultServiceClient)
        {
            this._opcVaultServiceClient = opcVaultServiceClient;
            InitializeComponent();

        }
        async void OnConnect(object sender, EventArgs e)
        {
            DisconnectClient();
            if (UsernameEntry.Text == "" || PasswordEntry.Text == "")
            {
                await DisplayAlert("Error", "Please enter administrator username/password", "Dismiss");
            }
            else
            {
                ConnectIndicator.IsRunning = true;
                ApplicationInstance application = new ApplicationInstance();
                application.ApplicationType = Opc.Ua.ApplicationType.Client;
                application.ConfigSectionName = "Opc.Ua.GdsClient";

                // load the application configuration.
                ApplicationConfiguration config = application.LoadApplicationConfiguration(false).Result;

                // check the application certificate.
                bool haveAppCertificate = await application.CheckApplicationInstanceCertificate(false, 0);

                bool connectToServer = true;
                if (!haveAppCertificate)
                {
                    connectToServer = await DisplayAlert("Warning", "missing application certificate, \nusing unsecure connection. \nDo you want to continue?", "Yes", "No");
                }

                config.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);


                if (connectToServer == true)
                {
                    try
                    {

                        GDSClient = new ServerPushConfigurationClient(application);
                        GDSClient.AdminCredentials = new UserIdentity(UsernameEntry.Text, PasswordEntry.Text);
                        await GDSClient.Connect(EntryUrl.Text);

                        if (GDSClient.IsConnected)
                        {
                            ApplicationNameEntry.Text = GDSClient.Endpoint.Description.Server.ApplicationName.ToString();
                            ApplicationUriEntry.Text = GDSClient.Endpoint.Description.Server.ApplicationUri.ToString();
                            ApplicationTypeEntry.Text = GDSClient.Endpoint.Description.Server.ApplicationType.ToString();
                            ProductUriEntry.Text = GDSClient.Endpoint.Description.Server.ProductUri.ToString();
                            DiscoveryUrlsPicker.ItemsSource = GDSClient.Endpoint.Description.Server.DiscoveryUrls;
                            DiscoveryUrlsPicker.SelectedIndex = 0;
                        }
                        ConnectIndicator.IsRunning = false;
                    }
                    catch (Exception ee)
                    {
                        ConnectIndicator.IsRunning = false;
                        await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("An error has occurred", "Exception message: " + ee.Message, "Dismiss");
                    }
                }
                else
                {
                    ConnectIndicator.IsRunning = false;
                }
            }
        }
        async void OnRegister(object sender, EventArgs e)
        {
            var apiModel = new ApplicationRecordApiModel();
            apiModel.ApplicationName = ApplicationNameEntry.Text;
            apiModel.ApplicationUri = ApplicationUriEntry.Text;
            //apiModel.ApplicationType.GetType.
            await RegisterAsync(apiModel);
        }

        public async Task RegisterAsync(ApplicationRecordApiModel apiModel)
        {
            try
            {
                if (apiModel.ApplicationType == Microsoft.Azure.IIoT.OpcUa.Api.Vault.Models.ApplicationType.Client)
                {
                    apiModel.ServerCapabilities = null;
                    apiModel.DiscoveryUrls = null;
                }
                var application = await this._opcVaultServiceClient.RegisterApplicationAsync(apiModel);
                
            }
            catch (Exception)
            {
                    
            }
               
        }
        void OnClearForm(object sender, EventArgs e)
        {
        }

        private static void CertificateValidator_CertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
            {
                e.Accept = AutoAccept;
                if (AutoAccept)
                {
                    Console.WriteLine("Accepted Certificate: {0}", e.Certificate.Subject);
                }
                else
                {
                    Console.WriteLine("Rejected Certificate: {0}", e.Certificate.Subject);
                }
            }
        }

        public void DisconnectClient()
        {
            Console.WriteLine("Disconnect Session. Waiting for exit...");

            if (GDSClient != null)
            {
                GDSClient.Disconnect();
            }

        }
    }
}

