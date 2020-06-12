using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.OTAFinance.Integration.Uniteller.FiscalizationPayment
{
    public class GetPaymentAttemptRequest
    {
        public string ShopID { get; set; }

        public string OrderID { get; set; }

        public string Password { get; set; }

        public string Signature { get; set; }

    }
}
