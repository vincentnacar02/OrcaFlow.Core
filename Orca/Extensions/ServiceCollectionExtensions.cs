using Microsoft.Extensions.DependencyInjection;
namespace Orca.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrchestrator<TContext>(
            this IServiceCollection services,
            Action<OrchestratorBuilder<TContext>> configureBuilder)
            where TContext : class
        {
            // Register Orchestrator<TContext> as a singleton (or scoped if preferred)
            services.AddSingleton<Orchestrator<TContext>>(sp =>
            {
                var builder = new OrchestratorBuilder<TContext>()
                    .UseServiceProvider(sp); // hook up DI resolution for tasks

                configureBuilder(builder);

                return builder.Build();
            });
            return services;
        }
    }
}
