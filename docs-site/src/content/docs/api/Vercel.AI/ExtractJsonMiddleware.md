---
title: "ExtractJsonMiddleware"
description: "Strips markdown JSON fences. Maps to extractJsonMiddleware."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExtractJsonMiddleware"
  hidden: true
---

<a id="Vercel_AI_ExtractJsonMiddleware"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ExtractJsonMiddleware : LanguageModelMiddleware, ILanguageModelMiddleware
```


Strips markdown JSON fences. Maps to `extractJsonMiddleware`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelMiddleware](/api/Vercel.AI/LanguageModelMiddleware/)

**Implements:** [ILanguageModelMiddleware](/api/Vercel.AI/ILanguageModelMiddleware/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L386)

## Constructors

<a id="Vercel_AI_ExtractJsonMiddleware__ctor_"></a>
<a id="Vercel_AI_ExtractJsonMiddleware__ctor"></a>

### ExtractJsonMiddleware()


```csharp
public ExtractJsonMiddleware()
```


Creates middleware that strips markdown code fences.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L391)

<a id="Vercel_AI_ExtractJsonMiddleware__ctor_System_Func_System_String_System_String__"></a>

### `ExtractJsonMiddleware(Func<string, string>?)`


```csharp
public ExtractJsonMiddleware(Func<string, string>? transform)
```


Creates middleware with an optional custom transform.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `transform` | [Func&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) | Custom transform applied to text content and structured output text. Replaces the default markdown fence stripping when set. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L398)

## Methods

<a id="Vercel_AI_ExtractJsonMiddleware_WrapGenerateAsync_"></a>
<a id="Vercel_AI_ExtractJsonMiddleware_WrapGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Func_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_System_Threading_Tasks_Task_Vercel_AI_Provider_LanguageModelGenerateResult___System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L404)
