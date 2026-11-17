---
title: "TranscriptionModelUrlCall"
description: "Arguments for a native URL transcription doGenerateFromUrl. Carries no audio bytes and no media type."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranscriptionModelUrlCall"
  hidden: true
---

<a id="Vercel_AI_Operations_TranscriptionModelUrlCall"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class TranscriptionModelUrlCall
```


Arguments for a native URL transcription `doGenerateFromUrl`. Carries no audio bytes and no media type.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L96)

## Constructors

<a id="Vercel_AI_Operations_TranscriptionModelUrlCall__ctor_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelUrlCall__ctor_System_Uri_System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `TranscriptionModelUrlCall(Uri, JsonElement, IReadOnlyDictionary<string, string>, CancellationToken)`


```csharp
public TranscriptionModelUrlCall(Uri audio, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
```


Creates a call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L99)

## Properties

<a id="Vercel_AI_Operations_TranscriptionModelUrlCall_Audio_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelUrlCall_Audio"></a>

### Audio


```csharp
public Uri Audio { get; }
```


HTTP(S) URL the provider fetches itself.

**Value:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L113)

<a id="Vercel_AI_Operations_TranscriptionModelUrlCall_CancellationToken_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelUrlCall_CancellationToken"></a>

### CancellationToken


```csharp
public CancellationToken CancellationToken { get; }
```


Cancellation token.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L125)

<a id="Vercel_AI_Operations_TranscriptionModelUrlCall_Headers_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelUrlCall_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Headers including the user-agent suffix. They are for the provider API and are not forwarded to the audio host.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L122)

<a id="Vercel_AI_Operations_TranscriptionModelUrlCall_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelUrlCall_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement ProviderOptions { get; }
```


Provider options.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L119)

<a id="Vercel_AI_Operations_TranscriptionModelUrlCall_TimestampGranularities_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelUrlCall_TimestampGranularities"></a>

### TimestampGranularities


```csharp
public IReadOnlyList<string>? TimestampGranularities { get; init; }
```


Timing granularities the caller asks for. Maps to `timestampGranularities`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L116)
