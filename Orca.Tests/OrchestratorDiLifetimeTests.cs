using Microsoft.Extensions.DependencyInjection;
using Orca.Extensions;

namespace Orca.Tests
{
    public class OrchestratorDiLifetimeTests
    {
        private class DummyContext { }
        private class DummyTask : ITask<DummyContext>
        {
            public string Name => nameof(DummyTask);
            public Task ExecuteAsync(DummyContext context, CancellationToken token = default)
                => Task.CompletedTask;
        }

        [Fact]
        public void TransientLifetime_ShouldCreateDifferentInstances()
        {
            var services = new ServiceCollection();
            services.AddOrchestrator<DummyContext>(builder =>
            {
                builder.AddStep<DummyTask>();
            }, ServiceLifetime.Transient);

            var sp = services.BuildServiceProvider();

            var orch1 = sp.GetRequiredService<Orchestrator<DummyContext>>();
            var orch2 = sp.GetRequiredService<Orchestrator<DummyContext>>();

            Assert.NotSame(orch1, orch2);
        }

        [Fact]
        public void SingletonLifetime_ShouldReturnSameInstance()
        {
            var services = new ServiceCollection();
            services.AddOrchestrator<DummyContext>(builder =>
            {
                builder.AddStep<DummyTask>();
            }, ServiceLifetime.Singleton);

            var sp = services.BuildServiceProvider();

            var orch1 = sp.GetRequiredService<Orchestrator<DummyContext>>();
            var orch2 = sp.GetRequiredService<Orchestrator<DummyContext>>();

            Assert.Same(orch1, orch2);
        }

        [Fact]
        public void ScopedLifetime_ShouldShareWithinScopeButDifferentAcrossScopes()
        {
            var services = new ServiceCollection();
            services.AddOrchestrator<DummyContext>(builder =>
            {
                builder.AddStep<DummyTask>();
            }, ServiceLifetime.Scoped);

            var sp = services.BuildServiceProvider();

            // First scope
            using var scope1 = sp.CreateScope();
            var orch1a = scope1.ServiceProvider.GetRequiredService<Orchestrator<DummyContext>>();
            var orch1b = scope1.ServiceProvider.GetRequiredService<Orchestrator<DummyContext>>();

            // Second scope
            using var scope2 = sp.CreateScope();
            var orch2 = scope2.ServiceProvider.GetRequiredService<Orchestrator<DummyContext>>();

            Assert.Same(orch1a, orch1b);  // same inside scope
            Assert.NotSame(orch1a, orch2); // different between scopes
        }
    }
}
