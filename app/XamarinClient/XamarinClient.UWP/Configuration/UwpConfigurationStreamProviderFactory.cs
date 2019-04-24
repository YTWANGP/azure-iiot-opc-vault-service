using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamarinClient.Configuration;

namespace XamarinClient.UWP.Configuration
{
    public class UwpConfigurationStreamProviderFactory : IConfigurationStreamProviderFactory
    {
        public IConfigurationStreamProvider Create()
        {
            return new UwpConfigurationStreamProvider();
        }
    }
}
