---
title: "DefaultSettingsMiddleware"
description: "Fills sampling settings when the caller left them unset. Maps to defaultSettingsMiddleware."
editUrl: false
prev: false
next: false
sidebar:
  label: "DefaultSettingsMiddleware"
  hidden: true
---

<a id="Vercel_AI_DefaultSettingsMiddleware"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class DefaultSettingsMiddleware : LanguageModelMiddleware, ILanguageModelMiddleware
```


Fills sampling settings when the caller left them unset. Maps to `defaultSettingsMiddleware`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelMiddleware](/api/Vercel.AI/LanguageModelMiddleware/)

**Implements:** [ILanguageModelMiddleware](/api/Vercel.AI/ILanguageModelMiddleware/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L459)

## Constructors

<a id="Vercel_AI_DefaultSettingsMiddleware__ctor_"></a>
<a id="Vercel_AI_DefaultSettingsMiddleware__ctor_System_Nullable_System_Double__System_Nullable_System_Int32__"></a>

### DefaultSettingsMiddleware(double?, int?)


```csharp
public DefaultSettingsMiddleware(double? temperature = null, int? maxOutputTokens = null)
```


Creates middleware with default sampling settings.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `temperature` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `maxOutputTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L462)

## Properties

<a id="Vercel_AI_DefaultSettingsMiddleware_MaxOutputTokens_"></a>
<a id="Vercel_AI_DefaultSettingsMiddleware_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public int? MaxOutputTokens { get; }
```


Default max output tokens.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L472)

<a id="Vercel_AI_DefaultSettingsMiddleware_Temperature_"></a>
<a id="Vercel_AI_DefaultSettingsMiddleware_Temperature"></a>

### Temperature


```csharp
public double? Temperature { get; }
```


Default temperature.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L469)

## Methods

<a id="Vercel_AI_DefaultSettingsMiddleware_WrapGenerateAsync_"></a>
<a id="Vercel_AI_DefaultSettingsMiddleware_WrapGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Func_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_System_Threading_Tasks_Task_Vercel_AI_Provider_LanguageModelGenerateResult___System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L475)

<a id="Vercel_AI_DefaultSettingsMiddleware_WrapStreamAsync_"></a>
<a id="Vercel_AI_DefaultSettingsMiddleware_WrapStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Func_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_System_Threading_Tasks_Task_Vercel_AI_Provider_LanguageModelStreamResult___System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L485)
