using System;
using System.Collections.Generic;
using System.Text;

namespace EVRSplitter
{
    public class EVRDetail
    {
        public int TradingPartnerId { get; set; }
        public string TradingPartnerName { get; set; }
        public string SubmitterClaimIdentification { get; set; }
        public string IEHPEncounterID { get; set; }
        public string EncounterStatus { get; set; }
        public long JsonDocId { get; set; }
        public string Severity { get; set; }
        public string IssueId { get; set; }
        public bool IsSNIP { get; set; }
        public string Description { get; set; }
    }

    public class EVRByTradingPartner
    {
        public int TradingPartnerId { get; set; }
        public int Counts { get; set; }
    }

}
