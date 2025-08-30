namespace Orca
{
    public class ParallelGroupTask<TContext> : ITask<TContext>
    {
        public string Name { get; }
        private readonly IEnumerable<ITask<TContext>> _tasks;

        public ParallelGroupTask(string name, IEnumerable<ITask<TContext>> tasks)
        {
            Name = name;
            _tasks = tasks;
        }

        public async Task ExecuteAsync(TContext context, CancellationToken token = default)
        {
            await Task.WhenAll(_tasks.Select(t => t.ExecuteAsync(context, token)));
        }
    }
}
