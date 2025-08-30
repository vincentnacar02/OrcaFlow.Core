using Microsoft.Extensions.Logging;
using Orca;
using System.Data;
using System.Globalization;

namespace PayrollCsvSample.Tasks
{
    public class ComputePayrollTask : ITask<PayrollContext>
    {
        private readonly ILogger<ComputePayrollTask> _logger;
        public string Name => nameof(ComputePayrollTask);

        public ComputePayrollTask(ILogger<ComputePayrollTask> logger) => _logger = logger;

        public Task ExecuteAsync(PayrollContext ctx, CancellationToken token = default)
        {
            decimal total = 0;
            foreach (DataRow row in ctx.Timesheet.Rows)
            {
                var rate = decimal.Parse(row["Rate"].ToString()!, CultureInfo.InvariantCulture);
                var hours = decimal.Parse(row["Hours"].ToString()!, CultureInfo.InvariantCulture);
                total += rate * hours;
            }
            ctx.TotalPayroll = total;
            _logger.LogInformation("Computed total payroll: {Total:C}", total);
            return Task.CompletedTask;
        }
    }
}
