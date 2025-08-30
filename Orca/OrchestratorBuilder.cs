namespace Orca
{
    public delegate Task TaskMiddleware<TContext>(
        ITask<TContext> task,
        TContext ctx,
        Func<Task> next,
        CancellationToken token);

    public class OrchestratorBuilder<TContext>
    {
        private readonly List<Func<IServiceProvider?, TContext, ITask<TContext>>> _taskFactories = new();
        private readonly OrchestratorOptions<TContext> _options = new();
        private IServiceProvider? _serviceProvider;

        public OrchestratorBuilder<TContext> UseServiceProvider(IServiceProvider provider)
        {
            _serviceProvider = provider;
            return this;
        }

        public OrchestratorBuilder<TContext> AddStep<T>(Func<TContext, bool>? condition = null)
            where T : class, ITask<TContext>
        {
            _taskFactories.Add((sp, ctx) =>
            {
                if (condition != null && !condition(ctx))
                    return new NoOpTask<TContext>($"Skipped {typeof(T).Name}");

                return (ITask<TContext>)(sp?.GetService(typeof(T)) ?? Activator.CreateInstance<T>());
            });
            return this;
        }

        public OrchestratorBuilder<TContext> AddStep(ITask<TContext> step, Func<TContext, bool>? condition = null)
        {
            _taskFactories.Add((sp, ctx) =>
            {
                if (condition != null && !condition(ctx))
                    return new NoOpTask<TContext>($"Skipped {step.Name}");

                return step;
            });
            return this;
        }

        public OrchestratorBuilder<TContext> Configure(Action<OrchestratorOptions<TContext>> configure)
        {
            configure(_options);
            return this;
        }

        public Orchestrator<TContext> Build() => new(_taskFactories, _options, _serviceProvider);
    }

}
