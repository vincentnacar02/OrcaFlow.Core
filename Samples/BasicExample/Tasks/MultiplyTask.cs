using Orca;
namespace BasicExample.Tasks
{
    public class MultiplyTask : ITask<Context>
    {
        public string Name => nameof(MultiplyTask);

        public Task ExecuteAsync(Context context, CancellationToken token = default)
        {
            context.Result = context.Result * 10;
            return Task.CompletedTask;
        }
    }
}
