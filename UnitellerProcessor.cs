using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AAP.OTAFinance.Integration.Uniteller.FiscalizationPayment
{
    public class UnitellerProcessor
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private const string unitellerCheckUrl = "https://fpay.uniteller.ru/v2/api/iacheck";

        private const string unitellerPayUrl = "https://fpay.uniteller.ru/v2/api/iapay";

        private string _password;

        public UnitellerProcessor(string password)
        {
            _password = password;
        }

        public string GetPaymentAttempt(ref FiscalizationData fiscal)
        {
            try
            {
                var getPaymentAttemptRequest = new GetPaymentAttemptRequest
                {
                    ShopID = fiscal.ShopID,
                    OrderID = fiscal.OrderID,
                    Password = _password
                };

                var signature = getPaymentAttemptRequest.Signature();
                getPaymentAttemptRequest.Signature = signature;

                var requestContent = RequestFormedHelper.ToFormUrlEncodedContent(getPaymentAttemptRequest);
                var responseXmlString = GetResponse(unitellerCheckUrl, requestContent);
                var response = DeserializeXml<XmlResponse>(responseXmlString);

                if (response == null ||
                    !string.Equals(response.Result, "0") ||
                    response.PaymentAttemptID == null)
                {
                    var errorMessage = (response == null || string.IsNullOrEmpty(response.ErrorMessage)) ? "GetPaymentAttemptError" : response.ErrorMessage;
                    logger.Error($"UnitellerProcessor.CreateFiscalCheck ShopId = {fiscal.ShopID}, OrderID = {fiscal.OrderID}, СustomerId = {fiscal.СustomerId}, " +
                            $"Error = {errorMessage}");
                    fiscal.ErrorMessage = errorMessage;
                    return string.Empty;
                }

                return response.PaymentAttemptID;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"UnitellerProcessor.GetPaymentAttempt Exception ShopId = {fiscal.ShopID}, OrderID = {fiscal.OrderID} ");
                fiscal.ErrorMessage = ex.Message;
                return string.Empty;
            }           
        }


        internal bool CreateFiscalCheck(ref FiscalizationData fiscal)
        {

            try
            {
                var customer = new Сustomer
                {
                    id = fiscal.СustomerId,
                    phone = fiscal.СustomerPhone,
                    email = fiscal.СustomerEmail,
                    name = fiscal.СustomerName,
                    inn = fiscal.СustomerInn
                };

                var lines = new Lines
                {
                    name = fiscal.СustomerName,
                    price = fiscal.PaidTotal,
                    qty = 1,
                    sum = fiscal.PaidTotal,
                    vat = VAT.NotVAT,
                    payattr = PayAttribute.PrepaidExpense,
                    lineattr = 10,
                };

                var payments = new Payments
                {
                    kind = PaymentsKind.PaymentByAdditionalMeansPayment,
                    type = PaymentsType.ExternalAcquiring,
                    amount = fiscal.PaidTotal
                };

                var receipt = new Receipt
                {
                    customer = customer,
                    lines = new List<Lines> { lines },
                    taxmode = Taxmode.SimplifiedTaxSystemIncomeMinusExpense,
                    payments = new List<Payments> { payments },
                    total = fiscal.PaidTotal
                };

                fiscal.Receipt = JsonConvert.SerializeObject(receipt);

                var createFiscalCheckRequest = new CreateFiscalCheckRequest
                {
                    ShopID = fiscal.ShopID,
                    PaymentAttemptID = fiscal.PaymentAttemptID,
                    Subtotal = fiscal.PaidTotal,
                    Receipt = fiscal.Receipt,
                    Password = _password
                };

                createFiscalCheckRequest.Receipt = Base64Encode(fiscal.Receipt);
                createFiscalCheckRequest.ReceiptSignature = createFiscalCheckRequest.ReceiptSignature();
                createFiscalCheckRequest.Signature = createFiscalCheckRequest.Signature();

                var requestContent = RequestFormedHelper.ToFormUrlEncodedContent(createFiscalCheckRequest);
                var responseXmlString = GetResponse(unitellerPayUrl, requestContent);
                var response = DeserializeXml<XmlResponse>(responseXmlString);

                if (response == null || !string.Equals(response.Result, "0"))
                {                  
                    var errorMessage = (response == null || string.IsNullOrEmpty(response.ErrorMessage)) ? "CreateFiscalCheckError" : response.ErrorMessage;
                    logger.Error($"UnitellerProcessor.CreateFiscalCheck ShopId = {fiscal.ShopID}, OrderID = {fiscal.OrderID}, СustomerId = {fiscal.СustomerId}, " +
                        $"Error = {errorMessage}");
                    fiscal.ErrorMessage = errorMessage;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"UnitellerProcessor.CreateFiscalCheck Exception ShopId = {fiscal.ShopID}, OrderID = {fiscal.OrderID}, СustomerId = {fiscal.СustomerId}");
                fiscal.ErrorMessage = ex.Message;
                return false;
            }

        }

        public static string Base64Encode(string txt)
        {
            var txtBytes = System.Text.Encoding.UTF8.GetBytes(txt);
            return System.Convert.ToBase64String(txtBytes);
        }

        private string GetResponse(string url, HttpContent content)
        {
            string responseString = null;
            try
            {
                var requestString = content.ReadAsStringAsync().Result;
                logger.Info($"Отправка запроса к Uniteller: POST {url} {requestString}");

                HttpStatusCode responseStatus;
               
                using (var client = new HttpClient())
                using (var response = client.PostAsync(url, content).Result)
                {
                    responseStatus = response.StatusCode;
                    if (responseStatus == HttpStatusCode.OK)
                    {
                        responseString = response.Content.ReadAsStringAsync().Result;
                    }
                }

                logger.Info($"Получен ответ от Uniteller: [status - {responseStatus}] {responseString}");
                if (responseString == null)
                {
                    throw new Exception("не удалось получить ответ от Uniteller");
                }
                return responseString;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"UnitellerProcessor.GetResponse Exception");
                responseString = null;
            }

            return responseString;
        }


        private static T DeserializeXml<T>(string xmlString)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(xmlString))
            {
                var obj = (T)serializer.Deserialize(reader);

                return obj;
            }
        }
    }
}
