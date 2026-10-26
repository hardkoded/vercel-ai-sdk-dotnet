---
title: "SpeechTextPart"
description: "One piece of streamed input text. Consecutive parts are synthesized in order as one continuous script. Maps to SpeechTextPart and SpeechModelV4StreamTextPart."
editUrl: false
prev: false
next: false
sidebar:
  label: "SpeechTextPart"
  hidden: true
---

<a id="Vercel_AI_Operations_SpeechTextPart"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class SpeechTextPart
```


One piece of streamed input text. Consecutive parts are synthesized in order as one continuous script.
Maps to `SpeechTextPart` and `SpeechModelV4StreamTextPart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L32)

## Constructors

<a id="Vercel_AI_Operations_SpeechTextPart__ctor_"></a>
<a id="Vercel_AI_Operations_SpeechTextPart__ctor_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### SpeechTextPart(string, string?, JsonElement?)


```csharp
public SpeechTextPart(string text, string? voice = null, JsonElement? providerOptions = null)
```


Creates a text part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `voice` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerOptions` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L35)

## Properties

<a id="Vercel_AI_Operations_SpeechTextPart_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_SpeechTextPart_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; }
```


Additional provider-specific options for this part, keyed by provider name.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L52)

<a id="Vercel_AI_Operations_SpeechTextPart_Text_"></a>
<a id="Vercel_AI_Operations_SpeechTextPart_Text"></a>

### Text


```csharp
public string Text { get; }
```


The text to synthesize. Parts may split anywhere, including mid-word.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L43)

<a id="Vercel_AI_Operations_SpeechTextPart_Voice_"></a>
<a id="Vercel_AI_Operations_SpeechTextPart_Voice"></a>

### Voice


```csharp
public string? Voice { get; }
```


The voice that speaks this part (provider-specific, like the call's voice). Defaults to the call's voice.
Single-voice providers fail the stream when it differs from the voice of the call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L49)

## Operators

<a id="Vercel_AI_Operations_SpeechTextPart_op_Implicit_"></a>
<a id="Vercel_AI_Operations_SpeechTextPart_op_Implicit_System_String__Vercel_AI_Operations_SpeechTextPart"></a>

### implicit operator SpeechTextPart(string)


```csharp
public static implicit operator SpeechTextPart(string text)
```


Wraps a plain string as a text part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [SpeechTextPart](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L55)
