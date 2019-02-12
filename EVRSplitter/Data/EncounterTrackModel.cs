using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EVRSplitter
{
    public class EncounterTrackModel
    {
        [Key]
        public long ID { get; set; }
        public long JsonDocId { get; set; }
        public int TradingPartnerId { get; set; }
        public string FileName { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
