# Orca

A lightweight, generic **task orchestration library** for .NET.  
Define tasks, chain them into pipelines, run them sequentially or in parallel, and add conditions – all with a minimal API.

---

## ✨ Features

- **Generic context** – pass in any type as shared state across tasks
- **Composable steps** – add tasks directly, via factories, or using `new()`
- **Conditional execution** – run tasks only if a condition is met
- **Parallel execution** – group tasks and run them concurrently with `ParallelGroupTask`
- **Error handling strategies** – choose `StopOnError` or `SkipFailed`

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
var builder = new Orchestrator<List<string>>.Builder()
    .AddStep(new AppendTask("A", "one"))
    .AddStep(new AppendTask("B", "two"))
    .AddStepFactory(ctx => new AppendTask("C", $"count={ctx.Count}"))
    .AddStep(new ParallelGroupTask<List<string>>(
        "ParallelGroup",
        new ITask<List<string>>[]
        {
            new AppendTask("P1", "parallel-1"),
            new AppendTask("P2", "parallel-2")
        }));

var orchestrator = builder.Build();
```

### 4. Run the pipeline

```csharp
var context = new List<string>();

var result = await orchestrator.RunAsync(context);

Console.WriteLine("Succeeded: " + result.Succeeded);
Console.WriteLine("Items: " + string.Join(", ", context));

if (!result.Succeeded)
{
    foreach (var (step, error) in result.Errors)
        Console.WriteLine($"Step {step} failed: {error.Message}");
}
```

Example Output:

```
Succeeded: True
Items: one, two, count=2, parallel-1, parallel-2
```

### Project Structure

```
/Orca          → library code
/Orca.Tests  → unit tests
```
