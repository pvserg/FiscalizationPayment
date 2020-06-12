using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.OTAFinance.Integration.Uniteller.FiscalizationPayment
{
    public enum EFiscalizationPaymentState : short
    {
        Unknown = 0,
        Processing = 1,
        Warning = 2,
        Error = 3,
        CriticalError = 4,
        PrintFiscalCheck = 5
    }
}
