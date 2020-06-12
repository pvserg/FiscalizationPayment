using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.OTAFinance.Integration.Uniteller.FiscalizationPayment
{
    public class CreateFiscalCheckRequest
    {
        public string ShopID { get; set; }

        public string PaymentAttemptID { get; set; }

        public float Subtotal { get; set; }

        public string Receipt { get; set; }

        public string Password { get; set; }

        public string ReceiptSignature { get; set; }

        public string Signature { get; set; }
    }
}
