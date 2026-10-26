---
title: "SimulateStreamingMiddleware"
description: "Turns a non-streaming model into text deltas. Maps to simulateStreamingMiddleware."
editUrl: false
prev: false
next: false
sidebar:
  label: "SimulateStreamingMiddleware"
  hidden: true
---

<a id="Vercel_AI_SimulateStreamingMiddleware"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class SimulateStreamingMiddleware : LanguageModelMiddleware, ILanguageModelMiddleware
```


Turns a non-streaming model into text deltas. Maps to `simulateStreamingMiddleware`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelMiddleware](/api/Vercel.AI/LanguageModelMiddleware/)

**Implements:** [ILanguageModelMiddleware](/api/Vercel.AI/ILanguageModelMiddleware/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L420)

## Methods

<a id="Vercel_AI_SimulateStreamingMiddleware_Simulate_"></a>
<a id="Vercel_AI_SimulateStreamingMiddleware_Simulate_Vercel_AI_Provider_ILanguageModel_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

### Simulate(ILanguageModel, LanguageModelCallOptions, CancellationToken)


```csharp
public Task<LanguageModelStreamResult> Simulate(ILanguageModel model, LanguageModelCallOptions options, CancellationToken cancellationToken)
```


Generates once and yields the text as a single delta.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `model` | [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/) |  |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;LanguageModelStreamResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L423)
