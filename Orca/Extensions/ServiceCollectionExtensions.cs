using Microsoft.Extensions.DependencyInjection;
namespace Orca.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrchestrator<TContext>(
            this IServiceCollection services,
            Action<OrchestratorBuilder<TContext>> configureBuilder,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TContext : class
        {
            services.Add(new ServiceDescriptor(
                    typeof(Orchestrator<TContext>),
                    sp =>
                        {
                            var builder = new OrchestratorBuilder<TContext>()
                                .UseServiceProvider(sp); // Hook up DI for tasks

                            configureBuilder(builder);

                            return builder.Build();
                        },
                    lifetime)
                );
            return services;
        }
    }
}
