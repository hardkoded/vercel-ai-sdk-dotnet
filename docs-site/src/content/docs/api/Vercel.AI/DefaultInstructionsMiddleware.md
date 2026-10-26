---
title: "DefaultInstructionsMiddleware"
description: "Prepends system instructions when the prompt has none. Maps to defaultInstructionsMiddleware."
editUrl: false
prev: false
next: false
sidebar:
  label: "DefaultInstructionsMiddleware"
  hidden: true
---

<a id="Vercel_AI_DefaultInstructionsMiddleware"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class DefaultInstructionsMiddleware : LanguageModelMiddleware, ILanguageModelMiddleware
```


Prepends system instructions when the prompt has none. Maps to `defaultInstructionsMiddleware`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelMiddleware](/api/Vercel.AI/LanguageModelMiddleware/)

**Implements:** [ILanguageModelMiddleware](/api/Vercel.AI/ILanguageModelMiddleware/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L502)

## Constructors

<a id="Vercel_AI_DefaultInstructionsMiddleware__ctor_"></a>
<a id="Vercel_AI_DefaultInstructionsMiddleware__ctor_System_String_"></a>

### DefaultInstructionsMiddleware(string)


```csharp
public DefaultInstructionsMiddleware(string instructions)
```


Creates middleware.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `instructions` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L505)

## Properties

<a id="Vercel_AI_DefaultInstructionsMiddleware_Instructions_"></a>
<a id="Vercel_AI_DefaultInstructionsMiddleware_Instructions"></a>

### Instructions


```csharp
public string Instructions { get; }
```


Instructions to prepend.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L511)

## Methods

<a id="Vercel_AI_DefaultInstructionsMiddleware_WrapGenerateAsync_"></a>
<a id="Vercel_AI_DefaultInstructionsMiddleware_WrapGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Func_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_System_Threading_Tasks_Task_Vercel_AI_Provider_LanguageModelGenerateResult___System_Threading_CancellationToken_"></a>

### `WrapGenerateAsync(LanguageModelCallOptions, Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelGenerateResult>>, CancellationToken)`


```csharp
public override Task<LanguageModelGenerateResult> WrapGenerateAsync(LanguageModelCallOptions options, Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelGenerateResult>> next, CancellationToken cancellationToken)
```


Wraps a generate call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `next` | [Func&lt;LanguageModelCallOptions, CancellationToken, Task&lt;LanguageModelGenerateResult&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;LanguageModelGenerateResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L514)
