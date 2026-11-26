---
title: "GenerateSpeechRequest"
description: "Options for CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateSpeechRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_GenerateSpeechRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class GenerateSpeechRequest : OperationRequest
```


Options for `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OperationRequest](/api/Vercel.AI.Operations/OperationRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L365)

## Properties

<a id="Vercel_AI_Operations_GenerateSpeechRequest_Instructions_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechRequest_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; set; }
```


Speaking instructions.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L380)

<a id="Vercel_AI_Operations_GenerateSpeechRequest_Language_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechRequest_Language"></a>

### Language


```csharp
public string? Language { get; set; }
```


Language code or `auto`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L386)

<a id="Vercel_AI_Operations_GenerateSpeechRequest_Model_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechRequest_Model"></a>

### Model


```csharp
public ISpeechCaller? Model { get; set; }
```


Speech model.

**Value:** [ISpeechCaller](/api/Vercel.AI.Operations/ISpeechCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L368)

<a id="Vercel_AI_Operations_GenerateSpeechRequest_OutputFormat_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechRequest_OutputFormat"></a>

### OutputFormat


```csharp
public string? OutputFormat { get; set; }
```


Output format such as `mp3` or `pcm`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L377)

<a id="Vercel_AI_Operations_GenerateSpeechRequest_Speed_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechRequest_Speed"></a>

### Speed


```csharp
public double? Speed { get; set; }
```


Playback speed.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L383)

<a id="Vercel_AI_Operations_GenerateSpeechRequest_Telemetry_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechRequest_Telemetry"></a>

### Telemetry


```csharp
public TelemetryOptions? Telemetry { get; set; }
```


Telemetry callbacks.

**Value:** [TelemetryOptions](/api/Vercel.AI/TelemetryOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L389)

<a id="Vercel_AI_Operations_GenerateSpeechRequest_Text_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechRequest_Text"></a>

### Text


```csharp
public string Text { get; set; }
```


Text to speak.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L371)

<a id="Vercel_AI_Operations_GenerateSpeechRequest_Voice_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechRequest_Voice"></a>

### Voice


```csharp
public string? Voice { get; set; }
```


Voice id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L374)
