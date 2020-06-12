using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AAP.OTAFinance.Integration.Uniteller.FiscalizationPayment
{
    [XmlRootAttribute("UnitellerAPI")]
    public class XmlResponse
    {
        [XmlElement("Result")]
        public string Result { get; set; }

        [XmlElement("ErrorMessage")]
        public string ErrorMessage { get; set; }

        [XmlElement("PaymentAttemptID")]
        public string PaymentAttemptID { get; set; }
    }
}   
