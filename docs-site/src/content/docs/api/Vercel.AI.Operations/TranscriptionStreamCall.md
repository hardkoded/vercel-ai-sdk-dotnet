---
title: "TranscriptionStreamCall"
description: "Arguments for streaming transcription."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranscriptionStreamCall"
  hidden: true
---

<a id="Vercel_AI_Operations_TranscriptionStreamCall"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class TranscriptionStreamCall
```


Arguments for streaming transcription.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L93)

## Constructors

<a id="Vercel_AI_Operations_TranscriptionStreamCall__ctor_"></a>
<a id="Vercel_AI_Operations_TranscriptionStreamCall__ctor_Vercel_AI_Operations_AudioChunkStream_System_String_System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Nullable_System_Boolean__System_Threading_CancellationToken_"></a>

### `TranscriptionStreamCall(AudioChunkStream, string, JsonElement, IReadOnlyDictionary<string, string>, bool?, CancellationToken)`


```csharp
public TranscriptionStreamCall(AudioChunkStream audio, string inputAudioFormat, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, bool? includeRawChunks, CancellationToken cancellationToken)
```


Creates a stream call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [AudioChunkStream](/api/Vercel.AI.Operations/AudioChunkStream/) |  |
| `inputAudioFormat` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `includeRawChunks` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L96)

## Properties

<a id="Vercel_AI_Operations_TranscriptionStreamCall_Audio_"></a>
<a id="Vercel_AI_Operations_TranscriptionStreamCall_Audio"></a>

### Audio


```csharp
public AudioChunkStream Audio { get; }
```


Audio stream.

**Value:** [AudioChunkStream](/api/Vercel.AI.Operations/AudioChunkStream/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L107)

<a id="Vercel_AI_Operations_TranscriptionStreamCall_CancellationToken_"></a>
<a id="Vercel_AI_Operations_TranscriptionStreamCall_CancellationToken"></a>

### CancellationToken


```csharp
public CancellationToken CancellationToken { get; }
```


Cancellation token shared with `fullStream` cancellation.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L122)

<a id="Vercel_AI_Operations_TranscriptionStreamCall_Headers_"></a>
<a id="Vercel_AI_Operations_TranscriptionStreamCall_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Headers including the user-agent suffix.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L116)

<a id="Vercel_AI_Operations_TranscriptionStreamCall_IncludeRawChunks_"></a>
<a id="Vercel_AI_Operations_TranscriptionStreamCall_IncludeRawChunks"></a>

### IncludeRawChunks


```csharp
public bool? IncludeRawChunks { get; }
```


When true, raw parts are requested.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L119)

<a id="Vercel_AI_Operations_TranscriptionStreamCall_InputAudioFormat_"></a>
<a id="Vercel_AI_Operations_TranscriptionStreamCall_InputAudioFormat"></a>

### InputAudioFormat


```csharp
public string InputAudioFormat { get; }
```


Input audio format.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L110)

<a id="Vercel_AI_Operations_TranscriptionStreamCall_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_TranscriptionStreamCall_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement ProviderOptions { get; }
```


Provider options.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L113)
