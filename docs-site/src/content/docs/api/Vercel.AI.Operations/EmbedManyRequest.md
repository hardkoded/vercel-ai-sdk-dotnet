---
title: "EmbedManyRequest"
description: "Options for CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "EmbedManyRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_EmbedManyRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class EmbedManyRequest : OperationRequest
```


Options for `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OperationRequest](/api/Vercel.AI.Operations/OperationRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L432)

## Properties

<a id="Vercel_AI_Operations_EmbedManyRequest_Dimensions_"></a>
<a id="Vercel_AI_Operations_EmbedManyRequest_Dimensions"></a>

### Dimensions


```csharp
public int? Dimensions { get; set; }
```


Requested output dimensions. Must be a positive integer. Support depends on the model.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L441)

<a id="Vercel_AI_Operations_EmbedManyRequest_ExperimentalOnEnd_"></a>
<a id="Vercel_AI_Operations_EmbedManyRequest_ExperimentalOnEnd"></a>

### ExperimentalOnEnd


```csharp
public Func<EmbedEndEvent, Task>? ExperimentalOnEnd { get; set; }
```


Deprecated alias of [OnEnd](#Vercel_AI_Operations_EmbedManyRequest_OnEnd).

**Value:** [Func&lt;EmbedEndEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L456)

<a id="Vercel_AI_Operations_EmbedManyRequest_ExperimentalOnStart_"></a>
<a id="Vercel_AI_Operations_EmbedManyRequest_ExperimentalOnStart"></a>

### ExperimentalOnStart


```csharp
public Func<EmbedStartEvent, Task>? ExperimentalOnStart { get; set; }
```


Deprecated alias of [OnStart](#Vercel_AI_Operations_EmbedManyRequest_OnStart).

**Value:** [Func&lt;EmbedStartEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L450)

<a id="Vercel_AI_Operations_EmbedManyRequest_ExperimentalTelemetry_"></a>
<a id="Vercel_AI_Operations_EmbedManyRequest_ExperimentalTelemetry"></a>

### ExperimentalTelemetry


```csharp
public EmbedTelemetry? ExperimentalTelemetry { get; set; }
```


Deprecated alias of [Telemetry](#Vercel_AI_Operations_EmbedManyRequest_Telemetry).

**Value:** [EmbedTelemetry](/api/Vercel.AI.Operations/EmbedTelemetry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L462)

<a id="Vercel_AI_Operations_EmbedManyRequest_MaxParallelCalls_"></a>
<a id="Vercel_AI_Operations_EmbedManyRequest_MaxParallelCalls"></a>

### MaxParallelCalls


```csharp
public int? MaxParallelCalls { get; set; }
```


Concurrent chunks when the model supports parallel calls. Null means unlimited.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L444)

<a id="Vercel_AI_Operations_EmbedManyRequest_Model_"></a>
<a id="Vercel_AI_Operations_EmbedManyRequest_Model"></a>

### Model


```csharp
public IEmbeddingCaller? Model { get; set; }
```


Embedding model.

**Value:** [IEmbeddingCaller](/api/Vercel.AI.Operations/IEmbeddingCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L435)

<a id="Vercel_AI_Operations_EmbedManyRequest_OnEnd_"></a>
<a id="Vercel_AI_Operations_EmbedManyRequest_OnEnd"></a>

### OnEnd


```csharp
public Func<EmbedEndEvent, Task>? OnEnd { get; set; }
```


Called after every chunk returns.

**Value:** [Func&lt;EmbedEndEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L453)

<a id="Vercel_AI_Operations_EmbedManyRequest_OnStart_"></a>
<a id="Vercel_AI_Operations_EmbedManyRequest_OnStart"></a>

### OnStart


```csharp
public Func<EmbedStartEvent, Task>? OnStart { get; set; }
```


Called before the model.

**Value:** [Func&lt;EmbedStartEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L447)

<a id="Vercel_AI_Operations_EmbedManyRequest_Telemetry_"></a>
<a id="Vercel_AI_Operations_EmbedManyRequest_Telemetry"></a>

### Telemetry


```csharp
public EmbedTelemetry? Telemetry { get; set; }
```


Telemetry integration callbacks.

**Value:** [EmbedTelemetry](/api/Vercel.AI.Operations/EmbedTelemetry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L459)

<a id="Vercel_AI_Operations_EmbedManyRequest_Values_"></a>
<a id="Vercel_AI_Operations_EmbedManyRequest_Values"></a>

### Values


```csharp
public IReadOnlyList<string> Values { get; set; }
```


Values to embed.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L438)
