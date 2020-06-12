using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.OTAFinance.Integration.Uniteller.FiscalizationPayment
{
    public class UnitellerFiscalizationController
    {
        public UnitellerFiscalizationController()
        {
        }

        ~UnitellerFiscalizationController()
        {

            try
            {

            }
            catch { }
        }

        private static readonly UnitellerFiscalizationController Instance = new UnitellerFiscalizationController();

        public static string GetPaymentAttempt(string shopId, string orderId)
        {
            return string.Empty;
        }
    }

}
