using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Globalization;

namespace AAP.OTAFinance.Integration.Uniteller.FiscalizationPayment
{
    internal static class CreateFiscalCheckReceiptSignature
    {
        public static string ReceiptSignature(this CreateFiscalCheckRequest parameters)
        {
            //------------------------ TEST ---------------------------------------------------------
            // проверка
            //using (var sha256 = SHA256.Create())
            //{
            //    var Shop_IDP = sha256.ComputeHash(Encoding.UTF8.GetBytes(parameters.ShopID));
            //    var PaymentAttemptID = sha256.ComputeHash(Encoding.UTF8.GetBytes(parameters.PaymentAttemptID));
            //    var Subtotal_P = sha256.ComputeHash(Encoding.UTF8.GetBytes(parameters.Subtotal.ToString("F2", CultureInfo.InvariantCulture)));
            //    var Receipt = sha256.ComputeHash(Encoding.UTF8.GetBytes(parameters.Receipt));
            //    var Password = sha256.ComputeHash(Encoding.UTF8.GetBytes(parameters.Password));

            //    Console.WriteLine($"ShopID={Byte2HexString(Shop_IDP)}");
            //    Console.WriteLine($"PaymentAttemptID={Byte2HexString(PaymentAttemptID)}");
            //    Console.WriteLine($"Subtotal={Byte2HexString(Subtotal_P)}");
            //    Console.WriteLine($"Receipt={Byte2HexString(Receipt)}");
            //    Console.WriteLine($"Password={Byte2HexString(Password)}");
            //}
            //--------------------------------------------------------------------------------------

            using (var sha256 = SHA256.Create())
            {
                var parts = parameters.GetParts()
                                      .Select(p => p.SignPart(sha256))
                                      .ToArray();

                var signature = string.Join("&", parts).Sign(sha256).ToUpper();
                return signature;
            }
        }

        public static string Byte2HexString(byte[] signatureBytes)
        {
            return string.Join(string.Empty, signatureBytes.Select(b => b.ToString("x2")));
        }

        private static IEnumerable<string> GetParts(this CreateFiscalCheckRequest parameters)
        {
            // Shop_IDP
            yield return parameters.ShopID;
            // PaymentAttemptID
            yield return parameters.PaymentAttemptID;
            // Subtotal
            yield return parameters.Subtotal.ToString("F2", CultureInfo.InvariantCulture);
            // Receipt
            yield return parameters.Receipt;
            // password
            yield return parameters.Password;
        }

        private static string SignPart(this string data, SHA256 sha256)
        {
            return string.IsNullOrEmpty(data)
                       ? GetEmptyPart(sha256)
                       : data.Sign(sha256);
        }

        private static string Sign(this string data, SHA256 sha256)
        {
            var signatureBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            var signature = string.Join(string.Empty, signatureBytes.Select(b => b.ToString("x2")));
            return signature;
        }

        private static string _emptyPart;

        private static string GetEmptyPart(SHA256 sha256)
        {
            return _emptyPart ?? (_emptyPart = string.Empty.Sign(sha256));
        }
    }
}
