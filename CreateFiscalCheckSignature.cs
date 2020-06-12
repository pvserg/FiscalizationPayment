using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AAP.OTAFinance.Integration.Uniteller.FiscalizationPayment
{

    internal static class CreateFiscalCheckSignature
    {
        public static string Byte2HexString(byte[] signatureBytes)
        {
            return string.Join(string.Empty, signatureBytes.Select(b => b.ToString("x2")));
        }

        public static string Signature(this CreateFiscalCheckRequest parameters)
        {
            //------------------------ TEST ---------------------------------------------------------
            // проверка
            //using (var md5 = MD5.Create())
            //{
            //    var shopID = md5.ComputeHash(Encoding.UTF8.GetBytes(parameters.ShopID));
            //    var paymentAttemptID = md5.ComputeHash(Encoding.UTF8.GetBytes(parameters.PaymentAttemptID));
            //    var subtotal = md5.ComputeHash(Encoding.UTF8.GetBytes(parameters.Subtotal.ToString("F2", CultureInfo.InvariantCulture)));
            //    var password = md5.ComputeHash(Encoding.UTF8.GetBytes(parameters.Password));

            //    Console.WriteLine($"ShopID={Byte2HexString(shopID)}");
            //    Console.WriteLine($"PaymentAttemptID={Byte2HexString(paymentAttemptID)}");
            //    Console.WriteLine($"Subtotal={Byte2HexString(subtotal)}");
            //    Console.WriteLine($"Password={Byte2HexString(password)}");
            //}
            //--------------------------------------------------------------------------------------

            using (var md5 = MD5.Create())
            {
                var parts = parameters.GetParts()
                                      .Select(p => p.SignPart(md5))
                                      .ToArray();

                //var signature = string.Join("&", parts).Sign(md5).ToUpper();
                var signature = string.Concat(parts).Sign(md5).ToUpper();
                return signature;
            }
        }

        private static IEnumerable<string> GetParts(this CreateFiscalCheckRequest parameters)
        {
            // ShopID
            yield return parameters.ShopID;
            // PaymentAttemptID
            yield return parameters.PaymentAttemptID;
            // Subtotal
            yield return parameters.Subtotal.ToString("F2", CultureInfo.InvariantCulture);
            // password
            yield return parameters.Password;
        }

        private static string SignPart(this string data, MD5 md5)
        {
            return string.IsNullOrEmpty(data)
                       ? GetEmptyPart(md5)
                       : data.Sign(md5);
        }

        private static string Sign(this string data, MD5 md5)
        {
            var signatureBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(data));
            var signature = string.Join(string.Empty, signatureBytes.Select(b => b.ToString("x2")));
            return signature;
        }

        private static string _emptyPart;

        private static string GetEmptyPart(MD5 md5)
        {
            return _emptyPart ?? (_emptyPart = string.Empty.Sign(md5));
        }
    }
}
