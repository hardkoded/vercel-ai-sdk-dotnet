---
title: "PrepareRetries"
description: "Validates maxRetries and builds the default retry strategy."
editUrl: false
prev: false
next: false
sidebar:
  label: "PrepareRetries"
  hidden: true
---

<a id="Vercel_AI_Util_PrepareRetries"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class PrepareRetries
```


Validates `maxRetries` and builds the default retry strategy.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/RetryBackoff.cs#L232)

## Methods

<a id="Vercel_AI_Util_PrepareRetries_Prepare_"></a>
<a id="Vercel_AI_Util_PrepareRetries_Prepare_System_Nullable_System_Int32__System_Threading_CancellationToken_System_String_System_Int32_"></a>

### Prepare(int?, CancellationToken, string, int)


```csharp
public static PrepareRetries.Result Prepare(int? maxRetries, CancellationToken abortSignal = default, string parameter = "maxRetries", int defaultMaxRetries = 2)
```


Validates &lt;code class="paramref"&gt;maxRetries&lt;/code&gt; and returns the default of 2 when it is null.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `maxRetries` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `abortSignal` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `parameter` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `defaultMaxRetries` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

**Returns:** [PrepareRetries.Result](/api/Vercel.AI.Util/PrepareRetries.Result/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/RetryBackoff.cs#L254)
