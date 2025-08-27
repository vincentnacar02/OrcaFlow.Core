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
