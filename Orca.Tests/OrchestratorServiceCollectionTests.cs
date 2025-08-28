using Microsoft.Extensions.DependencyInjection;
using Orca.Extensions;
namespace Orca.Tests
{
    public class OrchestratorServiceCollectionTests
    {
        // --- Test Context and Tasks ---
        public class TestContext
        {
            public List<string> Log { get; } = new();
        }

        public class FirstTask : ITask<TestContext>
        {
            public string Name => "FirstTask";

            public async Task ExecuteAsync(TestContext context, CancellationToken token = default)
            {
                await Task.Yield();
                context.Log.Add("First");
            }
        }

        public class SecondTask : ITask<TestContext>
        {
            public string Name => "SecondTask";

            public async Task ExecuteAsync(TestContext context, CancellationToken token = default)
            {
                await Task.Yield();
                context.Log.Add("Second");
            }
        }

        public class AppendTask : ITask<List<string>>
        {
            private readonly string _message;
            public AppendTask(string message) => _message = message;
            public string Name => $"Append({_message})";
            public Task ExecuteAsync(List<string> context, CancellationToken token = default)
            {
                context.Add(_message);
                return Task.CompletedTask;
            }
        }

        // --- Tests ---

        [Fact]
        public async Task AddOrchestrator_RegistersAndRunsTasks()
        {
            // Arrange
            var services = new ServiceCollection();

            // Register tasks
            services.AddTransient<FirstTask>();
            services.AddTransient<SecondTask>();

            // Register orchestrator
            services.AddOrchestrator<TestContext>(builder =>
            {
                builder.AddStep<FirstTask>()
                       .AddStep<SecondTask>();
            });

            var sp = services.BuildServiceProvider();
            var orchestrator = sp.GetRequiredService<Orchestrator<TestContext>>();

            var ctx = new TestContext();

            // Act
            await orchestrator.RunAsync(ctx);

            // Assert
            Assert.Equal(new[] { "First", "Second" }, ctx.Log);
        }

        [Fact]
        public void AddOrchestrator_ResolvesMultipleContexts()
        {
            var services = new ServiceCollection();

            services.AddTransient<FirstTask>();
            services.AddTransient<SecondTask>();

            services.AddOrchestrator<TestContext>(b => b.AddStep<FirstTask>());
            services.AddOrchestrator<List<string>>(b => b.AddStep(new AppendTask("Step")));

            var sp = services.BuildServiceProvider();

            // Both orchestrators should be resolved independently
            var orch1 = sp.GetRequiredService<Orchestrator<TestContext>>();
            var orch2 = sp.GetRequiredService<Orchestrator<List<string>>>();

            Assert.NotNull(orch1);
            Assert.NotNull(orch2);
            Assert.NotSame(orch1, orch2); // different orchestrators
        }

        [Fact]
        public async Task AddOrchestrator_UsesOnStepStartedHook()
        {
            var services = new ServiceCollection();
            services.AddTransient<FirstTask>();

            bool hookCalled = false;

            services.AddOrchestrator<TestContext>(builder =>
            {
                builder.AddStep<FirstTask>()
                       .Configure(o =>
                       {
                           o.OnStepStarted = async (task, ctx) =>
                           {
                               hookCalled = true;
                               await Task.Yield();
                           };
                       });
            });

            var sp = services.BuildServiceProvider();
            var orchestrator = sp.GetRequiredService<Orchestrator<TestContext>>();
            await orchestrator.RunAsync(new TestContext());

            Assert.True(hookCalled);
        }
    }
}
