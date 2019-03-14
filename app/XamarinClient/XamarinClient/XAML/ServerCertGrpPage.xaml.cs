using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Ua.Configuration;
using Opc.Ua;
using Opc.Ua.Gds.Client;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.ObjectModel;
using XamarinClient.viewModels;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Azure.IIoT.OpcUa.Api.Vault;
using Microsoft.Azure.IIoT.OpcUa.Api.Vault.Models;
using XamarinClient.Models;

namespace XamarinClient.XAML
{
    //private ObservableCollection<GroupedCategoryModel> grouped { get; set; }
    public partial class ServerCertGrpPage : ContentPage
    {
        public ServerPushConfigurationClient _PushClient;
        private ObservableCollection<CertificateGroupModel> grouped { get; set; }
        private IOpcVault _opcVaultServiceClient { get; }
        private string _cert { get; }
        private string _issuer { get; }
        private string _crl { get; }
        private string _requestid { get; }

        public ServerCertGrpPage()
        {
            InitializeComponent();
        }
        public ServerCertGrpPage(ServerPushConfigurationClient PushClient, IOpcVault opcVaultServiceClient)
        {
            InitializeComponent();
            this._PushClient = PushClient;
            this._opcVaultServiceClient = opcVaultServiceClient;
            ClickedButton.Text = "CreateCSR";
        }
        public ServerCertGrpPage(ServerPushConfigurationClient PushClient, IOpcVault opcVaultServiceClient, string cert, string issuer, string crl, string requestid)
        {
            InitializeComponent();
            this._PushClient = PushClient;
            this._opcVaultServiceClient = opcVaultServiceClient;
            this._cert = cert;
            this._issuer = issuer;
            this._crl = crl;
            this._requestid = requestid;
            ClickedButton.Text = "Download certificate to server";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            listView.ItemsSource = RefreshData();
        }

        protected override bool OnBackButtonPressed()
        {
            this._PushClient.Disconnect();
            return base.OnBackButtonPressed();
        }

        public ObservableCollection<CertificateGroupModel> RefreshData()
        {
            try
            {
                grouped = new ObservableCollection<CertificateGroupModel>();
                var TrustedCertificates = new CertificateGroupModel() { Title = "Trusted Certificates" };
                var TrustedCrls = new CertificateGroupModel() { Title = "Trusted Certificate Revocation Lists" };
                var IssuerCertificates = new CertificateGroupModel() { Title = "Issusers Certificates" };
                var IssuerCrls = new CertificateGroupModel() { Title = "Issusers Certificate Revocation Lists" };


                if (this._PushClient.ReadTrustList().TrustedCertificates.Count > 0)
                {
                    foreach (var TrustedCertificate in this._PushClient.ReadTrustList().TrustedCertificates)
                    {
                        var cert = new X509Certificate2(TrustedCertificate);
                        TrustedCertificates.Add(new CertificateModel { Subject = cert.Subject, ValidFrom = cert.NotBefore.ToString(), ValidTo = cert.NotAfter.ToString()});
                    }
                }

                if (this._PushClient.ReadTrustList().TrustedCrls.Count > 0)
                {
                    foreach (var TrustedCrl in this._PushClient.ReadTrustList().TrustedCrls)
                    {
                        var crl = new X509CRL(TrustedCrl);
                        TrustedCrls.Add(new CertificateModel { Subject = crl.Issuer, ValidFrom = crl.UpdateTime.ToString(), ValidTo = crl.NextUpdateTime.ToString() });
                    }
                }

                if (this._PushClient.ReadTrustList().IssuerCertificates.Count > 0)
                {
                    foreach (var IssuerCertificate in this._PushClient.ReadTrustList().IssuerCertificates)
                    {
                        var cert = new X509Certificate2(IssuerCertificate);
                        IssuerCertificates.Add(new CertificateModel { Subject = cert.Subject, ValidFrom = cert.NotBefore.ToString(), ValidTo = cert.NotAfter.ToString() });
                    }
                }

                if (this._PushClient.ReadTrustList().IssuerCrls.Count > 0)
                {
                    foreach (var IssuerCrl in this._PushClient.ReadTrustList().IssuerCrls)
                    {
                        var crl = new X509CRL(IssuerCrl);
                        IssuerCrls.Add(new CertificateModel { Subject = crl.Issuer, ValidFrom = crl.UpdateTime.ToString(), ValidTo = crl.NextUpdateTime.ToString() });
                    }
                }

                grouped.Add(TrustedCertificates);
                grouped.Add(TrustedCrls);
                grouped.Add(IssuerCertificates);
                grouped.Add(IssuerCrls);
            }
            catch (Exception ee)
            {
                Xamarin.Forms.Application.Current.MainPage.DisplayAlert("An error has occurred", "Exception message: " + ee.Message, "Dismiss");
            }
            return grouped;
        }

        async void OnClick(object sender, EventArgs e)
        {
            try
            {
                if (ClickedButton.Text == "Download certificate to server")
                {
                    //download new cert and issuer to server
                    if (await UpdateCertificateSelfSignedNoPrivateKey())
                    {
                        // add issuer and trusted certs to client stores
                        AddTrustListToStore(this._PushClient.Application.ApplicationConfiguration.SecurityConfiguration, this._PushClient.ReadTrustList());

                        DirectoryCertificateStore issuerStore = (DirectoryCertificateStore)CertificateStoreIdentifier.OpenStore(this._PushClient.Application.ApplicationConfiguration.SecurityConfiguration.TrustedIssuerCertificates.StorePath);
                        // add IssuerCrl to client stores
                        var issuerCrl = Convert.FromBase64String(this._crl);
                        File.WriteAllBytes(issuerStore.Directory.ToString() + "/crl/" + Utils.CertFileName(this._issuer) + ".crl", issuerCrl);

                        //Accept the CSR of OPC Vault service
                        var result = await AcceptAsync(this._requestid);
                        await Xamarin.Forms.Application.Current.MainPage.DisplayAlert(result, "", "OK");

                        listView.ItemsSource = RefreshData();
                        this._PushClient.Disconnect();
                        ClickedButton.IsVisible = false;
                    }
                }
                else
                {
                    if (this._PushClient.IsConnected)
                    {
                        byte[] nonce = new byte[0];
                        var csr = Convert.ToBase64String(this._PushClient.CreateSigningRequest(null, this._PushClient.ApplicationCertificateType, null, false, null));
                        if (csr != null)
                        {
                            var CreateSigningRequest = await StartSigningAsync(((ApplicationRecordApiModel)this.BindingContext).ApplicationId, csr);
                            string ApplicationUri = ((ApplicationRecordApiModel)this.BindingContext).ApplicationUri;
                            string ApplicationName = ((ApplicationRecordApiModel)this.BindingContext).ApplicationName;
                            var genNewCertPage = new GenNewCertPage(this._opcVaultServiceClient, ApplicationUri, ApplicationName);
                            genNewCertPage.BindingContext = CreateSigningRequest;
                            this._PushClient.Disconnect();
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

        public async Task<string> AcceptAsync(string id)
        {
            try
            {
                await this._opcVaultServiceClient.AcceptCertificateRequestAsync(id);
                return "CertificateRequest accepted!";
            }
            catch (Exception ee)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Failed to accept certificate request.", "Exception message: " + ee.Message, "Dismiss");
            }
            return null;
        }

        public async Task<bool> UpdateCertificateSelfSignedNoPrivateKey()
        {
            bool success = false;
            try
            {
                byte[] issuerCertificate = new byte[1];
                issuerCertificate = Convert.FromBase64String(this._issuer);
                byte[][] issuerCertificates = new byte[1][];
                issuerCertificates[0] = issuerCertificate;

                success = this._PushClient.UpdateCertificate(
                        null,
                        this._PushClient.ApplicationCertificateType,
                        Convert.FromBase64String(this._cert),
                        null,
                        null,
                        issuerCertificates);

                if (success)
                {
                    this._PushClient.ApplyChanges();
                    
                }
            }
            catch (Exception ee)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("An error has occurred", "Exception message: " + ee.Message, "Dismiss");
            }
            return success;
        }


        private bool AddTrustListToStore(SecurityConfiguration config, TrustListDataType trustList)
        {
            TrustListMasks masks = (TrustListMasks)trustList.SpecifiedLists;

            X509Certificate2Collection issuerCertificates = null;
            List<X509CRL> issuerCrls = null;
            X509Certificate2Collection trustedCertificates = null;
            List<X509CRL> trustedCrls = null;

            // test integrity of all CRLs
            if ((masks & TrustListMasks.IssuerCertificates) != 0)
            {
                issuerCertificates = new X509Certificate2Collection();
                foreach (var cert in trustList.IssuerCertificates)
                {
                    issuerCertificates.Add(new X509Certificate2(cert));
                }
            }
            if ((masks & TrustListMasks.IssuerCrls) != 0)
            {
                issuerCrls = new List<X509CRL>();
                foreach (var crl in trustList.IssuerCrls)
                {
                    issuerCrls.Add(new X509CRL(crl));
                }
            }
            if ((masks & TrustListMasks.TrustedCertificates) != 0)
            {
                trustedCertificates = new X509Certificate2Collection();
                foreach (var cert in trustList.TrustedCertificates)
                {
                    trustedCertificates.Add(new X509Certificate2(cert));
                }
            }
            if ((masks & TrustListMasks.TrustedCrls) != 0)
            {
                trustedCrls = new List<X509CRL>();
                foreach (var crl in trustList.TrustedCrls)
                {
                    trustedCrls.Add(new X509CRL(crl));
                }
            }

            // update store
            // test integrity of all CRLs
            TrustListMasks updateMasks = TrustListMasks.None;
            if ((masks & TrustListMasks.IssuerCertificates) != 0)
            {
                if (UpdateStoreCertificates(config.TrustedIssuerCertificates.StorePath, issuerCertificates))
                {
                    updateMasks |= TrustListMasks.IssuerCertificates;
                }
            }
            if ((masks & TrustListMasks.IssuerCrls) != 0)
            {
                if (UpdateStoreCrls(config.TrustedIssuerCertificates.StorePath, issuerCrls))
                {
                    updateMasks |= TrustListMasks.IssuerCrls;
                }
            }
            if ((masks & TrustListMasks.TrustedCertificates) != 0)
            {
                if (UpdateStoreCertificates(config.TrustedPeerCertificates.StorePath, trustedCertificates))
                {
                    updateMasks |= TrustListMasks.TrustedCertificates;
                }
            }
            if ((masks & TrustListMasks.TrustedCrls) != 0)
            {
                if (UpdateStoreCrls(config.TrustedPeerCertificates.StorePath, trustedCrls))
                {
                    updateMasks |= TrustListMasks.TrustedCrls;
                }
            }

            return masks == updateMasks;
        }

        private bool UpdateStoreCrls(
            string storePath,
            IList<X509CRL> updatedCrls)
        {
            bool result = true;
            try
            {
                using (ICertificateStore store = CertificateStoreIdentifier.OpenStore(storePath))
                {
                    var storeCrls = store.EnumerateCRLs();
                    foreach (var crl in storeCrls)
                    {
                        if (!updatedCrls.Contains(crl))
                        {
                            if (!store.DeleteCRL(crl))
                            {
                                result = false;
                            }
                        }
                        else
                        {
                            updatedCrls.Remove(crl);
                        }
                    }
                    foreach (var crl in updatedCrls)
                    {
                        store.AddCRL(crl);
                    }
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        private bool UpdateStoreCertificates(
           string storePath,
           X509Certificate2Collection updatedCerts)
        {
            bool result = true;
            try
            {
                using (ICertificateStore store = CertificateStoreIdentifier.OpenStore(storePath))
                {
                    var storeCerts = store.Enumerate().Result;
                    foreach (var cert in storeCerts)
                    {
                        if (!updatedCerts.Contains(cert))
                        {
                            if (!store.Delete(cert.Thumbprint).Result)
                            {
                                result = false;
                            }
                        }
                        else
                        {
                            updatedCerts.Remove(cert);
                        }
                    }
                    foreach (var cert in updatedCerts)
                    {
                        store.Add(cert).Wait();
                    }
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }
    }
}
