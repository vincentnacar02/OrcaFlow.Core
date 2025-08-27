namespace Orca
{
    public class NoOpTask<TContext> : ITask<TContext>
    {
        public string Name { get; }

        public NoOpTask(string name) => Name = name;

        public Task ExecuteAsync(TContext context, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }
    }

}
