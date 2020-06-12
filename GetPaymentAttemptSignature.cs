using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AAP.OTAFinance.Integration.Uniteller.FiscalizationPayment
{
    internal static class GetPaymentAttemptSignature
    {
        public static string Byte2HexString(byte[] signatureBytes)
        {
            return string.Join(string.Empty, signatureBytes.Select(b => b.ToString("x2")));
        }

        public static string Signature(this GetPaymentAttemptRequest parameters)
        {
            //------------------------ TEST ---------------------------------------------------------
            // проверка
            //using (var md5 = MD5.Create())
            //{
            //    var shopID = md5.ComputeHash(Encoding.UTF8.GetBytes(parameters.ShopID));
            //    var orderID = md5.ComputeHash(Encoding.UTF8.GetBytes(parameters.OrderID));
            //    var password = md5.ComputeHash(Encoding.UTF8.GetBytes(parameters.Password));

            //    Console.WriteLine($"ShopID={Byte2HexString(shopID)}");
            //    Console.WriteLine($"OrderID={Byte2HexString(orderID)}");
            //    Console.WriteLine($"Password={Byte2HexString(password)}");
            //}
            //---------------------------------------------------------------------------------------

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

        private static IEnumerable<string> GetParts(this GetPaymentAttemptRequest parameters)
        {
            // Shop_IDP
            yield return parameters.ShopID;
            // Order_IDP
            yield return parameters.OrderID;
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
