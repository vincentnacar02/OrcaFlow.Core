# Orca

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

## 🚀 Getting Started

### 1. Install

(For now, clone or add as a project reference – NuGet packaging coming soon.)

```bash
dotnet add reference ../Orca/Orca.csproj

```

### 2. Define a task

Tasks implement the ITask<TContext> interface:

```csharp
public sealed class AppendTask : ITask<List<string>>
{
    public string Name { get; }
    private readonly string _msg;

    public AppendTask(string name, string msg)
    {
        Name = name;
        _msg = msg;
    }

    public Task ExecuteAsync(List<string> context, CancellationToken token = default)
    {
        context.Add(_msg);
        return Task.CompletedTask;
    }
}
```

### 3. Build an orchestrator

Create a pipeline with sequential and parallel steps:

```csharp
var builder = new OrchestratorBuilder<List<string>>()
    .AddStep(new AppendTask("A", "one"))
    .AddStep(new AppendTask("B", "two"))
    .AddStep<AppendTask>(ctx => ctx.Count < 5) // conditional with generic new()
    .AddStep(
        new ParallelGroupTask<List<string>>(
            "ParallelGroup",
            new ITask<List<string>>[]
            {
                new AppendTask("P1", "parallel-1"),
                new AppendTask("P2", "parallel-2")
            }))
    .Configure(options =>
    {
        options.ErrorStrategy = ErrorHandlingStrategy.ContinueOnError;
        options.OnStepStarted = async (task, ctx) =>
            Console.WriteLine($"Starting {task.Name}...");
        options.OnStepCompleted = async (task, ctx) =>
            Console.WriteLine($"Completed {task.Name}");
        options.OnStepFailed = async (task, ex, ctx) =>
            Console.WriteLine($"Step {task.Name} failed: {ex.Message}");
    });

var orchestrator = builder.Build();
```

### 4. Run the pipeline

```csharp
var context = new List<string>();

await orchestrator.RunAsync(context);

Console.WriteLine("Items: " + string.Join(", ", context));
```

Example Output:

```
Starting A...
Completed A
Starting B...
Completed B
Starting AppendTask...
Completed AppendTask
Starting ParallelGroup...
Completed ParallelGroup
Items: one, two, conditional, parallel-1, parallel-2
```

### Roadmap

- Retry policies (with backoff)
- Structured logging integration
- NuGet packaging

### Project Structure

```
/Orca          → library code
/Orca.Tests  → unit tests
```
