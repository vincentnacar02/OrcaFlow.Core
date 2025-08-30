using Microsoft.Extensions.Logging;
using Orca;
using System.Data;

namespace PayrollCsvSample.Tasks
{
    public class LoadCsvTask : ITask<PayrollContext>
    {
        public string Name => nameof(LoadCsvTask);
        private readonly ILogger<LoadCsvTask> _logger;
        public LoadCsvTask(ILogger<LoadCsvTask> logger) => _logger = logger;

        public async Task ExecuteAsync(PayrollContext ctx, CancellationToken token = default)
        {
            var dt = new DataTable();
            var lines = await File.ReadAllLinesAsync(ctx.InputFile, token);
            var headers = lines[0].Split(',');

            foreach (var header in headers)
                dt.Columns.Add(header);

            foreach (var line in lines.Skip(1))
            {
                var values = line.Split(',');
                dt.Rows.Add(values);
            }

            ctx.Timesheet = dt;
            _logger.LogInformation("Loaded {RowCount} rows from {File}", dt.Rows.Count, ctx.InputFile);
        }
    }
}
