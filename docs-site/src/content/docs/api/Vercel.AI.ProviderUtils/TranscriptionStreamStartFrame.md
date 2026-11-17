---
title: "TranscriptionStreamStartFrame"
description: "The client's session start frame. Maps to TranscriptionStreamStartFrame. Optional keys are omitted when null."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranscriptionStreamStartFrame"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_TranscriptionStreamStartFrame"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class TranscriptionStreamStartFrame
```


The client's session start frame. Maps to `TranscriptionStreamStartFrame`. Optional keys are omitted when null.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TranscriptionStreamEnvelope.cs#L16)

## Constructors

<a id="Vercel_AI_ProviderUtils_TranscriptionStreamStartFrame__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_TranscriptionStreamStartFrame__ctor_Vercel_AI_Operations_AudioFormat_System_Nullable_System_Text_Json_JsonElement__System_Nullable_System_Boolean__"></a>

### TranscriptionStreamStartFrame(AudioFormat, JsonElement?, bool?)


```csharp
public TranscriptionStreamStartFrame(AudioFormat inputAudioFormat, JsonElement? providerOptions = null, bool? includeRawChunks = null)
```


Creates a start frame.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `inputAudioFormat` | [AudioFormat](/api/Vercel.AI.Operations/AudioFormat/) |  |
| `providerOptions` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `includeRawChunks` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TranscriptionStreamEnvelope.cs#L19)

## Properties

<a id="Vercel_AI_ProviderUtils_TranscriptionStreamStartFrame_IncludeRawChunks_"></a>
<a id="Vercel_AI_ProviderUtils_TranscriptionStreamStartFrame_IncludeRawChunks"></a>

### IncludeRawChunks


```csharp
public bool? IncludeRawChunks { get; }
```


When true, the server should include `raw` parts in the stream.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TranscriptionStreamEnvelope.cs#L41)

<a id="Vercel_AI_ProviderUtils_TranscriptionStreamStartFrame_InputAudioFormat_"></a>
<a id="Vercel_AI_ProviderUtils_TranscriptionStreamStartFrame_InputAudioFormat"></a>

### InputAudioFormat


```csharp
public AudioFormat InputAudioFormat { get; }
```


Audio format of the binary audio frames, for example `audio/pcm` at 16000 Hz.

**Value:** [AudioFormat](/api/Vercel.AI.Operations/AudioFormat/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TranscriptionStreamEnvelope.cs#L35)

<a id="Vercel_AI_ProviderUtils_TranscriptionStreamStartFrame_ProviderOptions_"></a>
<a id="Vercel_AI_ProviderUtils_TranscriptionStreamStartFrame_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; }
```


Provider-specific options, passed through verbatim.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TranscriptionStreamEnvelope.cs#L38)

<a id="Vercel_AI_ProviderUtils_TranscriptionStreamStartFrame_Type_"></a>
<a id="Vercel_AI_ProviderUtils_TranscriptionStreamStartFrame_Type"></a>

### Type


```csharp
public string Type { get; }
```


Always [TranscriptionStreamStartFrameType](/api/Vercel.AI.ProviderUtils/TranscriptionStreamEnvelope/#Vercel_AI_ProviderUtils_TranscriptionStreamEnvelope_TranscriptionStreamStartFrameType).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TranscriptionStreamEnvelope.cs#L32)

## Methods

<a id="Vercel_AI_ProviderUtils_TranscriptionStreamStartFrame_ToJson_"></a>
<a id="Vercel_AI_ProviderUtils_TranscriptionStreamStartFrame_ToJson"></a>

### ToJson()


```csharp
public string ToJson()
```


Serializes the frame. Optional keys are omitted when null.

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TranscriptionStreamEnvelope.cs#L44)
