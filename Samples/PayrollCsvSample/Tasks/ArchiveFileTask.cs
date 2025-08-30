using Microsoft.Extensions.Logging;
using Orca;
namespace PayrollCsvSample.Tasks
{
    public class ArchiveFileTask : ITask<PayrollContext>
    {
        private readonly ILogger<ArchiveFileTask> _logger;
        public string Name => nameof(ArchiveFileTask);

        public ArchiveFileTask(ILogger<ArchiveFileTask> logger) => _logger = logger;

        public Task ExecuteAsync(PayrollContext ctx, CancellationToken token = default)
        {
            if (!Directory.Exists(ctx.ArchiveFolder))
                Directory.CreateDirectory(ctx.ArchiveFolder);

            var fileName = Path.GetFileName(ctx.InputFile);
            var archivePath = Path.Combine(ctx.ArchiveFolder, fileName);

            File.Move(ctx.InputFile, archivePath, overwrite: true);
            _logger.LogInformation("[Archive] Moved {File} → {Archive}", ctx.InputFile, archivePath);

            return Task.CompletedTask;
        }
    }
}
