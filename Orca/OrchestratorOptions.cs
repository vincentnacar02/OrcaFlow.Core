namespace Orca
{
    public class OrchestratorOptions<TContext>
    {
        public ErrorHandlingStrategy ErrorStrategy { get; set; } = ErrorHandlingStrategy.StopOnError;

        public Func<ITask<TContext>, TContext, Task>? OnStepStarted { get; set; }
        public Func<ITask<TContext>, TContext, Task>? OnStepCompleted { get; set; }
        public Func<ITask<TContext>, Exception, TContext, Task>? OnStepFailed { get; set; }

        internal List<TaskMiddleware<TContext>> Middlewares { get; } = new();

        public OrchestratorOptions<TContext> Use(TaskMiddleware<TContext> middleware)
        {
            Middlewares.Add(middleware);
            return this;
        }
    }

}
