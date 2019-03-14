using Xamarin.Forms;
using System;
using Microsoft.Azure.IIoT.OpcUa.Api.Vault;
using Microsoft.Azure.IIoT.OpcUa.Api.Vault.Models;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
namespace XamarinClient.XAML
{
    [Android.Runtime.Preserve(AllMembers = true)]
    public partial class ApplicationPage : ContentPage
    {
        private IOpcVault _opcVaultServiceClient { get; }
        public ApplicationPage()
        {
            InitializeComponent();
        }

        public ApplicationPage(IOpcVault opcVaultServiceClient)
        {
            this._opcVaultServiceClient = opcVaultServiceClient;
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            listView.ItemsSource = await RefreshDataAsync();
        }

        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var ApplicationItem = e.SelectedItem as ApplicationRecordApiModel;
            var itemPage = new ApplicationDetailPage(this._opcVaultServiceClient);
            itemPage.BindingContext = ApplicationItem;
            Navigation.PushAsync(itemPage);
        }
        public async Task<List<ApplicationRecordApiModel>> RefreshDataAsync()
        {
            var applicationQuery = new QueryApplicationsApiModel();
            var items = new List<ApplicationRecordApiModel>();
            // TODO: Implement paging.
            string nextPageLink = null;
            try
            {
                do
                {
                    var applications = await this._opcVaultServiceClient.QueryApplicationsAsync(applicationQuery, nextPageLink);
                    foreach (var app in applications.Applications)
                    {
                        items.Add(app);
                    }
                    nextPageLink = applications.NextPageLink;
                } while (nextPageLink != null);
            }
            catch (Exception ex)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("The application query failed.", "Exception message: " + ex.Message, "Dismiss");
            }
            return items;
        }
        
    }
}

