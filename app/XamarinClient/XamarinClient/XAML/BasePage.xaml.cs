using System;
using Xamarin.Forms;
using Microsoft.Azure.IIoT.OpcUa.Api.Vault;
using Microsoft.Azure.IIoT.OpcUa.Api.Vault.Models;
using Opc.Ua.Gds.Server.OpcVault;

namespace XamarinClient.XAML
{
    [Android.Runtime.Preserve(AllMembers = true)]
    public partial class BasePage : MasterDetailPage
    {
        //AuthenticateResponse Response = null;
        private IOpcVault _opcVaultServiceClient { get; }
        public BasePage()
        {
            InitializeComponent();
            masterPage.listView.ItemSelected += OnItemSelected;

            if (Device.RuntimePlatform == Device.UWP)
            {
                MasterBehavior = MasterBehavior.Popover;
            }
        }

        public BasePage(IOpcVault opcVaultServiceClient)
        {
            InitializeComponent();
            this._opcVaultServiceClient = opcVaultServiceClient;
            masterPage.listView.ItemSelected += OnItemSelected;

            if (Device.RuntimePlatform == Device.UWP)
            {
                MasterBehavior = MasterBehavior.Popover;
            }
        }

        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MasterPageItem;
            if (item != null)
            {
                Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetType, this._opcVaultServiceClient));
                masterPage.listView.SelectedItem = null;
                IsPresented = false;
            }
        }
    }
}
