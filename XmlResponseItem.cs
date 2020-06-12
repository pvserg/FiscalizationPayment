using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AAP.OTAFinance.Integration.Uniteller.FiscalizationPayment
{
    public class XmlResponseItem
    {
        [XmlElement(ElementName = "result")]
        public string Result { get; set; }

        [XmlElement(ElementName = "errormessage")]
        public string ErrorMessage { get; set; }

        [XmlElement(ElementName = "receipt")]
        public string Receipt { get; set; }
    }
}
