using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AAP.OTAFinance.Integration.Uniteller.FiscalizationPayment
{
    internal static class RequestFormedHelper
    {
        public static FormUrlEncodedContent ToFormUrlEncodedContent(GetPaymentAttemptRequest original)
        {
            return new FormUrlEncodedContent(ToKeyValuePairs(original));
        }
        public static FormUrlEncodedContent ToFormUrlEncodedContent(CreateFiscalCheckRequest original)
        {
            return new FormUrlEncodedContent(ToKeyValuePairs(original));
        }

        private static IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs(GetPaymentAttemptRequest original)
        {
            yield return new KeyValuePair<string, string>("ShopID", original.ShopID);
            yield return new KeyValuePair<string, string>("OrderID", original.OrderID);
            yield return new KeyValuePair<string, string>("Signature", original.Signature);
        }

        private static IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs(CreateFiscalCheckRequest original)
        {
            yield return new KeyValuePair<string, string>("ShopID", original.ShopID);
            yield return new KeyValuePair<string, string>("PaymentAttemptID", original.PaymentAttemptID);
            yield return new KeyValuePair<string, string>("Subtotal", original.Subtotal.ToString("F2", CultureInfo.InvariantCulture));
            yield return new KeyValuePair<string, string>("Signature", original.Signature);
            yield return new KeyValuePair<string, string>("Receipt", original.Receipt);
            yield return new KeyValuePair<string, string>("ReceiptSignature", original.ReceiptSignature);         
        }

    }
}
