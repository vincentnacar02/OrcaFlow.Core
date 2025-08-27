namespace Orca
{
    public class Orchestrator<TContext>
    {
        private readonly IReadOnlyList<Func<TContext, ITask<TContext>>> _taskFactories;
        private readonly OrchestratorOptions<TContext> _options;

        public Orchestrator(IEnumerable<Func<TContext, ITask<TContext>>> taskFactories,
                            OrchestratorOptions<TContext> options)
        {
            _taskFactories = taskFactories.ToList();
            _options = options;
        }

        public async Task RunAsync(TContext ctx, CancellationToken token = default)
        {
            foreach (var factory in _taskFactories)
            {
                token.ThrowIfCancellationRequested();
                var task = factory(ctx);

                try
                {
                    if (_options.OnStepStarted != null)
                        await _options.OnStepStarted(task, ctx);

                    await task.ExecuteAsync(ctx, token);

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
