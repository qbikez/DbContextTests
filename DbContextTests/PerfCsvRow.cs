using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests
{
    class PerfCsvRow
    {
        public string TestName { get; set; }
        public bool IsRollback { get; set; }
        public string TransactionType { get; set; }
        public int ContextCount { get; set; }
        public int ContextTypeCount { get; set; }
        public int Loops { get; set; }
        public TimeSpan Elapsed { get; set; }
        public long ElapsedMs { get; set; }

        public string HeeaderRow()
        {
            return $"{nameof(TestName)};{nameof(TransactionType)};{nameof(IsRollback)};{nameof(ContextCount)};{nameof(ContextTypeCount)};{nameof(Loops)};{nameof(Elapsed)};{nameof(ElapsedMs)}";
        }
        public string ToCsvString()
        {
            return $"{TestName};{TransactionType};{IsRollback};{ContextCount};{ContextTypeCount};{Loops};{Elapsed};{ElapsedMs}";
        }
    }
}
