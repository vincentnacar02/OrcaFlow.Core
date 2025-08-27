namespace Orca.Tests
{
    public class OrchestratorTests
    {
        public class TestContext
        {
            public List<string> Log { get; set; } = new();
        }

        public class AppendTask : ITask<TestContext>
        {
            private readonly string _message;
            public AppendTask(string message) => _message = message;
            public string Name => $"Append({_message})";
            public Task ExecuteAsync(TestContext context, CancellationToken token = default)
            {
                context.Log.Add(_message);
                return Task.CompletedTask;
            }
        }

        public class FailTask : ITask<TestContext>
        {
            public string Name => nameof(FailTask);
            public Task ExecuteAsync(TestContext context, CancellationToken token = default) => throw new InvalidOperationException("boom");
        }

        // Basic execution order
        [Fact]
        public async Task RunsTasksInOrder()
        {
            var orch = new OrchestratorBuilder<TestContext>()
                .AddStep(new AppendTask("A"))
                .AddStep(new AppendTask("B"))
                .Build();

            var ctx = new TestContext();
            await orch.RunAsync(ctx);

            Assert.Equal(new[] { "A", "B" }, ctx.Log);
        }

        // Error handling: StopOnError
        [Fact]
        public async Task StopsOnFirstError()
        {
            var orch = new OrchestratorBuilder<TestContext>()
                .AddStep(new AppendTask("Start"))
                .AddStep(new FailTask())
                .AddStep(new AppendTask("AfterError"))
                .Configure(o => o.ErrorStrategy = ErrorHandlingStrategy.StopOnError)
                .Build();

            var ctx = new TestContext();
            await Assert.ThrowsAsync<InvalidOperationException>(() => orch.RunAsync(ctx));

            Assert.Equal(new[] { "Start" }, ctx.Log); // last step not run
        }

        // Error handling: SkipFailed
        [Fact]
        public async Task SkipsFailedTask()
        {
            var orch = new OrchestratorBuilder<TestContext>()
                .AddStep(new AppendTask("Start"))
                .AddStep(new FailTask())
                .AddStep(new AppendTask("Recovered"))
                .Configure(o => o.ErrorStrategy = ErrorHandlingStrategy.SkipFailed)
                .Build();

            var ctx = new TestContext();
            await orch.RunAsync(ctx);

            Assert.Equal(new[] { "Start", "Recovered" }, ctx.Log);
        }

        // Conditional execution
        [Fact]
        public async Task SkipsTaskIfConditionFails()
        {
            var orch = new OrchestratorBuilder<TestContext>()
                .AddStep(new AppendTask("Always"))
                .AddStep(new AppendTask("Conditional"), ctx => false)
                .Build();

            var ctx = new TestContext();
            await orch.RunAsync(ctx);

            Assert.Equal(new[] { "Always" }, ctx.Log);
        }

        // Logging hooks fire
        [Fact]
        public async Task CallsLoggingHooks()
        {
            var started = new List<string>();
            var completed = new List<string>();

            var orch = new OrchestratorBuilder<TestContext>()
                .AddStep(new AppendTask("Step1"))
                .Configure(o =>
                {
                    o.OnStepStarted = (task, ctx) =>
                    {
                        started.Add(task.Name);
                        return Task.CompletedTask;
                    };
                    o.OnStepCompleted = (task, ctx) =>
                    {
                        completed.Add(task.Name);
                        return Task.CompletedTask;
                    };
                })
                .Build();

            var ctx = new TestContext();
            await orch.RunAsync(ctx);

            Assert.Single(started, "Append(Step1)");
            Assert.Single(completed, "Append(Step1)");
        }

        // Parallel execution
        [Fact]
        public async Task RunsParallelTasksTogether()
        {
            var orch = new OrchestratorBuilder<TestContext>()
                .AddStep(new ParallelGroupTask<TestContext>("Parallel", new ITask<TestContext>[]
                {
                new AppendTask("One"),
                new AppendTask("Two")
                }))
                .Build();

            var ctx = new TestContext();
            await orch.RunAsync(ctx);

            Assert.Contains("One", ctx.Log);
            Assert.Contains("Two", ctx.Log);
        }
    }
}