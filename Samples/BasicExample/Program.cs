using BasicExample;
using BasicExample.Tasks;
using Orca;

var orchestrator = new OrchestratorBuilder<Context>()
    .AddStep<AddTask>()
    .AddStep<MultiplyTask>()
    .Configure(opts =>
    {
        opts.OnStepStarted = async (task, ctx) =>
        {
            Console.WriteLine($"Started {task.Name}");
        };
    })
    .Build();

var ctx = new Context
{
    Num1 = 1,
    Num2 = 2
};
await orchestrator.RunAsync(ctx);

Console.WriteLine(ctx.Result);