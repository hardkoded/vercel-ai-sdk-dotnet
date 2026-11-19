---
title: "StreamTextOptions"
description: "Options for CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamTextOptions"
  hidden: true
---

<a id="Vercel_AI_StreamTextOptions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class StreamTextOptions : GenerateTextOptions
```


Options for `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GenerateTextOptions](/api/Vercel.AI/GenerateTextOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1831)

## Properties

<a id="Vercel_AI_StreamTextOptions_AbortSignal_"></a>
<a id="Vercel_AI_StreamTextOptions_AbortSignal"></a>

### AbortSignal


```csharp
public AbortSignal? AbortSignal { get; set; }
```


Aborts the call and carries the reason. Use it instead of the [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) when the reason matters.
A reason that is an [Exception](https://learn.microsoft.com/dotnet/api/system.exception) faults the results as is. Maps to `abortSignal`.

**Value:** [AbortSignal](/api/Vercel.AI.Util/AbortSignal/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1846)

<a id="Vercel_AI_StreamTextOptions_ExperimentalTransform_"></a>
<a id="Vercel_AI_StreamTextOptions_ExperimentalTransform"></a>

### ExperimentalTransform


```csharp
public IReadOnlyList<StreamTextTransform>? ExperimentalTransform { get; set; }
```


Optional stream transformations, applied in order. They must maintain the stream structure for `StreamTextAsync` to work correctly.
Use [Create(SmoothStreamOptions)](/api/Vercel.AI.GenerateText/SmoothStream/#Vercel_AI_GenerateText_SmoothStream_Create_Vercel_AI_GenerateText_SmoothStreamOptions_) to smooth the text output. Maps to `experimental_transform`.

**Value:** [IReadOnlyList&lt;StreamTextTransform&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1837)

<a id="Vercel_AI_StreamTextOptions_Include_"></a>
<a id="Vercel_AI_StreamTextOptions_Include"></a>

### Include


```csharp
public StreamTextInclude? Include { get; set; }
```


What to retain of the request in the [StartStepPart](/api/Vercel.AI/StartStepPart/). Maps to `include`.

**Value:** [StreamTextInclude](/api/Vercel.AI/StreamTextInclude/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1840)

<a id="Vercel_AI_StreamTextOptions_OnAbort_"></a>
<a id="Vercel_AI_StreamTextOptions_OnAbort"></a>

### OnAbort


```csharp
public Func<StreamAbortContext, CancellationToken, Task>? OnAbort { get; set; }
```


Called after the call is aborted, with the steps that finished before it. It does not delay the rejection of the
results. The stream ends with an [AbortPart](/api/Vercel.AI/AbortPart/) after it returns. [OnError](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_OnError) and
[OnEnd](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_OnEnd) are not called. Maps to `onAbort`.

**Value:** [Func&lt;StreamAbortContext, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1853)
