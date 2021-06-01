using System;
using System.Linq;
using System.IO;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Columns;

namespace MetricBenchmark
{
    public class SizeColumn : IColumn
    {
        private readonly Func<string, string> getTag;

        public string Id { get; }
        public string ColumnName { get; }
        private string filename;

        public SizeColumn(string columnName, string filename, Func<string, string> getTag)
        {
            this.filename = filename;
            this.getTag = getTag;
            ColumnName = columnName;
            Id = nameof(TagColumn) + "." + ColumnName;
        }

        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        {
            string result = "N/A";

            var lines = File.ReadLines(this.filename);
            foreach (var line in lines)
            {
                Console.WriteLine(line);
                var values = line.Split("=");
                if (values[0] == benchmarkCase.FolderInfo)
                {
                    result = values[1];
                }
            }

            return result;
        }

        public bool IsAvailable(Summary summary) => true;
        public bool AlwaysShow => true;
        public ColumnCategory Category => ColumnCategory.Custom;
        public int PriorityInCategory => 0;
        public bool IsNumeric => false;
        public UnitType UnitType => UnitType.Dimensionless;
        public string Legend => $"Custom '{ColumnName}' tag column";
        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);
        public override string ToString() => ColumnName;
    }
}
