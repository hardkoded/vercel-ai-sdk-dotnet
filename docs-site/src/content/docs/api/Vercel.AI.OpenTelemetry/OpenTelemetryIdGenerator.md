---
title: "OpenTelemetryIdGenerator"
description: "Instance-scoped SDK bridge for durable tracing. Pass the same instance as OpenTelemetryOptions.Durable. Without a custom sampler, uses parent-based sampling..."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenTelemetryIdGenerator"
  hidden: true
---

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGenerator"></a>

Namespace: [Vercel.AI.OpenTelemetry](/api/Vercel.AI.OpenTelemetry/)


```csharp
public sealed class OpenTelemetryIdGenerator
```


Instance-scoped SDK bridge for durable tracing. Pass the same instance as `OpenTelemetryOptions.Durable`. Without a custom
sampler, uses parent-based sampling with roots enabled. Maps to `OpenTelemetryIdGenerator` in `durable-span-ids.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

.NET has no id-generator hook, and [SpanId](https://learn.microsoft.com/dotnet/api/system.diagnostics.activity.spanid) cannot be set. The requested trace id, sampling decision, and trace
state reach a span through its parent [ActivityContext](https://learn.microsoft.com/dotnet/api/system.diagnostics.activitycontext). The requested span id does not: a span keeps a random id, so the
root and step spans of a run do not link by their derived ids across processes.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L69)

## Constructors

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGenerator__ctor_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGenerator__ctor_Vercel_AI_OpenTelemetry_OpenTelemetryIdGeneratorOptions_"></a>

### OpenTelemetryIdGenerator(OpenTelemetryIdGeneratorOptions?)


```csharp
public OpenTelemetryIdGenerator(OpenTelemetryIdGeneratorOptions? options = null)
```


Creates the bridge.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenTelemetryIdGeneratorOptions](/api/Vercel.AI.OpenTelemetry/OpenTelemetryIdGeneratorOptions/) | Options. Null uses the defaults. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L77)

## Methods

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGenerator_CaptureContext_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGenerator_CaptureContext_System_String_System_String_System_Diagnostics_ActivityKind_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Object__"></a>

### `CaptureContext(string, string, ActivityKind, IReadOnlyDictionary<string, object?>)`


```csharp
public DurableTelemetryContext CaptureContext(string callId, string name, ActivityKind kind, IReadOnlyDictionary<string, object?> attributes)
```


Captures the tracing identity of a durable run. Maps to `captureContext`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `kind` | [ActivityKind](https://learn.microsoft.com/dotnet/api/system.diagnostics.activitykind) |  |
| `attributes` | [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [DurableTelemetryContext](/api/Vercel.AI/DurableTelemetryContext/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L122)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGenerator_DeriveSpanId_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGenerator_DeriveSpanId_System_String_System_Object___"></a>

### DeriveSpanId(string, params object[])


```csharp
public static string DeriveSpanId(string callId, params object[] path)
```


Derives a span ID of a durable run from its telemetry call ID. Maps to `deriveSpanId`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `path` | [object\[\]](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L220)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGenerator_DeriveTraceId_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGenerator_DeriveTraceId_System_String_"></a>

### DeriveTraceId(string)


```csharp
public static string DeriveTraceId(string callId)
```


Derives the trace ID of a durable run from its telemetry call ID. Maps to `deriveTraceId`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L217)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGenerator_GenerateSpanId_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGenerator_GenerateSpanId"></a>

### GenerateSpanId()


```csharp
public string GenerateSpanId()
```


Maps to `generateSpanId`.

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L84)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGenerator_GenerateTraceId_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGenerator_GenerateTraceId"></a>

### GenerateTraceId()


```csharp
public string GenerateTraceId()
```


Maps to `generateTraceId`.

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L81)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGenerator_ShouldSample_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGenerator_ShouldSample_System_Diagnostics_ActivityContext_System_String_System_String_System_Diagnostics_ActivityKind_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Object__"></a>

### `ShouldSample(ActivityContext, string, string, ActivityKind, IReadOnlyDictionary<string, object?>)`


```csharp
public DurableSamplingResult ShouldSample(ActivityContext parentContext, string traceId, string spanName, ActivityKind spanKind, IReadOnlyDictionary<string, object?> attributes)
```


Maps to `shouldSample`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `parentContext` | [ActivityContext](https://learn.microsoft.com/dotnet/api/system.diagnostics.activitycontext) |  |
| `traceId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `spanName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `spanKind` | [ActivityKind](https://learn.microsoft.com/dotnet/api/system.diagnostics.activitykind) |  |
| `attributes` | [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [DurableSamplingResult](/api/Vercel.AI.OpenTelemetry/DurableSamplingResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L87)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGenerator_ToString_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGenerator_ToString"></a>

### ToString()


```csharp
public override string ToString()
```


Maps to `toString`.

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L119)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGenerator_WithIds_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryIdGenerator_WithIds__1_Vercel_AI_OpenTelemetry_DurableRequestedIds_System_Func___0__"></a>

### `WithIds<T>(DurableRequestedIds, Func<T>)`


```csharp
public T WithIds<T>(DurableRequestedIds ids, Func<T> startSpan)
```


Scopes requested ids to this bridge and restores nested requests. Maps to `withIds`.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `ids` | [DurableRequestedIds](/api/Vercel.AI.OpenTelemetry/DurableRequestedIds/) |  |
| `startSpan` | [Func&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

**Returns:** `T`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/DurableSpanIds.cs#L168)
