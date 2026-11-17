---
title: "TranscriptionModelCall"
description: "Arguments for a transcription doGenerate."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranscriptionModelCall"
  hidden: true
---

<a id="Vercel_AI_Operations_TranscriptionModelCall"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class TranscriptionModelCall
```


Arguments for a transcription `doGenerate`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L27)

## Constructors

<a id="Vercel_AI_Operations_TranscriptionModelCall__ctor_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelCall__ctor_System_Byte___System_String_System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `TranscriptionModelCall(byte[], string, JsonElement, IReadOnlyDictionary<string, string>, CancellationToken)`


```csharp
public TranscriptionModelCall(byte[] audio, string mediaType, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
```


Creates a call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L30)

<a id="Vercel_AI_Operations_TranscriptionModelCall__ctor_System_String_System_String_System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `TranscriptionModelCall(string, string, JsonElement, IReadOnlyDictionary<string, string>, CancellationToken)`


```csharp
public TranscriptionModelCall(string audioBase64, string mediaType, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
```


Creates a call whose audio is a base64 string. Maps to upstream's `audio: string`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audioBase64` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L46)

## Properties

<a id="Vercel_AI_Operations_TranscriptionModelCall_Audio_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelCall_Audio"></a>

### Audio


```csharp
public byte[] Audio { get; }
```


Audio bytes. When the call was created from a base64 string, the string is decoded on first access, so a
provider that forwards the string untouched never decodes it.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L65)

<a id="Vercel_AI_Operations_TranscriptionModelCall_AudioBase64_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelCall_AudioBase64"></a>

### AudioBase64


```csharp
public string? AudioBase64 { get; }
```


The audio as the base64 string the caller passed, or &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt; when the call carries bytes.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L74)

<a id="Vercel_AI_Operations_TranscriptionModelCall_CancellationToken_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelCall_CancellationToken"></a>

### CancellationToken


```csharp
public CancellationToken CancellationToken { get; }
```


Cancellation token.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L92)

<a id="Vercel_AI_Operations_TranscriptionModelCall_Headers_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelCall_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Headers including the user-agent suffix.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L89)

<a id="Vercel_AI_Operations_TranscriptionModelCall_MediaType_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelCall_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


Detected media type, or `audio/wav`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L77)

<a id="Vercel_AI_Operations_TranscriptionModelCall_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelCall_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement ProviderOptions { get; }
```


Provider options.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L86)

<a id="Vercel_AI_Operations_TranscriptionModelCall_TimestampGranularities_"></a>
<a id="Vercel_AI_Operations_TranscriptionModelCall_TimestampGranularities"></a>

### TimestampGranularities


```csharp
public IReadOnlyList<string>? TimestampGranularities { get; init; }
```


Timing granularities the caller asks for, `word` and `segment`. An empty list requests no additional
timing detail and does not suppress timings returned by default. Maps to `timestampGranularities`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L83)
