---
title: "OpenTelemetryOptions"
description: "Options of OpenTelemetry. Maps to OpenTelemetryOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenTelemetryOptions"
  hidden: true
---

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions"></a>

Namespace: [Vercel.AI.OpenTelemetry](/api/Vercel.AI.OpenTelemetry/)


```csharp
public sealed class OpenTelemetryOptions
```


Options of [OpenTelemetry](/api/Vercel.AI.OpenTelemetry/OpenTelemetry/). Maps to `OpenTelemetryOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L95)

## Properties

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_AgentSpanName_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_AgentSpanName"></a>

### AgentSpanName


```csharp
public AgentSpanName? AgentSpanName { get; set; }
```


Controls the root agent span names for text generation. Null is [ModelId](/api/Vercel.AI.OpenTelemetry/AgentSpanName/#Vercel_AI_OpenTelemetry_AgentSpanName_ModelId).

**Value:** [AgentSpanName](/api/Vercel.AI.OpenTelemetry/AgentSpanName/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L110)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_Durable_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_Durable"></a>

### Durable


```csharp
public OpenTelemetryIdGenerator? Durable { get; set; }
```


The bridge that lets spans of a durable run share the run's trace. Pass the same instance as the durable option of the integration.
Maps to `durable`.

**Value:** [OpenTelemetryIdGenerator](/api/Vercel.AI.OpenTelemetry/OpenTelemetryIdGenerator/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L107)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_Embedding_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_Embedding"></a>

### Embedding


```csharp
public bool? Embedding { get; set; }
```


Emit embedding input and output values.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L122)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_EnrichSpan_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_EnrichSpan"></a>

### EnrichSpan


```csharp
public EnrichSpan? EnrichSpan { get; set; }
```


Adds custom attributes to spans when they are created.

**Value:** [EnrichSpan](/api/Vercel.AI.OpenTelemetry/EnrichSpan/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L113)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_ExperimentalDecision_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_ExperimentalDecision"></a>

### ExperimentalDecision


```csharp
public bool? ExperimentalDecision { get; set; }
```


Emit decision state, questions, and answers. Maps to `experimental_decision`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L128)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_ExperimentalEvaluation_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_ExperimentalEvaluation"></a>

### ExperimentalEvaluation


```csharp
[Obsolete("Use ExperimentalDecision instead.")]
public bool? ExperimentalEvaluation { get; set; }
```


Deprecated alias of [ExperimentalDecision](#Vercel_AI_OpenTelemetry_OpenTelemetryOptions_ExperimentalDecision). Maps to `experimental_evaluation`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L131)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_ExperimentalExtraction_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_ExperimentalExtraction"></a>

### ExperimentalExtraction


```csharp
public bool? ExperimentalExtraction { get; set; }
```


Emit extraction state, fields, reference date, and answers. Maps to `experimental_extraction`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L135)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_Headers_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_Headers"></a>

### Headers


```csharp
public bool? Headers { get; set; }
```


Emit request headers.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L141)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_ProviderMetadata_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public bool? ProviderMetadata { get; set; }
```


Emit provider metadata on response spans.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L119)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_Reranking_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_Reranking"></a>

### Reranking


```csharp
public bool? Reranking { get; set; }
```


Emit reranking input documents and output ranking.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L125)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_RuntimeContext_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_RuntimeContext"></a>

### RuntimeContext


```csharp
public bool? RuntimeContext { get; set; }
```


Emit runtime context values.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L138)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_Schema_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_Schema"></a>

### Schema


```csharp
public bool? Schema { get; set; }
```


Emit object generation schema information. Maps to `schema`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L147)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_ToolChoice_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_ToolChoice"></a>

### ToolChoice


```csharp
public bool? ToolChoice { get; set; }
```


Emit the tool choice of a step as `ai.prompt.toolChoice`. Maps to `toolChoice`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L144)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_Tracer_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_Tracer"></a>

### Tracer


```csharp
public ActivitySource? Tracer { get; set; }
```


The source that creates the spans. Maps to `tracer`. The default is a source named `gen_ai`.
Add that name to your `TracerProvider`.

**Value:** [ActivitySource](https://learn.microsoft.com/dotnet/api/system.diagnostics.activitysource)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L101)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_Usage_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryOptions_Usage"></a>

### Usage


```csharp
public bool? Usage { get; set; }
```


Emit AI SDK usage details that GenAI SemConv does not represent.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L116)
