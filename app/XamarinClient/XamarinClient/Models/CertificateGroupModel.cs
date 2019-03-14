using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Opc.Ua.Configuration;
using Opc.Ua;
using Opc.Ua.Gds.Client;
using Xamarin.Forms;
using System.Security.Cryptography.X509Certificates;

namespace XamarinClient.viewModels
{
    public class CertificateModel
    {
        public string Subject { get; set; }
        public string ValidFrom { get; set; }
        public string ValidTo { get; set; }
    }


    public class CertificateGroupModel : ObservableCollection<CertificateModel>
    {
        public string Title { get; set; } //Trusted/Issuers Certificates, Certificate Revocation Lists
    }

}
