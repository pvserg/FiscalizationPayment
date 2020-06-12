using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AAP.OTAFinance.Integration.Uniteller.FiscalizationPayment
{
    public interface IEmailModel
    {
        string Subject { get; }
        string Title { get; }
        MailAddress ToAddress { get; set; }
        MailAddress FromAddress { get; }
        string AttachmentReport { get; }
    }

    public class EmailModel : IEmailModel
    {
        public EmailModel()
        {
        }

        public string Title { get; private set; }
        public string Subject { get; set; }
        public MailAddress ToAddress { get; set; }
        public MailAddress FromAddress { get; set; }
        public string AttachmentReport { get; set; }
    }
}
