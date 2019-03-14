using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.IIoT.OpcUa.Api.Vault;
using Microsoft.Azure.IIoT.OpcUa.Api.Vault.Models;
using Newtonsoft.Json;

namespace XamarinClient.Models
{
    public class CertificateRequestIndexApiModel
    {
        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "ApplicationUri")]
        public string ApplicationUri { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "ApplicationName")]
        public string ApplicationName { get; set; }

        
        [JsonProperty(PropertyName = "requestId")]
        public string RequestId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "applicationId")]
        public string ApplicationId { get; set; }

        /// <summary>
        /// Gets possible values include: 'new', 'approved', 'rejected',
        /// 'accepted', 'deleted', 'revoked', 'removed'
        /// </summary>
        [JsonProperty(PropertyName = "state")]
        public CertificateRequestState State { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "certificateGroupId")]
        public string CertificateGroupId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "certificateTypeId")]
        public string CertificateTypeId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "signingRequest")]
        public bool SigningRequest { get; private set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "subjectName")]
        public string SubjectName { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "domainNames")]
        public IList<string> DomainNames { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "privateKeyFormat")]
        public string PrivateKeyFormat { get; set; }
        
        [JsonProperty(PropertyName = "discoveryUrls")]
        public IList<string> DiscoveryUrls { get; set; }
        
        public int TrimLength { get; set; }
        public string ApplicationUriTrimmed { get => Trimmed(ApplicationUri); }
        public string ApplicationNameTrimmed { get => Trimmed(ApplicationName); }
        public string SubjectNameTrimmed { get => Trimmed(SubjectName); }

        private string Trimmed(string value)
        {
            if (value?.Length > TrimLength)
                return value.Substring(0, TrimLength - 3) + "...";
            return value;
        }
    }
}
