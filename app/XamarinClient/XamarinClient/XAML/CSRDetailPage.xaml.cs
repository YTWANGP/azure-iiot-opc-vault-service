using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Microsoft.Azure.IIoT.OpcUa.Api.Vault;
using Microsoft.Azure.IIoT.OpcUa.Api.Vault.Models;
using XamarinClient.Models;

namespace XamarinClient.XAML
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CSRDetailPage : ContentPage
	{
        private IOpcVault _opcVaultServiceClient { get; }
        private string cert;
        private string issuer;
        private string crl;
        public CSRDetailPage ()
		{
			InitializeComponent ();
		}

        public CSRDetailPage(IOpcVault opcVaultServiceClient)
        {
            this._opcVaultServiceClient = opcVaultServiceClient;
            InitializeComponent();
            
        }

        protected async override void OnAppearing()
        {
            //initial form
            Cert.IsVisible = false;
            CertEntry.IsVisible = false;
            CertEntry.InputTransparent = false;
            Issuer.IsVisible = false;
            IssuerEntry.IsVisible = false;
            IssuerEntry.InputTransparent = false;
            Crl.IsVisible = false;
            CrlEntry.IsVisible = false;
            CrlEntry.InputTransparent = false;

            if (((CertificateRequestIndexApiModel)this.BindingContext).State.ToString() == "Approved")
            {
                //DownloadCertificateBase64
                cert = await DownloadCertificateBase64Async(((CertificateRequestIndexApiModel)this.BindingContext).RequestId, ((CertificateRequestIndexApiModel)this.BindingContext).ApplicationId);
                //DownloadIssuerBase64
                issuer = await DownloadIssuerBase64Async(((CertificateRequestIndexApiModel)this.BindingContext).CertificateGroupId, ((CertificateRequestIndexApiModel)this.BindingContext).RequestId);
                //DownloadIssuerCrlBase64
                crl = await DownloadIssuerCrlBase64Async(((CertificateRequestIndexApiModel)this.BindingContext).CertificateGroupId, ((CertificateRequestIndexApiModel)this.BindingContext).RequestId);

                if (cert != null && issuer != null && crl != null)
                {
                    Cert.IsVisible = true;
                    CertEntry.Text = cert.ToString();
                    CertEntry.IsVisible = true;
                    CertEntry.InputTransparent = true;
                    Issuer.IsVisible = true;
                    IssuerEntry.Text = issuer.ToString();
                    IssuerEntry.IsVisible = true;
                    IssuerEntry.InputTransparent = true;
                    Crl.IsVisible = true;
                    CrlEntry.Text = crl.ToString();
                    CrlEntry.IsVisible = true;
                    CrlEntry.InputTransparent = true;
                    CrlEntry.InputTransparent = true;
                }
                DownloadCertBtn.IsVisible = true;
            }
            base.OnAppearing();
        }

        public async Task<string> DownloadCertificateBase64Async(string requestId, string applicationId)
        {
            try
            {
                var result = await this._opcVaultServiceClient.FetchCertificateRequestResultAsync(requestId, applicationId);
                if ((result.State == Microsoft.Azure.IIoT.OpcUa.Api.Vault.Models.CertificateRequestState.Approved ||
                    result.State == Microsoft.Azure.IIoT.OpcUa.Api.Vault.Models.CertificateRequestState.Accepted) &&
                    result.SignedCertificate != null)
                {
                    return result.SignedCertificate;
                }

                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Certificate request not in the proper state or certificate is missing.", "", "Dismiss");

            }
            catch (Exception ex)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Failed to download certificate.", "Message: " + ex.Message, "Dismiss");
            }

            return null;
        }

        public async Task<string> DownloadIssuerBase64Async(string groupId, string requestId)
        {
            try
            {
                if (groupId == null)
                {
                    var request = await this._opcVaultServiceClient.GetCertificateRequestAsync(requestId);
                    if (request != null)
                    {
                        groupId = request.CertificateGroupId;
                    }
                    else
                    {
                        await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Certificate request " + requestId + " not found.", "", "Dismiss");
                    }
                }
                if (groupId != null)
                {
                    var issuer = await this._opcVaultServiceClient.GetCertificateGroupIssuerCAChainAsync(groupId);
                    return issuer.Chain[0].Certificate;
                }
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Certificate request " + requestId + " has no group id.", "", "Dismiss");
            }
            catch (Exception ex)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Failed to load Issuer CA certificate.", "Message: " + ex.Message, "Dismiss");
            }
            return null;
        }

        public async Task<string> DownloadIssuerCrlBase64Async(string groupId, string requestId)
        {
            try
            {
                if (groupId == null)
                {
                    var request = await this._opcVaultServiceClient.GetCertificateRequestAsync(requestId);
                    if (request != null)
                    {
                        groupId = request.CertificateGroupId;
                    }
                }

                if (groupId != null)
                {
                    var crl = await this._opcVaultServiceClient.GetCertificateGroupIssuerCACrlChainAsync(groupId);
                    return crl.Chain[0].Crl;
                }
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Certificate request " + requestId + " has no group id.", "", "Dismiss");
            }
            catch (Exception ex)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Failed to load Issuer CRL.", "Message: " + ex.Message, "Dismiss");
            }

            return null;
        }
        async void OnDownloadCert(object sender, EventArgs e)
        {
            var request = (CertificateRequestIndexApiModel)BindingContext;
            var connectPage = new ConnectPage(this._opcVaultServiceClient, cert, issuer, crl);
            connectPage.BindingContext = request;
            await Navigation.PushAsync(connectPage);
        }
    }
}
