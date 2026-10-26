---
title: "OpenTelemetryAiTelemetry"
description: "IAiTelemetry that records spans on ActivitySource."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenTelemetryAiTelemetry"
  hidden: true
---

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryAiTelemetry"></a>

Namespace: [Vercel.AI.OpenTelemetry](/api/Vercel.AI.OpenTelemetry/)


```csharp
public sealed class OpenTelemetryAiTelemetry : IAiTelemetry
```


[IAiTelemetry](/api/Vercel.AI/IAiTelemetry/) that records spans on [ActivitySource](#Vercel_AI_OpenTelemetry_OpenTelemetryAiTelemetry_ActivitySource).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IAiTelemetry](/api/Vercel.AI/IAiTelemetry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetryAiTelemetry.cs#L12)

## Fields

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryAiTelemetry_ActivitySource"></a>

### ActivitySource


```csharp
public static readonly ActivitySource ActivitySource
```


Source used by `String)`.

**Value:** [ActivitySource](https://learn.microsoft.com/dotnet/api/system.diagnostics.activitysource)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetryAiTelemetry.cs#L18)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryAiTelemetry_SourceName"></a>

### SourceName


```csharp
public const string SourceName = "Vercel.AI"
```


Activity source name. Add this source to a `TracerProvider`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetryAiTelemetry.cs#L15)

## Methods

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryAiTelemetry_Begin_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryAiTelemetry_Begin_System_String_System_String_"></a>

### Begin(string, string)


```csharp
public IDisposable Begin(string operation, string modelId)
```


Starts a span. Dispose it when the call finishes.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `operation` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetryAiTelemetry.cs#L21)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryAiTelemetry_OnFinish_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryAiTelemetry_OnFinish_System_IDisposable_Vercel_AI_Provider_FinishReason_"></a>

### OnFinish(IDisposable, FinishReason)


```csharp
public void OnFinish(IDisposable span, FinishReason finishReason)
```


Records the final finish reason on the span opened by `String)`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `span` | [IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable) |  |
| `finishReason` | [FinishReason](/api/Vercel.AI.Provider/FinishReason/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetryAiTelemetry.cs#L35)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryAiTelemetry_OnUsage_"></a>
<a id="Vercel_AI_OpenTelemetry_OpenTelemetryAiTelemetry_OnUsage_System_IDisposable_Vercel_AI_Provider_AudioUsage_"></a>

### OnUsage(IDisposable, AudioUsage?)


```csharp
public void OnUsage(IDisposable span, AudioUsage? usage)
```


Records speech or transcription usage on the span opened by `String)`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `span` | [IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable) |  |
| `usage` | [AudioUsage](/api/Vercel.AI.Provider/AudioUsage/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetryAiTelemetry.cs#L65)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetryAiTelemetry_OnUsage_System_IDisposable_Vercel_AI_Provider_LanguageModelUsage_"></a>

### OnUsage(IDisposable, LanguageModelUsage)


```csharp
public void OnUsage(IDisposable span, LanguageModelUsage usage)
```


Records token usage on the span opened by `String)`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `span` | [IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable) |  |
| `usage` | [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/OpenTelemetryAiTelemetry.cs#L50)
