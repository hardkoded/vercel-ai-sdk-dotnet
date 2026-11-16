---
title: "GoogleRealtimeSession"
description: "Session fields accepted by GoogleRealtime."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleRealtimeSession"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleRealtimeSession"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleRealtimeSession
```


Session fields accepted by [GoogleRealtime](/api/Vercel.AI.Google/GoogleRealtime/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L55)

## Properties

<a id="Vercel_AI_Google_GoogleRealtimeSession_InputAudioRate_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeSession_InputAudioRate"></a>

### InputAudioRate


```csharp
public int? InputAudioRate { get; set; }
```


Capture rate advertised on input audio blobs.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L82)

<a id="Vercel_AI_Google_GoogleRealtimeSession_InputAudioTranscription_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeSession_InputAudioTranscription"></a>

### InputAudioTranscription


```csharp
public bool InputAudioTranscription { get; set; }
```


When true, request input transcription.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L76)

<a id="Vercel_AI_Google_GoogleRealtimeSession_Instructions_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeSession_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; set; }
```


System instruction.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L61)

<a id="Vercel_AI_Google_GoogleRealtimeSession_ModelId_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeSession_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; set; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L58)

<a id="Vercel_AI_Google_GoogleRealtimeSession_OutputAudioTranscription_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeSession_OutputAudioTranscription"></a>

### OutputAudioTranscription


```csharp
public bool OutputAudioTranscription { get; set; }
```


When true, request output transcription.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L79)

<a id="Vercel_AI_Google_GoogleRealtimeSession_OutputModalities_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeSession_OutputModalities"></a>

### OutputModalities


```csharp
public IReadOnlyList<string>? OutputModalities { get; set; }
```


Output modalities. Defaults to audio.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L67)

<a id="Vercel_AI_Google_GoogleRealtimeSession_ProviderOptions_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeSession_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Provider options. `google` holds translation and thinking config. Other keys are copied onto setup.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L73)

<a id="Vercel_AI_Google_GoogleRealtimeSession_Tools_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeSession_Tools"></a>

### Tools


```csharp
public IReadOnlyList<GoogleRealtimeTool>? Tools { get; set; }
```


Function tools.

**Value:** [IReadOnlyList&lt;GoogleRealtimeTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L70)

<a id="Vercel_AI_Google_GoogleRealtimeSession_Voice_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeSession_Voice"></a>

### Voice


```csharp
public string? Voice { get; set; }
```


Voice name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L64)
