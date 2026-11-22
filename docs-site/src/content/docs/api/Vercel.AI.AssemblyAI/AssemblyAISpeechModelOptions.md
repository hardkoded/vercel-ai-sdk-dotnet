---
title: "AssemblyAISpeechModelOptions"
description: "Provider options for AssemblyAI speech generation, read from providerOptions.assemblyai. Maps to AssemblyAISpeechModelOptions of assemblyai-speech-model-opti..."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAISpeechModelOptions"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechModelOptions"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public sealed class AssemblyAISpeechModelOptions
```


Provider options for AssemblyAI speech generation, read from `providerOptions.assemblyai`.
Maps to `AssemblyAISpeechModelOptions` of `assemblyai-speech-model-options.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAISpeechModelOptions.cs#L14)

## Properties

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechModelOptions_BitRate_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechModelOptions_BitRate"></a>

### BitRate


```csharp
public int? BitRate { get; set; }
```


MP3 bit rate in bits per second (32000, 48000, 64000, 96000, 128000). MP3 output only. Maps to `bitRate`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAISpeechModelOptions.cs#L34)

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechModelOptions_Container_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechModelOptions_Container"></a>

### Container


```csharp
public string? Container { get; set; }
```


`raw` returns the encoded samples alone; `wav` wraps them in a WAV file. Requires a `pcm_*` encoding. Maps to `container`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAISpeechModelOptions.cs#L28)

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechModelOptions_Encoding_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechModelOptions_Encoding"></a>

### Encoding


```csharp
public string? Encoding { get; set; }
```


Audio encoding of the output (`pcm_s16le`, `pcm_mulaw`, `pcm_alaw`, or `mp3`). Overrides the encoding derived from the output format. Maps to `encoding`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAISpeechModelOptions.cs#L25)

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechModelOptions_SampleRate_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechModelOptions_SampleRate"></a>

### SampleRate


```csharp
public int? SampleRate { get; set; }
```


Sample rate of the output audio in Hz (8000, 16000, 22050, 24000, 44100, 48000). Maps to `sampleRate`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAISpeechModelOptions.cs#L31)

## Methods

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechModelOptions_Parse_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechModelOptions_Parse_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__"></a>

### `Parse(IReadOnlyDictionary<string, JsonElement>?)`


```csharp
public static AssemblyAISpeechModelOptions? Parse(IReadOnlyDictionary<string, JsonElement>? providerOptions)
```


Reads `providerOptions.assemblyai`. Maps to `parseProviderOptions` with `assemblyaiSpeechModelOptionsSchema`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [AssemblyAISpeechModelOptions](./)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/) | An option has the wrong shape or an unsupported value. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAISpeechModelOptions.cs#L38)
