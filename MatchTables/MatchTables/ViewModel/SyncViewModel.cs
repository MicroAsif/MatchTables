using System;
using System.Collections.Generic;
using System.Text;

namespace MatchTables.ViewModel
{
    public class SyncViewModel
    {
        public List<Dictionary<string, object>> Added { get; set; }
        public List<Dictionary<string, object>> Modified { get; set; }
        public List<Dictionary<string, object>> Exising { get; set; }
        public List<Dictionary<string, object>> Deleted { get; set; }
        public string TargetedTable { get; set; }
    }
}
