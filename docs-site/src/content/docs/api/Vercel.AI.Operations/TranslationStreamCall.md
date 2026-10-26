---
title: "TranslationStreamCall"
description: "Arguments for a speech-translation stream."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranslationStreamCall"
  hidden: true
---

<a id="Vercel_AI_Operations_TranslationStreamCall"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class TranslationStreamCall
```


Arguments for a speech-translation stream.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L10)

## Constructors

<a id="Vercel_AI_Operations_TranslationStreamCall__ctor_"></a>
<a id="Vercel_AI_Operations_TranslationStreamCall__ctor_Vercel_AI_Operations_AudioChunkStream_System_String_System_String_System_String_System_String_System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Nullable_System_Boolean__System_Threading_CancellationToken_"></a>

### `TranslationStreamCall(AudioChunkStream, string, string, string?, string?, JsonElement, IReadOnlyDictionary<string, string>, bool?, CancellationToken)`


```csharp
public TranslationStreamCall(AudioChunkStream audio, string inputAudioFormat, string targetLanguage, string? sourceLanguage, string? outputAudioFormat, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, bool? includeRawChunks, CancellationToken cancellationToken)
```


Creates a translation call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [AudioChunkStream](/api/Vercel.AI.Operations/AudioChunkStream/) |  |
| `inputAudioFormat` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `targetLanguage` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `sourceLanguage` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `outputAudioFormat` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `includeRawChunks` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L13)

## Properties

<a id="Vercel_AI_Operations_TranslationStreamCall_Audio_"></a>
<a id="Vercel_AI_Operations_TranslationStreamCall_Audio"></a>

### Audio


```csharp
public AudioChunkStream Audio { get; }
```


Audio stream.

**Value:** [AudioChunkStream](/api/Vercel.AI.Operations/AudioChunkStream/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L27)

<a id="Vercel_AI_Operations_TranslationStreamCall_CancellationToken_"></a>
<a id="Vercel_AI_Operations_TranslationStreamCall_CancellationToken"></a>

### CancellationToken


```csharp
public CancellationToken CancellationToken { get; }
```


Cancellation token shared with `fullStream` cancellation.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L51)

<a id="Vercel_AI_Operations_TranslationStreamCall_Headers_"></a>
<a id="Vercel_AI_Operations_TranslationStreamCall_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Headers including the user-agent suffix.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L45)

<a id="Vercel_AI_Operations_TranslationStreamCall_IncludeRawChunks_"></a>
<a id="Vercel_AI_Operations_TranslationStreamCall_IncludeRawChunks"></a>

### IncludeRawChunks


```csharp
public bool? IncludeRawChunks { get; }
```


When true, raw parts are requested.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L48)

<a id="Vercel_AI_Operations_TranslationStreamCall_InputAudioFormat_"></a>
<a id="Vercel_AI_Operations_TranslationStreamCall_InputAudioFormat"></a>

### InputAudioFormat


```csharp
public string InputAudioFormat { get; }
```


Input audio format.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L30)

<a id="Vercel_AI_Operations_TranslationStreamCall_OutputAudioFormat_"></a>
<a id="Vercel_AI_Operations_TranslationStreamCall_OutputAudioFormat"></a>

### OutputAudioFormat


```csharp
public string? OutputAudioFormat { get; }
```


Requested output audio format.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L39)

<a id="Vercel_AI_Operations_TranslationStreamCall_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_TranslationStreamCall_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement ProviderOptions { get; }
```


Provider options.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L42)

<a id="Vercel_AI_Operations_TranslationStreamCall_SourceLanguage_"></a>
<a id="Vercel_AI_Operations_TranslationStreamCall_SourceLanguage"></a>

### SourceLanguage


```csharp
public string? SourceLanguage { get; }
```


Source language. Null asks the provider to detect it.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L36)

<a id="Vercel_AI_Operations_TranslationStreamCall_TargetLanguage_"></a>
<a id="Vercel_AI_Operations_TranslationStreamCall_TargetLanguage"></a>

### TargetLanguage


```csharp
public string TargetLanguage { get; }
```


Target language.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L33)
