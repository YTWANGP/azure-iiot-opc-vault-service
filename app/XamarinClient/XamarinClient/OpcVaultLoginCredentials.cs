// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//

using Microsoft.Azure.IIoT.OpcUa.Api.Vault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.Ua.Gds.Server.OpcVault
{
    // Summary:
    //     Options for configuring authentication using Azure Active Directory.
    public class OpcVaultAzureADOptions
    {
        //
        // Summary:
        //     Gets or sets the client Id.
        public string ClientId { get; set; }
        //
        // Summary:
        //     Gets or sets the client secret.
        public string ClientSecret { get; set; }
        //
        // Summary:
        //     Gets or sets the tenant Id.
        public string TenantId { get; set; }
        //
        // Summary:
        //     Gets or sets the Azure Active Directory instance.
        public string Authority { get; set; }
    }

    public class OpcVaultLoginCredentials : ServiceClientCredentials
    {
        private OpcVaultApiOptions opcVaultOptions;
        private OpcVaultAzureADOptions azureADOptions;
        private const string kAuthority = "https://login.microsoftonline.com/";
        private string AuthenticationToken { get; set; }
        private DateTimeOffset ExpiresOn { get; set; }

        public OpcVaultLoginCredentials(
            OpcVaultApiOptions opcVaultOptions,
            OpcVaultAzureADOptions azureADOptions)
        {
            this.opcVaultOptions = opcVaultOptions;
            this.azureADOptions = azureADOptions;
        }

        public override void InitializeServiceClient<T>(ServiceClient<T> client)
        {
            InternalInitializeServiceClient();
        }
        private void InternalInitializeServiceClient()
        {
            var authenticationContext =
                new AuthenticationContext(
                    (String.IsNullOrEmpty(azureADOptions.Authority) ? kAuthority : azureADOptions.Authority) + azureADOptions.TenantId);

            ClientCredential clientCredential = new ClientCredential(
                clientId: azureADOptions.ClientId,
                clientSecret: azureADOptions.ClientSecret);

            
            authenticationContext.TokenCache.Clear();
            
            var result = authenticationContext.AcquireTokenAsync(
                        resource: opcVaultOptions.ResourceId,
                        clientCredential: clientCredential).GetAwaiter().GetResult();
            /*
            var user = new UserIdentifier("55d95438-d930-41c9-a72b-11624b1c6209", UserIdentifierType.UniqueId);

            result = authenticationContext.AcquireTokenSilentAsync(
                        resource: opcVaultOptions.ResourceId,
                        clientCredential: clientCredential,
                        userId: user).GetAwaiter().GetResult();
*/
            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            ExpiresOn = result.ExpiresOn;
            AuthenticationToken = result.AccessToken;
        }

        public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            DateTime now = DateTime.UtcNow;
            if (now.Add(TimeSpan.FromMinutes(2)) >= ExpiresOn)
            {
                InternalInitializeServiceClient();
            }

            if (AuthenticationToken == null)
            {
                throw new InvalidOperationException("Token Provider Cannot Be Null");
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthenticationToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //request.Version = new Version(apiVersion);
            await base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }

}
