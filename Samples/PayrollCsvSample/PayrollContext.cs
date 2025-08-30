using System.Data;
namespace PayrollCsvSample
{
    public class PayrollContext
    {
        public string InputFile { get; set; } = string.Empty;
        public DataTable Timesheet { get; set; } = new();
        public decimal TotalPayroll { get; set; }
        public string ArchiveFolder { get; set; } = "archive";
    }
}
