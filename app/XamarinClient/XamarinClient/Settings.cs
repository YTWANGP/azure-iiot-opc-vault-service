using System;
using System.Collections.Generic;
using System.Text;

namespace XamarinClient
{
    public class Settings
    {
        public static string TenantId = "";
        public static string AppServiceURL = ""; //opc vault service url, ex: https://opcvault-service.azurewebsites.net
        public static string clientId = ""; //opcvault-module application id
        public static string commonAuthority = "https://login.microsoftonline.com/";
        public static Uri returnUri = new Uri(""); //opcvault-module reply URLs
        public const string graphResourceUri = ""; //opcvault-service applicaiton id
        public const string ClientSecret = ""; //opcvault-module keys 
     }
}
