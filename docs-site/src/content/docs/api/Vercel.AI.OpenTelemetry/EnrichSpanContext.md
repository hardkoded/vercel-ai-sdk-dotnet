---
title: "EnrichSpanContext"
description: "The call an EnrichSpan callback is asked about. Maps to the callback options in EnrichSpan."
editUrl: false
prev: false
next: false
sidebar:
  label: "EnrichSpanContext"
  hidden: true
---

<a id="Vercel_AI_OpenTelemetry_EnrichSpanContext"></a>

Namespace: [Vercel.AI.OpenTelemetry](/api/Vercel.AI.OpenTelemetry/)


```csharp
public sealed class EnrichSpanContext
```


The call an [EnrichSpan](/api/Vercel.AI.OpenTelemetry/EnrichSpan/) callback is asked about. Maps to the callback options in `EnrichSpan`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L38)

## Properties

<a id="Vercel_AI_OpenTelemetry_EnrichSpanContext_CallId_"></a>
<a id="Vercel_AI_OpenTelemetry_EnrichSpanContext_CallId"></a>

### CallId


```csharp
public string CallId { get; init; }
```


The call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L47)

<a id="Vercel_AI_OpenTelemetry_EnrichSpanContext_OperationId_"></a>
<a id="Vercel_AI_OpenTelemetry_EnrichSpanContext_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; init; }
```


The operation id, such as `ai.generateText`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L44)

<a id="Vercel_AI_OpenTelemetry_EnrichSpanContext_RuntimeContext_"></a>
<a id="Vercel_AI_OpenTelemetry_EnrichSpanContext_RuntimeContext"></a>

### RuntimeContext


```csharp
public IReadOnlyDictionary<string, object?>? RuntimeContext { get; init; }
```


The runtime context of the call, or null.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L50)

<a id="Vercel_AI_OpenTelemetry_EnrichSpanContext_SpanType_"></a>
<a id="Vercel_AI_OpenTelemetry_EnrichSpanContext_SpanType"></a>

### SpanType


```csharp
public OpenTelemetrySpanType SpanType { get; init; }
```


The kind of span being created.

**Value:** [OpenTelemetrySpanType](/api/Vercel.AI.OpenTelemetry/OpenTelemetrySpanType/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L41)
