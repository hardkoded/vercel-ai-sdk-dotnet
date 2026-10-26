---
title: "EmbedTelemetry"
description: "Telemetry callbacks for an embedding operation."
editUrl: false
prev: false
next: false
sidebar:
  label: "EmbedTelemetry"
  hidden: true
---

<a id="Vercel_AI_Operations_EmbedTelemetry"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class EmbedTelemetry
```


Telemetry callbacks for an embedding operation.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L205)

## Properties

<a id="Vercel_AI_Operations_EmbedTelemetry_IncludeRuntimeContext_"></a>
<a id="Vercel_AI_Operations_EmbedTelemetry_IncludeRuntimeContext"></a>

### IncludeRuntimeContext


```csharp
public IReadOnlyDictionary<string, bool>? IncludeRuntimeContext { get; set; }
```


Runtime context keys included in telemetry events.

**Value:** [IReadOnlyDictionary&lt;string, bool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L223)

<a id="Vercel_AI_Operations_EmbedTelemetry_OnEmbedEnd_"></a>
<a id="Vercel_AI_Operations_EmbedTelemetry_OnEmbedEnd"></a>

### OnEmbedEnd


```csharp
public Func<EmbeddingModelCallEvent, Task>? OnEmbedEnd { get; set; }
```


Called when a `doEmbed` call ends.

**Value:** [Func&lt;EmbeddingModelCallEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L217)

<a id="Vercel_AI_Operations_EmbedTelemetry_OnEmbedStart_"></a>
<a id="Vercel_AI_Operations_EmbedTelemetry_OnEmbedStart"></a>

### OnEmbedStart


```csharp
public Func<EmbeddingModelCallEvent, Task>? OnEmbedStart { get; set; }
```


Called when a `doEmbed` call starts.

**Value:** [Func&lt;EmbeddingModelCallEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L214)

<a id="Vercel_AI_Operations_EmbedTelemetry_OnEnd_"></a>
<a id="Vercel_AI_Operations_EmbedTelemetry_OnEnd"></a>

### OnEnd


```csharp
public Func<EmbedEndEvent, Task>? OnEnd { get; set; }
```


Called when the operation ends. Receives filtered runtime context.

**Value:** [Func&lt;EmbedEndEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L211)

<a id="Vercel_AI_Operations_EmbedTelemetry_OnError_"></a>
<a id="Vercel_AI_Operations_EmbedTelemetry_OnError"></a>

### OnError


```csharp
public Func<string, Exception, Task>? OnError { get; set; }
```


Called when the operation fails.

**Value:** [Func&lt;string, Exception, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L220)

<a id="Vercel_AI_Operations_EmbedTelemetry_OnStart_"></a>
<a id="Vercel_AI_Operations_EmbedTelemetry_OnStart"></a>

### OnStart


```csharp
public Func<EmbedStartEvent, Task>? OnStart { get; set; }
```


Called when the operation starts. Receives filtered runtime context.

**Value:** [Func&lt;EmbedStartEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L208)
