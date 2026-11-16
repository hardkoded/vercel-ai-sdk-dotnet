---
title: "XaiTranscriptionStreamingOptions"
description: "Options of streaming for streaming speech-to-text over WebSocket."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiTranscriptionStreamingOptions"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiTranscriptionStreamingOptions"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiTranscriptionStreamingOptions
```


Options of `streaming` for streaming speech-to-text over WebSocket.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModelOptions.cs#L12)

## Properties

<a id="Vercel_AI_Xai_XaiTranscriptionStreamingOptions_Endpointing_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionStreamingOptions_Endpointing"></a>

### Endpointing


```csharp
public int? Endpointing { get; set; }
```


Silence duration in milliseconds before an utterance-final event. Between 0 and 5000.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModelOptions.cs#L18)

<a id="Vercel_AI_Xai_XaiTranscriptionStreamingOptions_InterimResults_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionStreamingOptions_InterimResults"></a>

### InterimResults


```csharp
public bool? InterimResults { get; set; }
```


Emit interim transcript results while speech is being processed.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModelOptions.cs#L15)

<a id="Vercel_AI_Xai_XaiTranscriptionStreamingOptions_SmartTurn_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionStreamingOptions_SmartTurn"></a>

### SmartTurn


```csharp
public double? SmartTurn { get; set; }
```


End-of-turn detection threshold between 0 and 1. When set, enables Smart Turn.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModelOptions.cs#L21)

<a id="Vercel_AI_Xai_XaiTranscriptionStreamingOptions_SmartTurnTimeout_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionStreamingOptions_SmartTurnTimeout"></a>

### SmartTurnTimeout


```csharp
public int? SmartTurnTimeout { get; set; }
```


Maximum silence duration in milliseconds before forcing `speech_final`. Between 1 and 5000.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModelOptions.cs#L24)
