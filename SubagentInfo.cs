using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.OTAFinance.Integration.Uniteller.FiscalizationPayment
{
    public class SubagentInfo
    {
        public int Id { get; internal set; }
        public int SubagentId { get; internal set; }
        public string PrimaryEmail { get; internal set; }
        public string SecondaryEmail { get; internal set; }
        public string TertiaryEmail { get; internal set; }
        public string Phone { get; internal set; }
        public string Inn { get; internal set; }
        public string Name { get; internal set; }
        public string ContractNumber { get; internal set; }
        public DateTime? СontractDate{ get; internal set; }
        public string Description { get; internal set; }
        public bool Deleted { get; internal set; }
    }
}
