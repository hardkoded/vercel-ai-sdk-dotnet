---
title: "ExtractTelemetry"
description: "Telemetry callbacks for CancellationToken). Generic operation callbacks on an integration are not called."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExtractTelemetry"
  hidden: true
---

<a id="Vercel_AI_Operations_ExtractTelemetry"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ExtractTelemetry
```


Telemetry callbacks for `CancellationToken)`. Generic operation callbacks on an integration are not called.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L98)

## Properties

<a id="Vercel_AI_Operations_ExtractTelemetry_FunctionId_"></a>
<a id="Vercel_AI_Operations_ExtractTelemetry_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; set; }
```


Copied onto telemetry events.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L125)

<a id="Vercel_AI_Operations_ExtractTelemetry_IncludeRuntimeContext_"></a>
<a id="Vercel_AI_Operations_ExtractTelemetry_IncludeRuntimeContext"></a>

### IncludeRuntimeContext


```csharp
public IReadOnlyDictionary<string, bool>? IncludeRuntimeContext { get; set; }
```


Keys copied into telemetry runtime context. Null yields an empty context.

**Value:** [IReadOnlyDictionary&lt;string, bool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L116)

<a id="Vercel_AI_Operations_ExtractTelemetry_IsEnabled_"></a>
<a id="Vercel_AI_Operations_ExtractTelemetry_IsEnabled"></a>

### IsEnabled


```csharp
public bool IsEnabled { get; set; }
```


When false, telemetry callbacks are skipped.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L128)

<a id="Vercel_AI_Operations_ExtractTelemetry_OnError_"></a>
<a id="Vercel_AI_Operations_ExtractTelemetry_OnError"></a>

### OnError


```csharp
public Func<string, Exception, Task>? OnError { get; set; }
```


`onError`. A download error arrives without its URL.

**Value:** [Func&lt;string, Exception, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L113)

<a id="Vercel_AI_Operations_ExtractTelemetry_OnExtractEnd_"></a>
<a id="Vercel_AI_Operations_ExtractTelemetry_OnExtractEnd"></a>

### OnExtractEnd


```csharp
public Func<ExtractEndEvent, Task>? OnExtractEnd { get; set; }
```


`experimental_onExtractEnd`.

**Value:** [Func&lt;ExtractEndEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L110)

<a id="Vercel_AI_Operations_ExtractTelemetry_OnExtractStart_"></a>
<a id="Vercel_AI_Operations_ExtractTelemetry_OnExtractStart"></a>

### OnExtractStart


```csharp
public Func<ExtractStartEvent, Task>? OnExtractStart { get; set; }
```


`experimental_onExtractStart`.

**Value:** [Func&lt;ExtractStartEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L101)

<a id="Vercel_AI_Operations_ExtractTelemetry_OnExtractionModelCallEnd_"></a>
<a id="Vercel_AI_Operations_ExtractTelemetry_OnExtractionModelCallEnd"></a>

### OnExtractionModelCallEnd


```csharp
public Func<ExtractionModelCallEndEvent, Task>? OnExtractionModelCallEnd { get; set; }
```


`experimental_onExtractionModelCallEnd`.

**Value:** [Func&lt;ExtractionModelCallEndEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L107)

<a id="Vercel_AI_Operations_ExtractTelemetry_OnExtractionModelCallStart_"></a>
<a id="Vercel_AI_Operations_ExtractTelemetry_OnExtractionModelCallStart"></a>

### OnExtractionModelCallStart


```csharp
public Func<ExtractionModelCallStartEvent, Task>? OnExtractionModelCallStart { get; set; }
```


`experimental_onExtractionModelCallStart`.

**Value:** [Func&lt;ExtractionModelCallStartEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L104)

<a id="Vercel_AI_Operations_ExtractTelemetry_RecordInputs_"></a>
<a id="Vercel_AI_Operations_ExtractTelemetry_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; set; }
```


When false, telemetry events carry no state, fields, or reference date.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L119)

<a id="Vercel_AI_Operations_ExtractTelemetry_RecordOutputs_"></a>
<a id="Vercel_AI_Operations_ExtractTelemetry_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; set; }
```


When false, telemetry end events carry no answers.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L122)
