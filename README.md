# OrcaFlow.Core

A lightweight, generic **task orchestration library** for .NET.  
Define tasks, chain them into pipelines, run them sequentially or in parallel, and add conditions – all with a minimal API.

---

## ✨ Features

- **Generic context** – pass any type as shared state across tasks
- **Fluent builder** – chain steps with `.AddStep<T>()` or `.AddStep(instance)`
- **Conditional execution** – skip steps dynamically with `Func<TContext, bool>`
- **Parallel execution** – group tasks and run them concurrently with `ParallelGroupTask`
- **Lifecycle hooks** – `OnStepStarted`, `OnStepCompleted`, `OnStepFailed`
- **Error handling strategies** – `StopOnError` or `SkipFailed`
- **Cancellation support** – via `CancellationToken`
- **Dependency Injection support** with `IServiceCollection`

## 🚀 Getting Started

### 1. Install

```bash
NuGet\Install-Package OrcaFlow.Core -Version 0.3.0
```

### 2. Define a task

Tasks implement the ITask<TContext> interface:

```csharp
public class AddTask : ITask<Context>
{
    public string Name => nameof(AddTask);

    public Task ExecuteAsync(Context context, CancellationToken token = default)
    {
        context.Result = context.Num1 + context.Num2;
        return Task.CompletedTask;
    }
}

public class MultiplyTask : ITask<Context>
{
    public string Name => nameof(MultiplyTask);

    public Task ExecuteAsync(Context context, CancellationToken token = default)
    {
        context.Result = context.Result * 10;
        return Task.CompletedTask;
    }
}
```

### 3. Build an orchestrator

Create a pipeline with sequential and parallel steps:

```csharp
var builder = new OrchestratorBuilder<Context>()
    .AddStep<AddTask>()
    .AddStep<MultiplyTask>()
    .Configure(options =>
    {
        options.ErrorStrategy = ErrorHandlingStrategy.StopOnError;
        options.OnStepStarted = async (task, ctx) =>
            Console.WriteLine($"Starting {task.Name}...");
        options.OnStepCompleted = async (task, ctx) =>
            Console.WriteLine($"Completed {task.Name}");
        options.OnStepFailed = async (task, ex, ctx) =>
            Console.WriteLine($"Step {task.Name} failed: {ex.Message}");
        // Middleware
        options.Use(async (task, ctx, next, token) =>
        {
            await next();
        });
    });

var orchestrator = builder.Build();
```

Or via DependencyInjection

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register services + tasks
    services.AddSingleton<MyService>();
    services.AddTransient<ServiceBackedTask>();
    services.AddTransient<OtherTask>();

    // Register orchestrator via extension
    services.AddOrchestrator<TestContext>(builder =>
    {
        builder.AddStep<ServiceBackedTask>()
               .AddStep<OtherTask>()
               .Configure(o =>
               {
                   o.OnStepStarted = async (task, ctx) =>
                       Console.WriteLine($"Starting {task.Name}...");
               });
    });
}
```

### 4. Run the pipeline

```csharp
var ctx = new Context
{
    Num1 = 1,
    Num2 = 2
};

await orchestrator.RunAsync(context);

Console.WriteLine("Result: " + context.Result);
```

Or inject the Orchestrator

```csharp
public class MyController
{
    private readonly Orchestrator<TestContext> _orchestrator;

    public MyController(Orchestrator<TestContext> orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public async Task<IActionResult> Run()
    {
        var ctx = new TestContext();
        await _orchestrator.RunAsync(ctx);
        return new OkObjectResult(ctx.Log);
    }
}
```

### Roadmap

- Retry policies (with backoff)

### Project Structure

```
/Orca        → library code
/Orca.Tests  → unit tests
/Samples     → Example projects
```
