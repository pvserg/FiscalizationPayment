using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.OTAFinance.Integration.Uniteller.FiscalizationPayment
{
    public enum ErrorOperationResult
    {
        NoError = 0,
        ErrorIdFromComment,
        ErrorGetSubagentById,
        NotValidOwnerEmail,
        NotFoundSubagentEmail,
        GetPaymentAttemptError,
        CreateFiscalCheckError
    }

}
