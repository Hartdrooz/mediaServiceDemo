using System;
using System.Collections.Generic;
using System.Text;

namespace MediaWeb.Infrastructure.Model
{
    public class ConfigWrapper
    {
        public string SubscriptionId => Environment.GetEnvironmentVariable("MediaSubscriptionId");

        public string ResourceGroup => Environment.GetEnvironmentVariable("MediaResourceGroup");


        public string AccountName => Environment.GetEnvironmentVariable("MediaAccountName");


        public string AadTenantId => Environment.GetEnvironmentVariable("MediaAdTenantId");


        public string AadClientId => Environment.GetEnvironmentVariable("MediaAdClientId");


        public string AadSecret => Environment.GetEnvironmentVariable("MediaAdClientSecret");


        public Uri ArmAadAudience => new Uri(Environment.GetEnvironmentVariable("MediaArmAdAudience"));


        public Uri AadEndpoint => new Uri(Environment.GetEnvironmentVariable("MediaAadEndpoint"));

        public Uri ArmEndpoint => new Uri(Environment.GetEnvironmentVariable("MediaArmEndpoint"));

        public string Region => Environment.GetEnvironmentVariable("MediaRegion");

        public string SymmetricKey => Environment.GetEnvironmentVariable("MediaSymetricKey");

        public string AdaptiveStreamingTransformName => Environment.GetEnvironmentVariable("MediaTransformName");

    }
}
