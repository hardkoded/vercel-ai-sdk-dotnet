---
title: "AbortErrors"
description: "Recognizes abort and timeout failures that must not be retried."
editUrl: false
prev: false
next: false
sidebar:
  label: "AbortErrors"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_AbortErrors"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class AbortErrors
```


Recognizes abort and timeout failures that must not be retried.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Delay.cs#L38)

## Methods

<a id="Vercel_AI_ProviderUtils_AbortErrors_IsAbortError_"></a>
<a id="Vercel_AI_ProviderUtils_AbortErrors_IsAbortError_System_Exception_"></a>

### IsAbortError(Exception?)


```csharp
public static bool IsAbortError(Exception? error)
```


True for [DelayAbortedException](/api/Vercel.AI.ProviderUtils/DelayAbortedException/), cancellation, [TimeoutException](https://learn.microsoft.com/dotnet/api/system.timeoutexception),
and exceptions named `AbortError`, `ResponseAborted`, or `TimeoutError`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Delay.cs#L44)
