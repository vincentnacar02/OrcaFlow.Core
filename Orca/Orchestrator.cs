namespace Orca
{
    public class Orchestrator<TContext>
    {
        private readonly IReadOnlyList<Func<IServiceProvider?, TContext, ITask<TContext>>> _taskFactories;
        private readonly OrchestratorOptions<TContext> _options;
        private readonly IServiceProvider? _serviceProvider;

        public Orchestrator(IEnumerable<Func<IServiceProvider?, TContext, ITask<TContext>>> taskFactories,
                            OrchestratorOptions<TContext> options, IServiceProvider? serviceProvider = null)
        {
            _taskFactories = taskFactories.ToList();
            _options = options;
            _serviceProvider = serviceProvider;
        }

        public async Task RunAsync(TContext ctx, CancellationToken token = default)
        {
            foreach (var factory in _taskFactories)
            {
                token.ThrowIfCancellationRequested();
                var task = factory(_serviceProvider, ctx);

                try
                {
                    if (_options.OnStepStarted != null)
                        await _options.OnStepStarted(task, ctx);

                    Func<Task> pipeline = () => task.ExecuteAsync(ctx, token);

                    foreach (var middleware in _options.Middlewares.AsEnumerable().Reverse())
                    {
                        var next = pipeline;
                        pipeline = () => middleware(task, ctx, next, token);
                    }

                    await pipeline();

                    if (_options.OnStepCompleted != null)
                        await _options.OnStepCompleted(task, ctx);
                }
                catch (Exception ex)
                {
                    if (_options.OnStepFailed != null)
                        await _options.OnStepFailed(task, ex, ctx);

                    if (_options.ErrorStrategy == ErrorHandlingStrategy.StopOnError)
                        throw;
                }
            }
        }
    }

}
