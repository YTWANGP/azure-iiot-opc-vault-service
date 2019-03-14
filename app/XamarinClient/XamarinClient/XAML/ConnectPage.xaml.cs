/* ========================================================================
 * Copyright (c) 2005-2018 The OPC Foundation, Inc. All rights reserved.
 *
 * OPC Foundation MIT License 1.00
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * The complete license agreement can be found here:
 * http://opcfoundation.org/License/MIT/1.00/
 * ======================================================================*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamarinClient.XAML;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Opc.Ua.Configuration;
using Opc.Ua;
using Opc.Ua.Gds.Client;
using Microsoft.Azure.IIoT.OpcUa.Api.Vault;
using Microsoft.Azure.IIoT.OpcUa.Api.Vault.Models;
using XamarinClient;
using System.Security.Cryptography.X509Certificates;
using XamarinClient.Models;

namespace XamarinClient.XAML
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ConnectPage : ContentPage
	{
        private IOpcVault _opcVaultServiceClient { get; }
        public ServerPushConfigurationClient PushClient;
        public static bool AutoAccept = true;

        private string _cert { get; }
        private string _issuer { get; }
        private string _crl { get; }

        public ConnectPage()
        {
            InitializeComponent();
        }

        public ConnectPage(IOpcVault opcVaultServiceClient)
        {
            InitializeComponent();
            this._opcVaultServiceClient = opcVaultServiceClient;
            CreatePushButton.Text = "CreateCSR";
        }

        public ConnectPage(IOpcVault opcVaultServiceClient, string cert, string issuer, string crl)
        {
            InitializeComponent();
            this._opcVaultServiceClient = opcVaultServiceClient;
            this._cert = cert;
            this._issuer = issuer;
            this._crl = crl;
            CreatePushButton.Text = "Download certificate to server";
            
        }

        protected override bool OnBackButtonPressed()
        {
            DisconnectClient();
            return base.OnBackButtonPressed();
        }

        protected override void OnAppearing()
        {
            DiscoveryUrlsPicker.SelectedIndex = 0;
            UsernameEntry.Text = "";
            PasswordEntry.Text = "";
            ConnectButton.IsEnabled = true;
            ConnectCreateLabel.IsVisible = false;
            CreatePushButton.IsVisible = false;
            base.OnAppearing();
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
                        // connect to OPC UA server
                        PushClient = new ServerPushConfigurationClient(application);
                        PushClient.AdminCredentials = new UserIdentity(UsernameEntry.Text, PasswordEntry.Text);
                        await PushClient.Connect(DiscoveryUrlsPicker.Items[DiscoveryUrlsPicker.SelectedIndex]);

                        if (PushClient.IsConnected)
                        {
                            //ConnectButton.IsEnabled = false;
                            //ConnectIndicator.IsRunning = false;
                            if (CreatePushButton.Text == "CreateCSR")
                            {
                                //ConnectCreateLabel.IsVisible = true;
                                //CreatePushButton.IsVisible = true;
                                var app = (ApplicationRecordApiModel)BindingContext;
                                var Page = new ServerCertGrpPage(PushClient, this._opcVaultServiceClient);
                                Page.BindingContext = app;
                                await Navigation.PushAsync(Page);
                            }
                            else
                            {
                                await Navigation.PushAsync(new ServerCertGrpPage(PushClient, this._opcVaultServiceClient, this._cert, this._issuer, this._crl, ((CertificateRequestIndexApiModel)this.BindingContext).RequestId));
                            }
                        }
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

        async void OnCreatePush(object sender, EventArgs e)
        {
            try
            {
                if (CreatePushButton.Text == "CreateCSR")
                {
                    if (PushClient.IsConnected)
                    {
                        byte[] nonce = new byte[0];
                        var csr = Convert.ToBase64String(PushClient.CreateSigningRequest(null, PushClient.ApplicationCertificateType, null, false, null));
                        if (csr != null)
                        {
                            var CreateSigningRequest = await StartSigningAsync(((ApplicationRecordApiModel)this.BindingContext).ApplicationId, csr);
                            string ApplicationUri = ((ApplicationRecordApiModel)this.BindingContext).ApplicationUri;
                            string ApplicationName = ((ApplicationRecordApiModel)this.BindingContext).ApplicationName;
                            var genNewCertPage = new GenNewCertPage(this._opcVaultServiceClient, ApplicationUri, ApplicationName);
                            genNewCertPage.BindingContext = CreateSigningRequest;
                            DisconnectClient();
                            await Navigation.PushAsync(genNewCertPage);
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("An error has occurred", "Exception message: " + ee.Message, "Dismiss");
            }

        }

        public async Task<CreateSigningRequestApiModel> StartSigningAsync(string id, string csr)
        {
            try
            {
                var groups = await this._opcVaultServiceClient.GetCertificateGroupsConfigurationAsync();

                string defaultGroupId = null, defaultTypeId = null;
                if (groups.Groups.Count > 0)
                {
                    defaultGroupId = groups.Groups[0].Name;
                    defaultTypeId = groups.Groups[0].CertificateType;
                }

                var application = await this._opcVaultServiceClient.GetApplicationAsync(id);

                var request = new CreateSigningRequestApiModel()
                {
                    ApplicationId = id,
                    CertificateGroupId = defaultGroupId,
                    CertificateTypeId = defaultTypeId,
                    CertificateRequest = csr
                };
                return request;
            }
            catch (Exception ee)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("An error has occurred", "Exception message: " + ee.Message, "Dismiss");
                return null;
            }
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

            if (PushClient != null)
            {
                PushClient.Disconnect();
            }

        }

    }
}
