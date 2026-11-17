---
title: "TranscribeTelemetry"
description: "Telemetry callbacks for CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranscribeTelemetry"
  hidden: true
---

<a id="Vercel_AI_Operations_TranscribeTelemetry"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class TranscribeTelemetry
```


Telemetry callbacks for `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L562)

## Properties

<a id="Vercel_AI_Operations_TranscribeTelemetry_FunctionId_"></a>
<a id="Vercel_AI_Operations_TranscribeTelemetry_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; set; }
```


Identifies the call in telemetry.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L580)

<a id="Vercel_AI_Operations_TranscribeTelemetry_IsEnabled_"></a>
<a id="Vercel_AI_Operations_TranscribeTelemetry_IsEnabled"></a>

### IsEnabled


```csharp
public bool IsEnabled { get; set; }
```


When false, telemetry callbacks are skipped.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L583)

<a id="Vercel_AI_Operations_TranscribeTelemetry_OnEnd_"></a>
<a id="Vercel_AI_Operations_TranscribeTelemetry_OnEnd"></a>

### OnEnd


```csharp
public Func<TranscriptionEndEvent, Task>? OnEnd { get; set; }
```


Called when the operation ends.

**Value:** [Func&lt;TranscriptionEndEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L568)

<a id="Vercel_AI_Operations_TranscribeTelemetry_OnError_"></a>
<a id="Vercel_AI_Operations_TranscribeTelemetry_OnError"></a>

### OnError


```csharp
public Func<string, Exception, Task>? OnError { get; set; }
```


Called with the call id when the operation fails.

**Value:** [Func&lt;string, Exception, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L571)

<a id="Vercel_AI_Operations_TranscribeTelemetry_OnStart_"></a>
<a id="Vercel_AI_Operations_TranscribeTelemetry_OnStart"></a>

### OnStart


```csharp
public Func<TranscriptionStartEvent, Task>? OnStart { get; set; }
```


Called when the operation starts.

**Value:** [Func&lt;TranscriptionStartEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L565)

<a id="Vercel_AI_Operations_TranscribeTelemetry_RecordInputs_"></a>
<a id="Vercel_AI_Operations_TranscribeTelemetry_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; set; }
```


When false, input fields such as provider options are hidden from events.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L574)

<a id="Vercel_AI_Operations_TranscribeTelemetry_RecordOutputs_"></a>
<a id="Vercel_AI_Operations_TranscribeTelemetry_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; set; }
```


When false, `text`, `segments` and `words` are hidden from events.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L577)
