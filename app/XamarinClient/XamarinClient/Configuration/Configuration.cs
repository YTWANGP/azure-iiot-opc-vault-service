using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XamarinClient.Configuration
{
	public class Configuration  
    {
        public string TenantId { get; set; }
        public string AppServiceURL { get; set; }
        public string clientId { get; set; }
        public string commonAuthority { get; set; }
        public string graphResourceUri { get; set;}
        public string ClientSecret { get; set; }
    }
}
