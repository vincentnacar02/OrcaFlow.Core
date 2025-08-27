namespace Orca
{
    public interface ITask<TContext>
    {
        string Name { get; }
        Task ExecuteAsync(TContext context, CancellationToken token = default);
    }
}
