using System;
using System.Collections.Generic;
using System.Text;

namespace XamarinClient.Configuration
{
    public interface IConfigurationStreamProviderFactory
    {
        IConfigurationStreamProvider Create();
    }
}
