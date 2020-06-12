using AAP.OTAFinance.Data;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AAP.OTAFinance.Integration.Uniteller.FiscalizationPayment
{
    
    public class UnitellerFiscalizationPaymentAdapter
    {
        private static readonly UnitellerFiscalizationPaymentAdapter instance = new UnitellerFiscalizationPaymentAdapter();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        //private const string keyWidget = "widget";

        private readonly object _locker = new object();

        //private const string passwordTest = "k05KeLKX66qQqbTHiDeAYVYvWNaPRyHY5YBBZkyCtVGnM8Erd2qZOL0DACaDkUWSUdnbB5ibkJx7V2K3";

        private const string password = "1YP65OIDzglFExIYmxQY5BxmacN9Hzs2R8E2RLpCpCcz7OtYCDx45MqYeHO74603bVTqkAPIpuhl5Z23";

        private readonly Action<FiscalizationData> RequestNewTaskAction;
        private readonly AsyncPCQueue<FiscalizationData> ControllerNewTaskQueue;

        readonly UnitellerProcessor _unitellerProcessor;
        private EmailModel _emailModel;

        private static Dictionary<ErrorOperationResult, string> ErrorTypeToMessage
        {
            get
            {
                return new Dictionary<ErrorOperationResult, string>
                           {
                               //{ErrorOperationResult.UnknownError, "Неизвестная ошибка"},
                               {ErrorOperationResult.ErrorIdFromComment, "Неправильный ввод ID субагента в виджете"},
                               {ErrorOperationResult.ErrorGetSubagentById, "Не найден субагент - проверить БД субагента"},
                               {ErrorOperationResult.NotValidOwnerEmail, "Невалидный email в виджете"},
                               {ErrorOperationResult.NotFoundSubagentEmail, "Не найден email для субагента"},
                               {ErrorOperationResult.GetPaymentAttemptError, "Ошибка получения идентификатора системы оплаты"},
                               {ErrorOperationResult.CreateFiscalCheckError, "Ошибка создания чека"},
                           };
            }
        }

        private UnitellerFiscalizationPaymentAdapter()
        {
            RequestNewTaskAction = FiscalizationPaymentActionHandler;
            ControllerNewTaskQueue = new AsyncPCQueue<FiscalizationData>(FiscalizationPaymentActionHandler);

            _unitellerProcessor = new UnitellerProcessor(password);

            _emailModel = new EmailModel()
            {
                FromAddress = new MailAddress("unitellererror@aerotur.aero", "UnitellerFiscalizationPayment"),
                ToAddress = new MailAddress("unitellererror@aerotur.aero"),
            };
        }

        public static UnitellerFiscalizationPaymentAdapter GetInstance()
        {
            return instance;
        }

        

        public void UnitellerFiscalizationPayment()
        {
            logger.Info("UnitellerFiscalizationPayment - запущена процедура формирования данных по фискализации и их отправке");
            //List<FiscalizationData> fiscalizationDataList = new List<FiscalizationData>();
            List<ErrorOperationResult> ErrorOperationResultList = new List<ErrorOperationResult>();

            try
            {
                using (OTAFinanceDbDataContext db = new OTAFinanceDbDataContext())
                {
                    // ОСТАВЛЕНО для тестирования
                    //var payment = db.UnitellerOrderPayments.FirstOrDefault(x => x.UnitellerOrderPaymentId.Contains("5ee243234ec347"));
                    //if (payment != null)
                    //{
                    //    {
                    //        // из поля comment надо получить ID субагента
                    //        int? subagentIdFromComment = null;
                    //        if (!string.IsNullOrWhiteSpace(payment.Comment))
                    //        {
                    //            subagentIdFromComment = UnitellerFiscalizationHelper.GetPaymentId(payment.Comment);
                    //        }

                    //        var unitellerResultCheckByWidget = new UnitellerResultCheckByWidget
                    //        {
                    //            UnitellerShopPointId = payment.UnitellerShopPointId,
                    //            UnitellerOrderPaymentId = payment.UnitellerOrderPaymentId,
                    //            UnitellerPaidTotal = payment.PaidTotal,
                    //            UnitellerOwnerEmail = payment.OwnerEmail,
                    //            UnitellerSubagentId = (subagentIdFromComment != null && subagentIdFromComment.HasValue) ? subagentIdFromComment.Value : (int?)null,

                    //            ProcessingStatus = (short)EFiscalizationPaymentState.Unknown,
                    //            ErrorCode = 0
                    //        };

                    //        db.UnitellerResultCheckByWidget.InsertOnSubmit(unitellerResultCheckByWidget);
                    //        db.SubmitChanges();
                    //    }
                    //}

                    // в этой части ничего не анализируем
                    // у нас есть UnitellerResultCheckByWidget и набор данных
                    //UnitellerShopPointId,
                    //UnitellerOrderPaymentId
                    //UnitellerPaidTotal 
                    //UnitellerOwnerEmail - email для отправки чека (мыло из виджета)
                    //UnitellerSubagentId - ID субагента из виджета (поле комментарии)

                    // вытаскиваем из БД нужную информацию, формируем данные, отправляем в очередь
                    var payments = (from p in db.UnitellerResultCheckByWidget
                                    where p.ProcessingStatus == (short)EFiscalizationPaymentState.Unknown
                                    select p).ToList();

                    //List<UnitellerOrderPayment> payments = db.UnitellerOrderPayments
                    //    .Where(t => t.UnitellerOrderPaymentId == null && t.FiscalizationPaymentState != (short)EFiscalizationPaymentState.PrintFiscalCheck)
                    //    .OrderByDescending(t => t.CreateDate).ToList();
                    foreach (var pay in payments)
                    {
                        var data = db.UnitellerResultCheckByWidget.FirstOrDefault(x => x.Id == pay.Id);
                        if(data != null)
                        {                            
                            var fiscalizationData = new FiscalizationData
                            {
                                ShopID = data.UnitellerShopPointId,
                                OrderID = data.UnitellerOrderPaymentId,
                                PaidTotal = (float)data.UnitellerPaidTotal,
                                СustomerId = data.UnitellerSubagentId ?? 0,
                                СustomerEmail = data.UnitellerOwnerEmail,
                            };
                            
                            // выставляем новый статус - обработка
                            data.ProcessingStatus = (short)(EFiscalizationPaymentState.Processing);
                            db.SubmitChanges();

                            ControllerNewTaskQueue.Add(fiscalizationData);
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "UnitellerFiscalizationPaymentAdapter.UnitellerFiscalizationPayment Exception: ");
            }

        }

        private void FiscalizationPaymentActionHandler(FiscalizationData fiscalizationData)
        {
            try
            {
                lock (_locker)
                {
                    if (fiscalizationData == null)
                    {
                        return;
                    }                   

                    // проверяем данные
                    //1. проверим - валидный ли email
                    if (string.IsNullOrEmpty(fiscalizationData.СustomerEmail)
                        || fiscalizationData.СustomerEmail.Length > 255
                        || !UnitellerFiscalizationHelper.IsEMailAddress(fiscalizationData.СustomerEmail))
                    {
                        fiscalizationData.ErrorMessage = ErrorOperationResult.NotValidOwnerEmail.ToString();
                        fiscalizationData.ProcessEndTime = DateTime.Now;
                        UpdateUnitellerResultCheck(EFiscalizationPaymentState.Error, ErrorOperationResult.NotValidOwnerEmail, fiscalizationData);
                        SendEmail(ErrorOperationResult.NotValidOwnerEmail, fiscalizationData);
                        return;
                    }
                    // ID субагента
                    if (fiscalizationData.СustomerId == 0)
                    {
                        // выставляем соотв. статус
                        fiscalizationData.ErrorMessage = ErrorOperationResult.ErrorIdFromComment.ToString();
                        fiscalizationData.ProcessEndTime = DateTime.Now;
                        UpdateUnitellerResultCheck(EFiscalizationPaymentState.Error, ErrorOperationResult.ErrorIdFromComment, fiscalizationData);
                        SendEmail(ErrorOperationResult.ErrorIdFromComment, fiscalizationData);
                        return;
                    }

                    // проверяем, если такой субагент в таблице субагентов
                    var subagentInfo = GetSubagentInfo(fiscalizationData.СustomerId);
                    if (subagentInfo == null)
                    {
                        fiscalizationData.ErrorMessage = ErrorOperationResult.ErrorGetSubagentById.ToString();
                        fiscalizationData.ProcessEndTime = DateTime.Now;
                        UpdateUnitellerResultCheck(EFiscalizationPaymentState.Error, ErrorOperationResult.ErrorGetSubagentById, fiscalizationData);
                        SendEmail(ErrorOperationResult.ErrorGetSubagentById, fiscalizationData);
                        return;
                    }

                    fiscalizationData.СustomerPhone = subagentInfo.Phone;
                    fiscalizationData.СustomerInn = subagentInfo.Inn;
                    fiscalizationData.СustomerName = subagentInfo.Name;
                    fiscalizationData.СustomerContractNumber = subagentInfo.ContractNumber;
                    fiscalizationData.СustomerСontractDate = subagentInfo.СontractDate;

                    // проверяем email субагента
                    if (!(UnitellerFiscalizationHelper.IsEqualEmail(fiscalizationData.СustomerEmail, subagentInfo.PrimaryEmail)
                        || UnitellerFiscalizationHelper.IsEqualEmail(fiscalizationData.СustomerEmail, subagentInfo.SecondaryEmail)
                        || UnitellerFiscalizationHelper.IsEqualEmail(fiscalizationData.СustomerEmail, subagentInfo.TertiaryEmail)))
                    {
                        // не ошибка, но должны уведомить!
                        fiscalizationData.WarningMessage = ErrorTypeToMessage[ErrorOperationResult.NotFoundSubagentEmail];
                        fiscalizationData.ProcessEndTime = DateTime.Now;
                        UpdateUnitellerResultCheck(EFiscalizationPaymentState.Warning, ErrorOperationResult.NotFoundSubagentEmail, fiscalizationData);

                    }


                    var paymentAttemptID = _unitellerProcessor.GetPaymentAttempt(ref fiscalizationData);
                    if (string.IsNullOrEmpty(paymentAttemptID))
                    {
                        logger.Error($"UnitellerFiscalizationPaymentAdapter GetPaymentAttempt вернул пустое значение. ShopID={fiscalizationData.ShopID} " +
                            $"OrderID={fiscalizationData.OrderID} errorMessage={fiscalizationData.ErrorMessage}");

                        fiscalizationData.ProcessEndTime = DateTime.Now;
                        UpdateUnitellerResultCheck(EFiscalizationPaymentState.Error, ErrorOperationResult.GetPaymentAttemptError, fiscalizationData);
                        SendEmail(ErrorOperationResult.GetPaymentAttemptError, fiscalizationData);
                        return;
                    }
                    fiscalizationData.PaymentAttemptID = paymentAttemptID;

                    EFiscalizationPaymentState state;
                    ErrorOperationResult operationResult;
                    var result = _unitellerProcessor.CreateFiscalCheck(ref fiscalizationData);
                    if (result)
                    {
                        state = EFiscalizationPaymentState.PrintFiscalCheck;
                        operationResult = ErrorOperationResult.NoError;

                        logger.Debug($"UnitellerFiscalizationPaymentAdapter CreateFiscalCheck чек успешно создан. ShopID={fiscalizationData.ShopID} " +
                            $"OrderID={fiscalizationData.OrderID}");
                    }
                    else
                    {
                        state = EFiscalizationPaymentState.Error;
                        operationResult = ErrorOperationResult.CreateFiscalCheckError;

                        logger.Error($"UnitellerFiscalizationPaymentAdapter CreateFiscalCheck вернул ошибку. ShopID={fiscalizationData.ShopID} " +
                            $"OrderID={fiscalizationData.OrderID} errorMessage={fiscalizationData.ErrorMessage}");
                    }

                    fiscalizationData.ProcessEndTime = DateTime.Now;
                    UpdateUnitellerResultCheck(state, operationResult, fiscalizationData);

                    SendEmail(operationResult, fiscalizationData);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "UnitellerFiscalizationPaymentAdapter.ControllerNewTaskActionHandler Exception: ");
            }
        }

        private void UpdateUnitellerResultCheck(EFiscalizationPaymentState state, ErrorOperationResult operationResult, FiscalizationData fiscalizationData)
        {
            using (OTAFinanceDbDataContext db = new OTAFinanceDbDataContext())
            {
                var unitellerResultCheck = db.UnitellerResultCheckByWidget.FirstOrDefault(x => string.Equals(x.UnitellerOrderPaymentId, fiscalizationData.OrderID));
                if (unitellerResultCheck != null)
                {
                    unitellerResultCheck.SubagentId = fiscalizationData.СustomerId == 0 ? (int?)null : fiscalizationData.СustomerId;
                    unitellerResultCheck.PaymentAttemptId = string.IsNullOrWhiteSpace(fiscalizationData.PaymentAttemptID) ? null : fiscalizationData.PaymentAttemptID;
                    unitellerResultCheck.Receipt = string.IsNullOrWhiteSpace(fiscalizationData.Receipt) ? null : fiscalizationData.Receipt;
                    unitellerResultCheck.Message = string.IsNullOrWhiteSpace(fiscalizationData.ErrorMessage) ? null : fiscalizationData.ErrorMessage;
                    unitellerResultCheck.ErrorCode = (short)operationResult;                   
                    unitellerResultCheck.ProcessingStatus = (short)state;
                    unitellerResultCheck.ProcessEndTime = fiscalizationData.ProcessEndTime;

                    db.SubmitChanges();
                }

            }
        }

        private void SendEmail(ErrorOperationResult operationResult, FiscalizationData fiscalizationData)
        {
            var errorMessageExternal = string.Empty;
            switch (operationResult)
            {
                case ErrorOperationResult.NoError:
                    _emailModel.Subject = "Чек создан успешно";
                    break;
                case ErrorOperationResult.ErrorIdFromComment:
                case ErrorOperationResult.ErrorGetSubagentById:
                case ErrorOperationResult.NotValidOwnerEmail:
                //case ErrorOperationResult.NotFoundSubagentEmail:
                case ErrorOperationResult.GetPaymentAttemptError:
                case ErrorOperationResult.CreateFiscalCheckError:
                    _emailModel.Subject = "Ошибка выписки чека";
                    errorMessageExternal = ErrorTypeToMessage[operationResult];
                    break;
                default:
                    break;
            }

            _emailModel.AttachmentReport = GetEmailReport(_emailModel.Subject, fiscalizationData, errorMessageExternal);

            SendMail(_emailModel);
            
        }

        private string GetEmailReport(string header, FiscalizationData fiscalizationData, string errorMessageExternal)
        {
           
            // не хотелось возиться с Razor
            StringBuilder sb = new StringBuilder();

            sb.Append(@"<!DOCTYPE html><html lang = ""en"">
                <head>
                <meta http-equiv = ""content-type"" content=""text /html; charset=utf-8""/>
                </head>

                <style>
                body{
                color: #000000;
                margin: 0 auto;
                background-color: #ebebeb;
                padding: 0px;
                width: 700px;
            }
            .block-content{
                background-color: #f6f6f6;
                display: block;
                padding: 40px;
                font-family: sans-serif;
                z-index: 1;
            }           
            p{
                margin-top: 5px;
                margin-bottom: 2px;
                line-height: 2;
                word-break: normal;
            }
            .param{
                font-weight: lighter;
				color: maroon;
                display: block;
                margin-bottom: 0;
                margin-top: 12px;
                line-height: 1.5;
            }
            .data{
                display: block;
                border-bottom: 1px dashed black;
                width: 530px;
                padding-bottom: 15px;
                margin-top: 0;
                line-height: 1.5;
            }
            </style>
                <body>");

            sb.Append(string.Format("<div class=\"block-content\"><div class=\"p1\"><h3 class=\"page-header\">{0}</h3></div>", header));
            sb.Append(string.Format("<p class=\"param\">Дата и время:</p><p class=\"data\">{0}</p>", fiscalizationData.ProcessEndTime.ToString("dd.MM.yyyy HH:mm:ss")));
            sb.Append(string.Format("<p class=\"param\">Идентификатор субагента (SubagentID):</p><p class=\"data\">{0}</p>", fiscalizationData.СustomerId));
            sb.Append(string.Format("<p class=\"param\">Идентификатор точки продажи (ShopID):</p><p class=\"data\">{0}</p>", fiscalizationData.ShopID));
            sb.Append(string.Format("<p class=\"param\">Номер платежа (OrderID):</p><p class=\"data\">{0}</p>", fiscalizationData.OrderID));
            sb.Append(string.Format("<p class=\"param\">Сумма:</p><p class=\"data\">{0} руб.</p>", fiscalizationData.PaidTotal));
            sb.Append(string.Format("<p class=\"param\">Адрес электронной почты субагента:</p><p class=\"data\">{0}</p>", fiscalizationData.СustomerEmail));
            sb.Append(string.Format("<p class=\"param\">Сообщение об ошибках:</p><p class=\"data\">{0}<br>{1}</p>",
                string.IsNullOrEmpty(fiscalizationData.ErrorMessage) ? "нет" : fiscalizationData.ErrorMessage, errorMessageExternal));           

            sb.Append(string.Format("<p class=\"param\">Сообщение о предупреждениях:</p><p class=\"data\">{0}</p>",
                string.IsNullOrEmpty(fiscalizationData.WarningMessage) ? "нет" : fiscalizationData.WarningMessage));

            sb.Append("</div></div></body></html>");

            return sb.ToString();
        }
       
        private SubagentInfo GetSubagentInfo(int сustomerId)
        {
            SubagentInfo subagentInfo = null;
            using (OTAFinanceDbDataContext db = new OTAFinanceDbDataContext())
            {
                var subagent = db.UnitellerFiscalizationPaymentSubagentData.FirstOrDefault(x => x.SubagentId == сustomerId && !x.Deleted);
                if (subagent != null)
                {
                    subagentInfo = new SubagentInfo
                    {
                        Id = subagent.Id,
                        SubagentId = subagent.SubagentId,
                        PrimaryEmail = subagent.PrimaryEmail,
                        SecondaryEmail = subagent.SecondaryEmail,
                        TertiaryEmail = subagent.TertiaryEmail,
                        Phone = subagent.Phone,
                        Inn = subagent.Inn,
                        Name = subagent.Name,
                        ContractNumber = subagent.ContractNumber,
                        СontractDate = subagent.СontractDate,
                        Description = subagent.Description,
                    };
                }
            }

            return subagentInfo;
        }

        
        private string GetErrorMailMessage(string shopID, string orderID, string errorMessage)
        {
            var sb = new StringBuilder();
            sb.Append("Ошибка выписки чека");
            sb.Append(Environment.NewLine);
            sb.Append(string.Format("ShopID={0}, OrderID={1}", shopID, orderID));
            sb.Append(Environment.NewLine);
            sb.Append(errorMessage);

            return sb.ToString();
        }

        private static void SendMail(IEmailModel emailModel)
        {
            using (var mm = new MailMessage())
            {
                mm.From = emailModel.FromAddress;
                mm.To.Add(emailModel.ToAddress);
                mm.IsBodyHtml = true;
                mm.Subject = emailModel.Subject;
                mm.Body = emailModel.AttachmentReport;

                using (var sc = new SmtpClient("mail2.aerotur.aero", 25))
                {
                    ServicePointManager.ServerCertificateValidationCallback = ServerCertificateValidationCallback;
                    //sc.EnableSsl = true;
                    sc.DeliveryMethod = SmtpDeliveryMethod.Network;
                    sc.UseDefaultCredentials = false;
                    sc.Credentials = new NetworkCredential("ticketfinder@aerotur.aero", "rtbKLier7ft2D6A");
                    sc.Send(mm);
                    ServicePointManager.ServerCertificateValidationCallback = null;
                }
                
            }
        }

        private static bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }

}
