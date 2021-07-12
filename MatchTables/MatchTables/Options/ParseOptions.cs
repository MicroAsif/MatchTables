using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace MatchTables.Options
{
    public class ParseOptions
    {
        [Option('s', "table1", Required = true, HelpText = "Input source table name to be processed.")]
        public string table1 { get; set; }

        [Option('t', "table2", Required = true, HelpText = "Input target table name to be processed.")]
        public string table2 { get; set; }

        [Option('p', "primarykey", Required = true, HelpText = "Input primarykey name to be processed.")]
        public string primarykey { get; set; }
    }
}
