---
title: "GladiaTranscriptionRequest"
description: "Headers and extra pre-recorded fields for one Gladia transcription."
editUrl: false
prev: false
next: false
sidebar:
  label: "GladiaTranscriptionRequest"
  hidden: true
---

<a id="Vercel_AI_Gladia_GladiaTranscriptionRequest"></a>

Namespace: [Vercel.AI.Gladia](/api/Vercel.AI.Gladia/)


```csharp
public sealed class GladiaTranscriptionRequest
```


Headers and extra pre-recorded fields for one Gladia transcription.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L79)

## Properties

<a id="Vercel_AI_Gladia_GladiaTranscriptionRequest_Body_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionRequest_Body"></a>

### Body


```csharp
public JsonObject? Body { get; set; }
```


Extra JSON fields sent with `audio_url`.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L85)

<a id="Vercel_AI_Gladia_GladiaTranscriptionRequest_Headers_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionRequest_Headers"></a>

### Headers


```csharp
public IDictionary<string, string>? Headers { get; set; }
```


Headers merged over the provider headers.

**Value:** [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L82)
