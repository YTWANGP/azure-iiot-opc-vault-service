using System;
using System.Collections.Generic;
using System.Text;

namespace XamarinClient
{
    public class Settings
    {
        public static string TenantId = "2882881a-c014-4058-9161-14ff03d0b09a";
        public static string AppServiceURL = "https://dev-opcvault-service.azurewebsites.net/";
        //public static string clientId = "336f1866-809a-4cb8-94f4-43504fe12a79"; //opcvault-client
        public static string clientId = "e810cb9d-337a-4db1-a9fd-d7d110c9caba"; //opcvault-module
        //public static string clientId = "06eea79d-d512-4870-9f3f-7a9cc783997f"; //xamarin-client
        public static string commonAuthority = "https://login.microsoftonline.com/";
        //public static Uri returnUri = new Uri("https://dev-opcvault.azurewebsites.net/signin-oidc"); //opcvault-client
        public static Uri returnUri = new Uri("urn:ietf:wg:oauth:2.0:oob"); //opcvault-module
        //public static Uri returnUri = new Uri("ms-app://s-1-15-2-2311861159-1103096834-897136840-3227634866-2296469827-1823422204-3933316547/");
        public const string graphResourceUri = "033f66d9-3d04-41f5-8a44-ee62950c4eb4"; //opcvault-service
        //public const string graphResourceUri = "336f1866-809a-4cb8-94f4-43504fe12a79"; //opcvault-client
        //public const string ClientSecret = "/aZCa8qSBO9yBD2slZyIh+blfvrzkC9Bd1oAEYiNpuY="; //opcvault-client
        public const string ClientSecret = "ZTkZsf2wI5TKhHYlkhJm+N+zdXUVODoGRpj1QG1BMOU="; //opcvault-module
        //public const string ClientSecret = "" //xamarin-client
     }
}
