---
title: "ExtractReasoningMiddleware"
description: "Splits delimited reasoning, such as <think>...</think>, out of text. Maps to extractReasoningMiddleware."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExtractReasoningMiddleware"
  hidden: true
---

<a id="Vercel_AI_ExtractReasoningMiddleware"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ExtractReasoningMiddleware : LanguageModelMiddleware, ILanguageModelMiddleware
```


Splits delimited reasoning, such as `<think>...</think>`, out of text. Maps to `extractReasoningMiddleware`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelMiddleware](/api/Vercel.AI/LanguageModelMiddleware/)

**Implements:** [ILanguageModelMiddleware](/api/Vercel.AI/ILanguageModelMiddleware/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L76)

## Constructors

<a id="Vercel_AI_ExtractReasoningMiddleware__ctor_"></a>
<a id="Vercel_AI_ExtractReasoningMiddleware__ctor_System_String_System_String_System_Boolean_"></a>

### ExtractReasoningMiddleware(string, string, bool)


```csharp
public ExtractReasoningMiddleware(string tagName = "think", string separator = "\n", bool startWithReasoning = false)
```


Creates middleware for the XML tags `<tagName>` and `</tagName>`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tagName` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Tag name without angle brackets. |
| `separator` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Joins reasoning blocks, and the text around them. |
| `startWithReasoning` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | Prepends the opening tag when the model omits it. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L88)

<a id="Vercel_AI_ExtractReasoningMiddleware__ctor_System_ValueTuple_System_String_System_String__System_String_System_Boolean_"></a>

### ExtractReasoningMiddleware((string Opening, string Closing), string, bool)


```csharp
public ExtractReasoningMiddleware((string Opening, string Closing) tagName, string separator = "\n", bool startWithReasoning = false)
```


Creates middleware for literal delimiters, such as Gemma's thought channel.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tagName` | [(string Opening, string Closing)](https://learn.microsoft.com/dotnet/api/system.string) | Literal opening and closing delimiters. |
| `separator` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Joins reasoning blocks, and the text around them. |
| `startWithReasoning` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | Prepends the opening delimiter when the model omits it. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L97)

## Methods

<a id="Vercel_AI_ExtractReasoningMiddleware_WrapGenerateAsync_"></a>
<a id="Vercel_AI_ExtractReasoningMiddleware_WrapGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Func_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_System_Threading_Tasks_Task_Vercel_AI_Provider_LanguageModelGenerateResult___System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L128)

<a id="Vercel_AI_ExtractReasoningMiddleware_WrapStreamAsync_"></a>
<a id="Vercel_AI_ExtractReasoningMiddleware_WrapStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Func_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_System_Threading_Tasks_Task_Vercel_AI_Provider_LanguageModelStreamResult___System_Threading_CancellationToken_"></a>

### `WrapStreamAsync(LanguageModelCallOptions, Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelStreamResult>>, CancellationToken)`


```csharp
public override Task<LanguageModelStreamResult> WrapStreamAsync(LanguageModelCallOptions options, Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelStreamResult>> next, CancellationToken cancellationToken)
```


Wraps a stream call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `next` | [Func&lt;LanguageModelCallOptions, CancellationToken, Task&lt;LanguageModelStreamResult&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;LanguageModelStreamResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L182)
