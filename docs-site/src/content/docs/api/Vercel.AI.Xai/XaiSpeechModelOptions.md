---
title: "XaiSpeechModelOptions"
description: "Provider options for xAI speech generation, read from providerOptions.xai. Maps to XaiSpeechModelOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiSpeechModelOptions"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiSpeechModelOptions"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiSpeechModelOptions
```


Provider options for xAI speech generation, read from `providerOptions.xai`. Maps to `XaiSpeechModelOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiSpeechModelOptions.cs#L11)

## Properties

<a id="Vercel_AI_Xai_XaiSpeechModelOptions_BitRate_"></a>
<a id="Vercel_AI_Xai_XaiSpeechModelOptions_BitRate"></a>

### BitRate


```csharp
public int? BitRate { get; set; }
```


MP3 bit rate in bits per second. Only applies when the output format is mp3. Maps to `bitRate`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiSpeechModelOptions.cs#L23)

<a id="Vercel_AI_Xai_XaiSpeechModelOptions_OptimizeStreamingLatency_"></a>
<a id="Vercel_AI_Xai_XaiSpeechModelOptions_OptimizeStreamingLatency"></a>

### OptimizeStreamingLatency


```csharp
public int? OptimizeStreamingLatency { get; set; }
```


Reduces time to first audio chunk, trading some quality for latency. Maps to `optimizeStreamingLatency`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiSpeechModelOptions.cs#L26)

<a id="Vercel_AI_Xai_XaiSpeechModelOptions_Replace_"></a>
<a id="Vercel_AI_Xai_XaiSpeechModelOptions_Replace"></a>

### Replace


```csharp
public IReadOnlyDictionary<string, string>? Replace { get; set; }
```


Map of phrases to spoken substitutions applied before synthesis. Maps to `replace`.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiSpeechModelOptions.cs#L37)

<a id="Vercel_AI_Xai_XaiSpeechModelOptions_SampleRate_"></a>
<a id="Vercel_AI_Xai_XaiSpeechModelOptions_SampleRate"></a>

### SampleRate


```csharp
public int? SampleRate { get; set; }
```


Sample rate of the generated audio in Hz. Maps to `sampleRate`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiSpeechModelOptions.cs#L20)

<a id="Vercel_AI_Xai_XaiSpeechModelOptions_TextNormalization_"></a>
<a id="Vercel_AI_Xai_XaiSpeechModelOptions_TextNormalization"></a>

### TextNormalization


```csharp
public bool? TextNormalization { get; set; }
```


Normalizes written-form text into spoken-form text before synthesis. Maps to `textNormalization`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiSpeechModelOptions.cs#L29)

<a id="Vercel_AI_Xai_XaiSpeechModelOptions_WithTimestamps_"></a>
<a id="Vercel_AI_Xai_XaiSpeechModelOptions_WithTimestamps"></a>

### WithTimestamps


```csharp
public bool? WithTimestamps { get; set; }
```


Returns character-level timing metadata alongside the audio, exposed through `providerMetadata.xai`. Maps to `withTimestamps`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiSpeechModelOptions.cs#L34)

## Methods

<a id="Vercel_AI_Xai_XaiSpeechModelOptions_Parse_"></a>
<a id="Vercel_AI_Xai_XaiSpeechModelOptions_Parse_System_Text_Json_JsonElement_"></a>

### Parse(JsonElement)


```csharp
public static XaiSpeechModelOptions? Parse(JsonElement providerOptions)
```


Reads `providerOptions.xai`. Maps to `parseProviderOptions` with `xaiSpeechModelOptionsSchema`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [XaiSpeechModelOptions](./)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/) | An option has the wrong shape or an unsupported value. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiSpeechModelOptions.cs#L41)
