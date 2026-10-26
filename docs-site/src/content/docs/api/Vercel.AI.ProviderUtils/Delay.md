---
title: "Delay"
description: "Waits for a duration, or completes immediately when the duration is null. Negative durations wait zero."
editUrl: false
prev: false
next: false
sidebar:
  label: "Delay"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_Delay"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class Delay
```


Waits for a duration, or completes immediately when the duration is null. Negative durations wait zero.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Delay.cs#L62)

## Methods

<a id="Vercel_AI_ProviderUtils_Delay_WaitAsync_"></a>
<a id="Vercel_AI_ProviderUtils_Delay_WaitAsync_System_Nullable_System_Int32__System_Threading_CancellationToken_"></a>

### WaitAsync(int?, CancellationToken)


```csharp
public static Task WaitAsync(int? milliseconds, CancellationToken cancellationToken = default)
```


Waits &lt;code class="paramref"&gt;milliseconds&lt;/code&gt;, or completes immediately when it is null.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `milliseconds` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Delay.cs#L72)

<a id="Vercel_AI_ProviderUtils_Delay_WaitAsync_System_Threading_CancellationToken_"></a>

### WaitAsync(CancellationToken)


```csharp
public static Task WaitAsync(CancellationToken cancellationToken = default)
```


Completes immediately. This is the null and undefined duration.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Delay.cs#L65)
