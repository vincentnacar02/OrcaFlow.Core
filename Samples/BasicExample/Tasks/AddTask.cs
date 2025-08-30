using Orca;

namespace BasicExample.Tasks
{
    public class AddTask : ITask<Context>
    {
        public string Name => nameof(AddTask);

        public Task ExecuteAsync(Context context, CancellationToken token = default)
        {
            context.Result = context.Num1 + context.Num2;
            return Task.CompletedTask;
        }
    }
}
