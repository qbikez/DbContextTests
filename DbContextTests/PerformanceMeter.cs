using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests
{
    class PerformanceMeter
    {
        public string OutFile { get; }
        public int LoopsCount { get; }

        public PerformanceMeter(string outFile, int loopsCount)
        {
            OutFile = outFile;
            LoopsCount = loopsCount;
        }

        public void MeasurePerf(Action action, [CallerMemberName] string callerName = null)
        {
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < LoopsCount; i++)
            {
                action();
            }

            Trace.WriteLine($"[{callerName}] elapsed: {sw.Elapsed}");

            var csvRow = new PerfCsvRow()
            {
                TestName = callerName,
                Loops = LoopsCount,
                Elapsed = sw.Elapsed,
                ElapsedMs = sw.ElapsedMilliseconds,
            };

            ExtractTestNameData(callerName, ref csvRow);

            AppendCsvRow(OutFile, csvRow);
        }

        private void ExtractTestNameData(string testName, ref PerfCsvRow csvRow)
        {
            testName = testName.ToLower();
            csvRow.IsRollback = testName.Contains("rollback");
            csvRow.TransactionType = testName.Contains("no_transaction") ? "None"
                : testName.Contains("db_transaction") || testName.Contains("dbtransaction") ? "DatabaseTransaction"
                : testName.Contains("_transaction_") ? "TransactionScope"
                : "?";
            csvRow.ContextCount = testName.Contains("multiple") ? 2 : 1;
            csvRow.ContextTypeCount = testName.Contains("different_contexts") ? 2 : 1;
        }

        private void AppendCsvRow(string perfLogFile, PerfCsvRow row)
        {
            if (!File.Exists(perfLogFile)) File.AppendAllText(perfLogFile, row.HeeaderRow() + "\r\n");
            File.AppendAllText(perfLogFile, row.ToCsvString() + "\r\n");
        }

    }
}
