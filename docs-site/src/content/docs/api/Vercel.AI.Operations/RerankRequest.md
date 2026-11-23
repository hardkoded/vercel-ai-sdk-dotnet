---
title: "RerankRequest"
description: "Options for CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "RerankRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_RerankRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class RerankRequest : OperationRequest
```


Options for `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OperationRequest](/api/Vercel.AI.Operations/OperationRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L507)

## Properties

<a id="Vercel_AI_Operations_RerankRequest_Documents_"></a>
<a id="Vercel_AI_Operations_RerankRequest_Documents"></a>

### Documents


```csharp
public IReadOnlyList<object?> Documents { get; set; }
```


Documents. Strings are sent as text; anything else is sent as objects.

**Value:** [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L513)

<a id="Vercel_AI_Operations_RerankRequest_ExperimentalOnEnd_"></a>
<a id="Vercel_AI_Operations_RerankRequest_ExperimentalOnEnd"></a>

### ExperimentalOnEnd


```csharp
public Func<RerankEndEvent, Task>? ExperimentalOnEnd { get; set; }
```


Deprecated alias of [OnEnd](#Vercel_AI_Operations_RerankRequest_OnEnd).

**Value:** [Func&lt;RerankEndEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L531)

<a id="Vercel_AI_Operations_RerankRequest_ExperimentalOnStart_"></a>
<a id="Vercel_AI_Operations_RerankRequest_ExperimentalOnStart"></a>

### ExperimentalOnStart


```csharp
public Func<RerankStartEvent, Task>? ExperimentalOnStart { get; set; }
```


Deprecated alias of [OnStart](#Vercel_AI_Operations_RerankRequest_OnStart).

**Value:** [Func&lt;RerankStartEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L525)

<a id="Vercel_AI_Operations_RerankRequest_ExperimentalTelemetry_"></a>
<a id="Vercel_AI_Operations_RerankRequest_ExperimentalTelemetry"></a>

### ExperimentalTelemetry


```csharp
public TelemetryOptions? ExperimentalTelemetry { get; set; }
```


Deprecated alias of [Telemetry](#Vercel_AI_Operations_RerankRequest_Telemetry).

**Value:** [TelemetryOptions](/api/Vercel.AI/TelemetryOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L537)

<a id="Vercel_AI_Operations_RerankRequest_Model_"></a>
<a id="Vercel_AI_Operations_RerankRequest_Model"></a>

### Model


```csharp
public IRerankCaller? Model { get; set; }
```


Reranking model.

**Value:** [IRerankCaller](/api/Vercel.AI.Operations/IRerankCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L510)

<a id="Vercel_AI_Operations_RerankRequest_OnEnd_"></a>
<a id="Vercel_AI_Operations_RerankRequest_OnEnd"></a>

### OnEnd


```csharp
public Func<RerankEndEvent, Task>? OnEnd { get; set; }
```


Called after the model.

**Value:** [Func&lt;RerankEndEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L528)

<a id="Vercel_AI_Operations_RerankRequest_OnStart_"></a>
<a id="Vercel_AI_Operations_RerankRequest_OnStart"></a>

### OnStart


```csharp
public Func<RerankStartEvent, Task>? OnStart { get; set; }
```


Called before the model.

**Value:** [Func&lt;RerankStartEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L522)

<a id="Vercel_AI_Operations_RerankRequest_Query_"></a>
<a id="Vercel_AI_Operations_RerankRequest_Query"></a>

### Query


```csharp
public string Query { get; set; }
```


Query.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L516)

<a id="Vercel_AI_Operations_RerankRequest_Telemetry_"></a>
<a id="Vercel_AI_Operations_RerankRequest_Telemetry"></a>

### Telemetry


```csharp
public TelemetryOptions? Telemetry { get; set; }
```


Telemetry callbacks.

**Value:** [TelemetryOptions](/api/Vercel.AI/TelemetryOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L534)

<a id="Vercel_AI_Operations_RerankRequest_TopN_"></a>
<a id="Vercel_AI_Operations_RerankRequest_TopN"></a>

### TopN


```csharp
public int? TopN { get; set; }
```


Maximum results.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L519)
