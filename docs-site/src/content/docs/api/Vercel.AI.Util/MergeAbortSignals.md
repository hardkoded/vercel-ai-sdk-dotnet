---
title: "MergeAbortSignals"
description: "Combines abort signals and timeout durations."
editUrl: false
prev: false
next: false
sidebar:
  label: "MergeAbortSignals"
  hidden: true
---

<a id="Vercel_AI_Util_MergeAbortSignals"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class MergeAbortSignals
```


Combines abort signals and timeout durations.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AbortSignals.cs#L157)

## Methods

<a id="Vercel_AI_Util_MergeAbortSignals_Merge_"></a>
<a id="Vercel_AI_Util_MergeAbortSignals_Merge_System_Object___"></a>

### Merge(params object?[]?)


```csharp
public static AbortSignal? Merge(params object?[]? signals)
```


Returns a signal that aborts when any source aborts.
Numbers are timeout durations in milliseconds. Null entries are ignored.
A single signal is returned as-is. No sources returns `null`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `signals` | [object\[\]](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [AbortSignal](/api/Vercel.AI.Util/AbortSignal/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/AbortSignals.cs#L164)
