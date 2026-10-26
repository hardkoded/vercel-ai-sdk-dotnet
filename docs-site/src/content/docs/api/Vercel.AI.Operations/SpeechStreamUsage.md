---
title: "SpeechStreamUsage"
description: "Usage of a speech stream call. Every field is optional because providers bill speech in different units."
editUrl: false
prev: false
next: false
sidebar:
  label: "SpeechStreamUsage"
  hidden: true
---

<a id="Vercel_AI_Operations_SpeechStreamUsage"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class SpeechStreamUsage
```


Usage of a speech stream call. Every field is optional because providers bill speech in different units.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L115)

## Properties

<a id="Vercel_AI_Operations_SpeechStreamUsage_InputCharacters_"></a>
<a id="Vercel_AI_Operations_SpeechStreamUsage_InputCharacters"></a>

### InputCharacters


```csharp
public int? InputCharacters { get; set; }
```


Number of input text characters (Unicode code points) sent for synthesis, if known.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L118)

<a id="Vercel_AI_Operations_SpeechStreamUsage_InputTokens_"></a>
<a id="Vercel_AI_Operations_SpeechStreamUsage_InputTokens"></a>

### InputTokens


```csharp
public int? InputTokens { get; set; }
```


Number of input text tokens, if reported.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L121)

<a id="Vercel_AI_Operations_SpeechStreamUsage_OutputTokens_"></a>
<a id="Vercel_AI_Operations_SpeechStreamUsage_OutputTokens"></a>

### OutputTokens


```csharp
public int? OutputTokens { get; set; }
```


Number of output audio tokens, if reported.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L124)

<a id="Vercel_AI_Operations_SpeechStreamUsage_Raw_"></a>
<a id="Vercel_AI_Operations_SpeechStreamUsage_Raw"></a>

### Raw


```csharp
public JsonElement? Raw { get; set; }
```


Raw usage information from the provider, in the shape the provider returns it.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L127)
