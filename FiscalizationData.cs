using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.OTAFinance.Integration.Uniteller.FiscalizationPayment
{
    public class FiscalizationData
    {
        public FiscalizationData()
        {
        }

        public string ShopID { get; internal set; }
        public string OrderID { get; internal set; }
        public float PaidTotal { get; internal set; }
        public int СustomerId { get; internal set; }
        public string СustomerEmail { get; internal set; }
        public string СustomerPhone { get; internal set; }
        public string СustomerInn { get; internal set; }
        public string СustomerName { get; internal set; }
        public string СustomerContractNumber { get; internal set; }
        public DateTime? СustomerСontractDate { get; internal set; }

        public string PaymentAttemptID { get; internal set; }
        public string Receipt { get; internal set; }
        public DateTime ProcessEndTime { get; internal set; }
        public string ErrorMessage { get; internal set; }
        public string WarningMessage { get; internal set; }
    }
}
