---
title: "SpeechModelStreamCall"
description: "Arguments for doStream on a speech model. Maps to SpeechModelV4StreamOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "SpeechModelStreamCall"
  hidden: true
---

<a id="Vercel_AI_Operations_SpeechModelStreamCall"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class SpeechModelStreamCall
```


Arguments for `doStream` on a speech model. Maps to `SpeechModelV4StreamOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L205)

## Constructors

<a id="Vercel_AI_Operations_SpeechModelStreamCall__ctor_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamCall__ctor_Vercel_AI_Operations_SpeechTextStream_System_String_Vercel_AI_Operations_SpeechAudioFormat_System_String_System_Nullable_System_Double__System_String_System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Nullable_System_Boolean__System_Threading_CancellationToken_"></a>

### `SpeechModelStreamCall(SpeechTextStream, string?, SpeechAudioFormat?, string?, double?, string?, JsonElement, IReadOnlyDictionary<string, string>, bool?, CancellationToken)`


```csharp
public SpeechModelStreamCall(SpeechTextStream text, string? voice, SpeechAudioFormat? outputFormat, string? instructions, double? speed, string? language, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, bool? includeRawChunks, CancellationToken cancellationToken)
```


Creates a speech stream call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [SpeechTextStream](/api/Vercel.AI.Operations/SpeechTextStream/) |  |
| `voice` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `outputFormat` | [SpeechAudioFormat](/api/Vercel.AI.Operations/SpeechAudioFormat/) |  |
| `instructions` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `speed` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `language` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `includeRawChunks` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L208)

## Properties

<a id="Vercel_AI_Operations_SpeechModelStreamCall_CancellationToken_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamCall_CancellationToken"></a>

### CancellationToken


```csharp
public CancellationToken CancellationToken { get; }
```


Cancels the operation. Also fires when the consumer cancels the result stream.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L253)

<a id="Vercel_AI_Operations_SpeechModelStreamCall_Headers_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamCall_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Headers including the user-agent suffix.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L247)

<a id="Vercel_AI_Operations_SpeechModelStreamCall_IncludeRawChunks_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamCall_IncludeRawChunks"></a>

### IncludeRawChunks


```csharp
public bool? IncludeRawChunks { get; }
```


When true, providers include raw provider chunks in the stream.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L250)

<a id="Vercel_AI_Operations_SpeechModelStreamCall_Instructions_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamCall_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; }
```


Speaking instructions.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L235)

<a id="Vercel_AI_Operations_SpeechModelStreamCall_Language_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamCall_Language"></a>

### Language


```csharp
public string? Language { get; }
```


ISO 639-1 language code or `auto`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L241)

<a id="Vercel_AI_Operations_SpeechModelStreamCall_OutputFormat_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamCall_OutputFormat"></a>

### OutputFormat


```csharp
public SpeechAudioFormat? OutputFormat { get; }
```


Desired format of the output audio chunks. The provider default applies when null.

**Value:** [SpeechAudioFormat](/api/Vercel.AI.Operations/SpeechAudioFormat/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L232)

<a id="Vercel_AI_Operations_SpeechModelStreamCall_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamCall_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement ProviderOptions { get; }
```


Provider options. Empty object when omitted.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L244)

<a id="Vercel_AI_Operations_SpeechModelStreamCall_Speed_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamCall_Speed"></a>

### Speed


```csharp
public double? Speed { get; }
```


Playback speed.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L238)

<a id="Vercel_AI_Operations_SpeechModelStreamCall_Text_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamCall_Text"></a>

### Text


```csharp
public SpeechTextStream Text { get; }
```


Text to convert to speech, as a stream of text parts. The provider owns it once `doStream` is called and
releases it on every terminal path.

**Value:** [SpeechTextStream](/api/Vercel.AI.Operations/SpeechTextStream/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L226)

<a id="Vercel_AI_Operations_SpeechModelStreamCall_Voice_"></a>
<a id="Vercel_AI_Operations_SpeechModelStreamCall_Voice"></a>

### Voice


```csharp
public string? Voice { get; }
```


Voice to use. Provider-specific.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L229)
