using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orca;
using Orca.Extensions;
using PayrollCsvSample;
using PayrollCsvSample.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);
        builder.ConfigureServices((hostContext, services) =>
        {
            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Register tasks
            services.AddTransient<LoadCsvTask>();
            services.AddTransient<ComputePayrollTask>();
            services.AddTransient<ArchiveFileTask>();

            // Register OrcaFlow orchestrator with DI
            services.AddOrchestrator<PayrollContext>(builder =>
            {
                builder.AddStep<LoadCsvTask>()
                       .AddStep<ComputePayrollTask>()
                       .AddStep<ArchiveFileTask>();
            });
        });

        var host = builder.Build();

        var ctx = new PayrollContext
        {
            InputFile = "timesheet.csv",
            ArchiveFolder = "archive"
        };

        using (var scope = host.Services.CreateScope())
        {
            var orchestrator = scope.ServiceProvider.GetRequiredService<Orchestrator<PayrollContext>>();
            await orchestrator.RunAsync(ctx);
        }

        await host.RunAsync();
    }
}
