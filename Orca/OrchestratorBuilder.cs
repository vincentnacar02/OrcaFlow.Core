namespace Orca
{
    public class OrchestratorBuilder<TContext>
    {
        private readonly List<Func<TContext, ITask<TContext>>> _taskFactories = new();
        private readonly OrchestratorOptions<TContext> _options = new();

        public OrchestratorBuilder<TContext> AddStep<T>(Func<TContext, bool>? condition = null)
            where T : ITask<TContext>, new()
        {
            _taskFactories.Add(ctx =>
            {
                if (condition != null && !condition(ctx))
                    return new NoOpTask<TContext>($"Skipped {typeof(T).Name}");

                return new T();
            });
            return this;
        }

        public OrchestratorBuilder<TContext> AddStep(ITask<TContext> step, Func<TContext, bool>? condition = null)
        {
            _taskFactories.Add(ctx =>
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

        public Orchestrator<TContext> Build() => new(_taskFactories, _options);
    }

}
