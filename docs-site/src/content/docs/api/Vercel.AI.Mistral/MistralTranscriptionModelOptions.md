---
title: "MistralTranscriptionModelOptions"
description: "Mistral transcription options. Maps to mistralTranscriptionModelOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralTranscriptionModelOptions"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralTranscriptionModelOptions
```


Mistral transcription options. Maps to `mistralTranscriptionModelOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralTranscriptionModelOptions.cs#L29)

## Properties

<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_ContextBias_"></a>
<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_ContextBias"></a>

### ContextBias


```csharp
public IReadOnlyList<string>? ContextBias { get; init; }
```


Words or phrases that guide the model toward correct spellings.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralTranscriptionModelOptions.cs#L47)

<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_Diarize_"></a>
<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_Diarize"></a>

### Diarize


```csharp
public bool? Diarize { get; init; }
```


Whether to identify speakers in the transcription.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralTranscriptionModelOptions.cs#L44)

<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_Language_"></a>
<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_Language"></a>

### Language


```csharp
public string? Language { get; init; }
```


The language of the audio, such as `en`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralTranscriptionModelOptions.cs#L32)

<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_ReturnLanguage_"></a>
<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_ReturnLanguage"></a>

### ReturnLanguage


```csharp
public bool? ReturnLanguage { get; init; }
```


Return the detected language. Cannot be combined with [Language](#Vercel_AI_Mistral_MistralTranscriptionModelOptions_Language).

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralTranscriptionModelOptions.cs#L35)

<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_SpeakerReferences_"></a>
<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_SpeakerReferences"></a>

### SpeakerReferences


```csharp
public IReadOnlyDictionary<string, string>? SpeakerReferences { get; init; }
```


Realtime only. Named base64-encoded audio clips for known speakers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralTranscriptionModelOptions.cs#L53)

<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_TargetStreamingDelayMs_"></a>
<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_TargetStreamingDelayMs"></a>

### TargetStreamingDelayMs


```csharp
public int? TargetStreamingDelayMs { get; init; }
```


Realtime only. Delay in milliseconds: 80–1200, or 2400.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralTranscriptionModelOptions.cs#L50)

<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_Temperature_"></a>
<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_Temperature"></a>

### Temperature


```csharp
public double? Temperature { get; init; }
```


The sampling temperature.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralTranscriptionModelOptions.cs#L38)

<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_TimestampGranularities_"></a>
<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_TimestampGranularities"></a>

### TimestampGranularities


```csharp
public IReadOnlyList<string>? TimestampGranularities { get; init; }
```


The timestamp granularities to include in the response, `segment` and `word`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralTranscriptionModelOptions.cs#L41)

<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_TurnDetection_"></a>
<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_TurnDetection"></a>

### TurnDetection


```csharp
public MistralTurnDetection? TurnDetection { get; init; }
```


Realtime only. Turn detection settings. An empty object selects the defaults.

**Value:** [MistralTurnDetection](/api/Vercel.AI.Mistral/MistralTurnDetection/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralTranscriptionModelOptions.cs#L56)

## Methods

<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_Parse_"></a>
<a id="Vercel_AI_Mistral_MistralTranscriptionModelOptions_Parse_System_Text_Json_JsonElement_"></a>

### Parse(JsonElement)


```csharp
public static MistralTranscriptionModelOptions? Parse(JsonElement providerOptions)
```


Parses `providerOptions.mistral`. Maps to `parseProviderOptions` with the transcription schema.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [MistralTranscriptionModelOptions](./)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/) | An option has the wrong shape or is out of range. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralTranscriptionModelOptions.cs#L60)
