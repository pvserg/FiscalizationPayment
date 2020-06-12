using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AAP.OTAFinance.Integration.Uniteller.FiscalizationPayment
{
    public static class UnitellerFiscalizationHelper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static int? GetPaymentId(string str)
        {
            try
            {
                if (string.IsNullOrEmpty(str))
                {
                    return null;
                }
                int id;
                string[] comments = str.Split(' ');
                str = str.Trim();
                if (str.Length == 0 || str.Length < 3)
                {
                    return null;
                }
                Regex regex = new Regex(@"ID(\d+)");
                var match = regex.Match(str);
                if (!match.Success)
                {
                    return null;
                }

                if (match.Groups[1] != null)
                {
                    var strId = match.Groups[1].Value;
                    if (int.TryParse(strId, out id))
                    {
                        return id;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "UnitellerFiscalizationHelper.GetPaymentId Exception: ");
                return null;
            }
        }

        public static bool IsEMailAddress(string str)
        {
            const string patternEmail = @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$";
            str = str.Trim();
            if (str.Length == 0)
                return false;
            return Regex.IsMatch(str, patternEmail);
        }

        public static string GetId(string str)
        {
            str = str.Trim();
            if (str.Length == 0 || str.Length < 3)
            {
                return null;
            }
            Regex regex = new Regex(@"ID(\d+)");
            var match = regex.Match(str);
            if (!match.Success)
            {
                return null;
            }

            if (match.Groups[1] != null)
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        public static bool IsEqualEmail(string ownerEmail, string subagentEmail)
        {
            if (string.IsNullOrEmpty(ownerEmail) || string.IsNullOrEmpty(subagentEmail))
            {
                return false;
            }
            return (string.Compare(ownerEmail, subagentEmail, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}
