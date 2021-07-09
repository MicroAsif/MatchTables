using System;
using System.Collections.Generic;
using System.Text;
using MatchTables.Models;

namespace MatchTables.ViewModel
{
    public class SyncViewModel
    {
        public List<Customer> Added { get; set; }
        public List<Customer> Modified { get; set; }
        public List<Customer> Deleted { get; set; }
        public string TargetedTable { get; set; }
    }
}
