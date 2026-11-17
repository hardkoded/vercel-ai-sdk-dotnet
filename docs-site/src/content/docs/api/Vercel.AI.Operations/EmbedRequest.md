---
title: "EmbedRequest"
description: "Options for CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "EmbedRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_EmbedRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class EmbedRequest : OperationRequest
```


Options for `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OperationRequest](/api/Vercel.AI.Operations/OperationRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L401)

## Properties

<a id="Vercel_AI_Operations_EmbedRequest_Dimensions_"></a>
<a id="Vercel_AI_Operations_EmbedRequest_Dimensions"></a>

### Dimensions


```csharp
public int? Dimensions { get; set; }
```


Requested output dimensions. Must be a positive integer. Support depends on the model.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L410)

<a id="Vercel_AI_Operations_EmbedRequest_ExperimentalOnEnd_"></a>
<a id="Vercel_AI_Operations_EmbedRequest_ExperimentalOnEnd"></a>

### ExperimentalOnEnd


```csharp
public Func<EmbedEndEvent, Task>? ExperimentalOnEnd { get; set; }
```


Deprecated alias of [OnEnd](#Vercel_AI_Operations_EmbedRequest_OnEnd).

**Value:** [Func&lt;EmbedEndEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L422)

<a id="Vercel_AI_Operations_EmbedRequest_ExperimentalOnStart_"></a>
<a id="Vercel_AI_Operations_EmbedRequest_ExperimentalOnStart"></a>

### ExperimentalOnStart


```csharp
public Func<EmbedStartEvent, Task>? ExperimentalOnStart { get; set; }
```


Deprecated alias of [OnStart](#Vercel_AI_Operations_EmbedRequest_OnStart).

**Value:** [Func&lt;EmbedStartEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L416)

<a id="Vercel_AI_Operations_EmbedRequest_ExperimentalTelemetry_"></a>
<a id="Vercel_AI_Operations_EmbedRequest_ExperimentalTelemetry"></a>

### ExperimentalTelemetry


```csharp
public EmbedTelemetry? ExperimentalTelemetry { get; set; }
```


Deprecated alias of [Telemetry](#Vercel_AI_Operations_EmbedRequest_Telemetry).

**Value:** [EmbedTelemetry](/api/Vercel.AI.Operations/EmbedTelemetry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L428)

<a id="Vercel_AI_Operations_EmbedRequest_Model_"></a>
<a id="Vercel_AI_Operations_EmbedRequest_Model"></a>

### Model


```csharp
public IEmbeddingCaller? Model { get; set; }
```


Embedding model.

**Value:** [IEmbeddingCaller](/api/Vercel.AI.Operations/IEmbeddingCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L404)

<a id="Vercel_AI_Operations_EmbedRequest_OnEnd_"></a>
<a id="Vercel_AI_Operations_EmbedRequest_OnEnd"></a>

### OnEnd


```csharp
public Func<EmbedEndEvent, Task>? OnEnd { get; set; }
```


Called after the model.

**Value:** [Func&lt;EmbedEndEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L419)

<a id="Vercel_AI_Operations_EmbedRequest_OnStart_"></a>
<a id="Vercel_AI_Operations_EmbedRequest_OnStart"></a>

### OnStart


```csharp
public Func<EmbedStartEvent, Task>? OnStart { get; set; }
```


Called before the model.

**Value:** [Func&lt;EmbedStartEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L413)

<a id="Vercel_AI_Operations_EmbedRequest_Telemetry_"></a>
<a id="Vercel_AI_Operations_EmbedRequest_Telemetry"></a>

### Telemetry


```csharp
public EmbedTelemetry? Telemetry { get; set; }
```


Telemetry integration callbacks.

**Value:** [EmbedTelemetry](/api/Vercel.AI.Operations/EmbedTelemetry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L425)

<a id="Vercel_AI_Operations_EmbedRequest_Value_"></a>
<a id="Vercel_AI_Operations_EmbedRequest_Value"></a>

### Value


```csharp
public string Value { get; set; }
```


Value to embed.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L407)
