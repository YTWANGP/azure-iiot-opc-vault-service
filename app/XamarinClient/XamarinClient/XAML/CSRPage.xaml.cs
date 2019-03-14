using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.IIoT.OpcUa.Api.Vault;
using Microsoft.Azure.IIoT.OpcUa.Api.Vault.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinClient.Models;

namespace XamarinClient.XAML
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Android.Runtime.Preserve(AllMembers = true)]
    public partial class CSRPage : ContentPage
    {
        private IOpcVault _opcVaultServiceClient { get; }
        public CSRPage()
        {
            InitializeComponent();
        }
        public CSRPage(IOpcVault opcVaultServiceClient)
        {
            this._opcVaultServiceClient = opcVaultServiceClient;
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            listView.ItemsSource = await RefreshDataAsync();
            base.OnAppearing();
        }
        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            
            var CSRItem = e.SelectedItem as CertificateRequestIndexApiModel;
            var itemPage = new CSRDetailPage(this._opcVaultServiceClient);
            itemPage.BindingContext = CSRItem;
            Navigation.PushAsync(itemPage);
        }

        public async Task<List<CertificateRequestIndexApiModel>> RefreshDataAsync()
        {
            var appDictionary = new Dictionary<string, ApplicationRecordApiModel>();
            var indexRequests = new List<CertificateRequestIndexApiModel>();
            string nextPageLink = null;
            const int PageSize = 1000;
            try
            {
                var requests = await this._opcVaultServiceClient.QueryCertificateRequestsAsync(pageSize: PageSize);
                while (requests != null)
                {
                    foreach (var request in requests.Requests)
                    {
                        if (request.State.ToString() == "New" || request.State.ToString() == "Approved" || request.State.ToString() == "Rejected")
                        {
                            
                            var indexRequest = new CertificateRequestIndexApiModel();
                            indexRequest.RequestId = request.RequestId;
                            indexRequest.CertificateGroupId = request.CertificateGroupId;
                            indexRequest.CertificateTypeId = request.CertificateTypeId;
                            indexRequest.State = request.State;
                            indexRequest.ApplicationId = request.ApplicationId;
                            ApplicationRecordApiModel application;
                            if (!appDictionary.TryGetValue(request.ApplicationId, out application))
                            {
                                application = await this._opcVaultServiceClient.GetApplicationAsync(request.ApplicationId);
                            }

                            if (application != null)
                            {
                                appDictionary[request.ApplicationId] = application;
                                indexRequest.ApplicationName = application.ApplicationName;
                                indexRequest.ApplicationUri = application.ApplicationUri;
                                indexRequest.DiscoveryUrls = application.DiscoveryUrls;
                            }
                            indexRequests.Add(indexRequest);
                        }
                    }
                    if (requests.NextPageLink == null)
                    {
                        break;
                    }
                    requests = await this._opcVaultServiceClient.QueryCertificateRequestsAsync(nextPageLink, pageSize: PageSize);
                }
            }
            catch (Exception ex)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Failed to load the certificate requests.", "Exception message: " + ex.Message, "Dismiss");
            }
            return indexRequests;
        }
    }
}
