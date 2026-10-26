---
title: "LanguageModelMiddleware"
description: "Passes both operations through."
editUrl: false
prev: false
next: false
sidebar:
  label: "LanguageModelMiddleware"
  hidden: true
---

<a id="Vercel_AI_LanguageModelMiddleware"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public abstract class LanguageModelMiddleware : ILanguageModelMiddleware
```


Passes both operations through.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModelMiddleware](/api/Vercel.AI/ILanguageModelMiddleware/)

**Derived:** [DefaultInstructionsMiddleware](/api/Vercel.AI/DefaultInstructionsMiddleware/), [DefaultSettingsMiddleware](/api/Vercel.AI/DefaultSettingsMiddleware/), [ExtractJsonMiddleware](/api/Vercel.AI/ExtractJsonMiddleware/), [ExtractReasoningMiddleware](/api/Vercel.AI/ExtractReasoningMiddleware/), [SimulateStreamingMiddleware](/api/Vercel.AI/SimulateStreamingMiddleware/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L30)

## Methods

<a id="Vercel_AI_LanguageModelMiddleware_WrapGenerateAsync_"></a>
<a id="Vercel_AI_LanguageModelMiddleware_WrapGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Func_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_System_Threading_Tasks_Task_Vercel_AI_Provider_LanguageModelGenerateResult___System_Threading_CancellationToken_"></a>

### `WrapGenerateAsync(LanguageModelCallOptions, Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelGenerateResult>>, CancellationToken)`


```csharp
public virtual Task<LanguageModelGenerateResult> WrapGenerateAsync(LanguageModelCallOptions options, Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelGenerateResult>> next, CancellationToken cancellationToken)
```


Wraps a generate call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `next` | [Func&lt;LanguageModelCallOptions, CancellationToken, Task&lt;LanguageModelGenerateResult&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;LanguageModelGenerateResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L33)

<a id="Vercel_AI_LanguageModelMiddleware_WrapStreamAsync_"></a>
<a id="Vercel_AI_LanguageModelMiddleware_WrapStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Func_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_System_Threading_Tasks_Task_Vercel_AI_Provider_LanguageModelStreamResult___System_Threading_CancellationToken_"></a>

### `WrapStreamAsync(LanguageModelCallOptions, Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelStreamResult>>, CancellationToken)`


```csharp
public virtual Task<LanguageModelStreamResult> WrapStreamAsync(LanguageModelCallOptions options, Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelStreamResult>> next, CancellationToken cancellationToken)
```


Wraps a stream call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `next` | [Func&lt;LanguageModelCallOptions, CancellationToken, Task&lt;LanguageModelStreamResult&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;LanguageModelStreamResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L42)
