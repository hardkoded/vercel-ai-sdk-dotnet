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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2237)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2256)

<a id="Vercel_AI_StreamTextOptions_ExperimentalInclude_"></a>
<a id="Vercel_AI_StreamTextOptions_ExperimentalInclude"></a>

### ExperimentalInclude


```csharp
[Obsolete("Use Include instead.")]
public StreamTextInclude? ExperimentalInclude { get; set; }
```


Deprecated alias of [Include](#Vercel_AI_StreamTextOptions_Include). Hides [ExperimentalInclude](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ExperimentalInclude). Maps to `experimental_include`.

**Value:** [StreamTextInclude](/api/Vercel.AI/StreamTextInclude/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2249)

<a id="Vercel_AI_StreamTextOptions_ExperimentalTransform_"></a>
<a id="Vercel_AI_StreamTextOptions_ExperimentalTransform"></a>

### ExperimentalTransform


```csharp
public IReadOnlyList<StreamTextTransform>? ExperimentalTransform { get; set; }
```


Optional stream transformations, applied in order. They must maintain the stream structure for `StreamTextAsync` to work correctly.
Use [Create(SmoothStreamOptions)](/api/Vercel.AI.GenerateText/SmoothStream/#Vercel_AI_GenerateText_SmoothStream_Create_Vercel_AI_GenerateText_SmoothStreamOptions_) to smooth the text output. Maps to `experimental_transform`.

**Value:** [IReadOnlyList&lt;StreamTextTransform&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2243)

<a id="Vercel_AI_StreamTextOptions_Include_"></a>
<a id="Vercel_AI_StreamTextOptions_Include"></a>

### Include


```csharp
public StreamTextInclude? Include { get; set; }
```


What to retain of the request in the [StartStepPart](/api/Vercel.AI/StartStepPart/) and whether to forward raw chunks. Hides [Include](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_Include), which has the type of `generateText`. Maps to `include`.

**Value:** [StreamTextInclude](/api/Vercel.AI/StreamTextInclude/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2246)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2263)

<a id="Vercel_AI_StreamTextOptions_OnChunk_"></a>
<a id="Vercel_AI_StreamTextOptions_OnChunk"></a>

### OnChunk


```csharp
public Func<StreamTextChunkEvent, CancellationToken, Task>? OnChunk { get; set; }
```


Called for each part of the stream, after the [ExperimentalTransform](#Vercel_AI_StreamTextOptions_ExperimentalTransform) transforms. The stream processing pauses until
the callback returns. Maps to `onChunk`.

**Value:** [Func&lt;StreamTextChunkEvent, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2269)

<a id="Vercel_AI_StreamTextOptions_OnErrorRetry_"></a>
<a id="Vercel_AI_StreamTextOptions_OnErrorRetry"></a>

### OnErrorRetry


```csharp
public Func<Exception, CancellationToken, Task<StreamTextOnErrorResult?>>? OnErrorRetry { get; set; }
```


Like [OnError](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_OnError), and its result can request one retry of the current model step after a provider
error that was received from the response stream, when [StreamRetries](#Vercel_AI_StreamTextOptions_StreamRetries) is set. It takes the place of
[OnError](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_OnError). C# has no union of callbacks, so the `onError` that returns `{ retry: true }` is this property.

**Value:** [Func&lt;Exception, CancellationToken, Task&lt;StreamTextOnErrorResult&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2286)

<a id="Vercel_AI_StreamTextOptions_StreamRetries_"></a>
<a id="Vercel_AI_StreamTextOptions_StreamRetries"></a>

### StreamRetries


```csharp
public int? StreamRetries { get; set; }
```


Maximum number of automatic retries for provider errors received after response streaming has started. Each retry reruns only
the current model step. Completed earlier steps and their tool results are preserved.
Partial output from a failed attempt that was already emitted cannot be retracted and remains in consumer-facing streams. It is
excluded from the recovered step result, structured output parsing, response messages, and subsequent model steps.
Set it to `0` to disable automatic retries while allowing [OnErrorRetry](#Vercel_AI_StreamTextOptions_OnErrorRetry) to request retries. Leave it
&lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt; to disable all stream retry behavior. Maps to `streamRetries`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2279)
