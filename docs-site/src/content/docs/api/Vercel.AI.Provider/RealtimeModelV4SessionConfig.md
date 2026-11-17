---
title: "RealtimeModelV4SessionConfig"
description: "Maps to RealtimeModelV4SessionConfig."
editUrl: false
prev: false
next: false
sidebar:
  label: "RealtimeModelV4SessionConfig"
  hidden: true
---

<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class RealtimeModelV4SessionConfig
```


Maps to `RealtimeModelV4SessionConfig`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L162)

## Properties

<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_InputAudioFormatRate_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_InputAudioFormatRate"></a>

### InputAudioFormatRate


```csharp
public int? InputAudioFormatRate { get; set; }
```


Input audio sample rate in Hz.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L177)

<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_InputAudioFormatType_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_InputAudioFormatType"></a>

### InputAudioFormatType


```csharp
public string? InputAudioFormatType { get; set; }
```


Input audio MIME type, for example `audio/pcm`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L174)

<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_InputAudioTranscription_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_InputAudioTranscription"></a>

### InputAudioTranscription


```csharp
public RealtimeModelV4AudioTranscription? InputAudioTranscription { get; set; }
```


Input audio transcription.

**Value:** [RealtimeModelV4AudioTranscription](/api/Vercel.AI.Provider/RealtimeModelV4AudioTranscription/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L186)

<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_Instructions_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; set; }
```


System instructions.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L165)

<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_OutputAudioFormatRate_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_OutputAudioFormatRate"></a>

### OutputAudioFormatRate


```csharp
public int? OutputAudioFormatRate { get; set; }
```


Output audio sample rate in Hz.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L183)

<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_OutputAudioFormatType_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_OutputAudioFormatType"></a>

### OutputAudioFormatType


```csharp
public string? OutputAudioFormatType { get; set; }
```


Output audio MIME type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L180)

<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_OutputAudioTranscription_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_OutputAudioTranscription"></a>

### OutputAudioTranscription


```csharp
public RealtimeModelV4AudioTranscription? OutputAudioTranscription { get; set; }
```


Output audio transcription.

**Value:** [RealtimeModelV4AudioTranscription](/api/Vercel.AI.Provider/RealtimeModelV4AudioTranscription/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L189)

<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_OutputModalities_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_OutputModalities"></a>

### OutputModalities


```csharp
public IReadOnlyList<string>? OutputModalities { get; set; }
```


`text` and/or `audio`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L171)

<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonObject? ProviderOptions { get; set; }
```


Provider-specific options.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L201)

<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_Tools_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_Tools"></a>

### Tools


```csharp
public IReadOnlyList<RealtimeModelV4ToolDefinition>? Tools { get; set; }
```


Function tools.

**Value:** [IReadOnlyList&lt;RealtimeModelV4ToolDefinition&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L198)

<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_TurnDetection_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_TurnDetection"></a>

### TurnDetection


```csharp
public RealtimeModelV4TurnDetection? TurnDetection { get; set; }
```


Turn detection. [TurnDetectionDisabled](#Vercel_AI_Provider_RealtimeModelV4SessionConfig_TurnDetectionDisabled) maps the upstream `null` value.

**Value:** [RealtimeModelV4TurnDetection](/api/Vercel.AI.Provider/RealtimeModelV4TurnDetection/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L192)

<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_TurnDetectionDisabled_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_TurnDetectionDisabled"></a>

### TurnDetectionDisabled


```csharp
public bool TurnDetectionDisabled { get; set; }
```


True when upstream passes `turnDetection: null` (no turn detection).

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L195)

<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_Voice_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4SessionConfig_Voice"></a>

### Voice


```csharp
public string? Voice { get; set; }
```


Voice name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L168)
