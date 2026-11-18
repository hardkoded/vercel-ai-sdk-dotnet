---
title: "MistralSpeechModelOptions"
description: "Mistral speech options. Maps to mistralSpeechModelOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralSpeechModelOptions"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralSpeechModelOptions"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralSpeechModelOptions
```


Mistral speech options. Maps to `mistralSpeechModelOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralSpeechModelOptions.cs#L12)

## Properties

<a id="Vercel_AI_Mistral_MistralSpeechModelOptions_Expressiveness_"></a>
<a id="Vercel_AI_Mistral_MistralSpeechModelOptions_Expressiveness"></a>

### Expressiveness


```csharp
public double? Expressiveness { get; init; }
```


How expressive the speech is, from 1 to 5. Supported by Voxtral 3.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralSpeechModelOptions.cs#L18)

<a id="Vercel_AI_Mistral_MistralSpeechModelOptions_PhonemeHints_"></a>
<a id="Vercel_AI_Mistral_MistralSpeechModelOptions_PhonemeHints"></a>

### PhonemeHints


```csharp
public IReadOnlyDictionary<string, IReadOnlyList<string>>? PhonemeHints { get; init; }
```


Pronunciation hints mapping words to lists of IPA phonemes. Supported by Voxtral 3.

**Value:** [IReadOnlyDictionary&lt;string, IReadOnlyList&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralSpeechModelOptions.cs#L21)

<a id="Vercel_AI_Mistral_MistralSpeechModelOptions_RefAudio_"></a>
<a id="Vercel_AI_Mistral_MistralSpeechModelOptions_RefAudio"></a>

### RefAudio


```csharp
public string? RefAudio { get; init; }
```


Base64-encoded reference audio for one-off voice cloning. It takes precedence over the standard voice option.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralSpeechModelOptions.cs#L15)

## Methods

<a id="Vercel_AI_Mistral_MistralSpeechModelOptions_Parse_"></a>
<a id="Vercel_AI_Mistral_MistralSpeechModelOptions_Parse_System_Text_Json_JsonElement_"></a>

### Parse(JsonElement)


```csharp
public static MistralSpeechModelOptions? Parse(JsonElement providerOptions)
```


Parses `providerOptions.mistral`. Maps to `parseProviderOptions` with the speech schema.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [MistralSpeechModelOptions](./)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/) | An option has the wrong shape or is out of range. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralSpeechModelOptions.cs#L25)
