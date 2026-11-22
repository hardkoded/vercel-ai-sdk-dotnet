---
title: "TelemetryErrorEvent"
description: "The event an integration receives in OnError. Upstream passes the plain object { callId, error, aiVersion } to onError; the port needs a type for it."
editUrl: false
prev: false
next: false
sidebar:
  label: "TelemetryErrorEvent"
  hidden: true
---

<a id="Vercel_AI_TelemetryErrorEvent"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class TelemetryErrorEvent
```


The event an integration receives in [OnError](/api/Vercel.AI/ITelemetry/#Vercel_AI_ITelemetry_OnError). Upstream passes the plain object
`{ callId, error, aiVersion }` to `onError`; the port needs a type for it.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryErrorEvent.cs#L11)

## Constructors

<a id="Vercel_AI_TelemetryErrorEvent__ctor_"></a>
<a id="Vercel_AI_TelemetryErrorEvent__ctor_System_String_System_Exception_"></a>

### TelemetryErrorEvent(string, Exception)


```csharp
public TelemetryErrorEvent(string callId, Exception error)
```


Creates an event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryErrorEvent.cs#L14)

## Properties

<a id="Vercel_AI_TelemetryErrorEvent_AiVersion_"></a>
<a id="Vercel_AI_TelemetryErrorEvent_AiVersion"></a>

### AiVersion


```csharp
public string? AiVersion { get; init; }
```


The version of the AI SDK that emitted this event.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryErrorEvent.cs#L27)

<a id="Vercel_AI_TelemetryErrorEvent_CallId_"></a>
<a id="Vercel_AI_TelemetryErrorEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Unique identifier of the failed call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryErrorEvent.cs#L21)

<a id="Vercel_AI_TelemetryErrorEvent_Error_"></a>
<a id="Vercel_AI_TelemetryErrorEvent_Error"></a>

### Error


```csharp
public Exception Error { get; }
```


The error. A download error arrives without its URL.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryErrorEvent.cs#L24)

<a id="Vercel_AI_TelemetryErrorEvent_FunctionId_"></a>
<a id="Vercel_AI_TelemetryErrorEvent_FunctionId"></a>

### FunctionId


```csharp
public string? FunctionId { get; init; }
```


Copied from [FunctionId](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_FunctionId).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryErrorEvent.cs#L36)

<a id="Vercel_AI_TelemetryErrorEvent_RecordInputs_"></a>
<a id="Vercel_AI_TelemetryErrorEvent_RecordInputs"></a>

### RecordInputs


```csharp
public bool? RecordInputs { get; init; }
```


Copied from [RecordInputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordInputs).

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryErrorEvent.cs#L30)

<a id="Vercel_AI_TelemetryErrorEvent_RecordOutputs_"></a>
<a id="Vercel_AI_TelemetryErrorEvent_RecordOutputs"></a>

### RecordOutputs


```csharp
public bool? RecordOutputs { get; init; }
```


Copied from [RecordOutputs](/api/Vercel.AI/TelemetryOptions/#Vercel_AI_TelemetryOptions_RecordOutputs).

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Telemetry/TelemetryErrorEvent.cs#L33)
