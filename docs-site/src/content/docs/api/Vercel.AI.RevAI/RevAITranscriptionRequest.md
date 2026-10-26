---
title: "RevAITranscriptionRequest"
description: "Extra fields and headers for one Rev.ai transcription."
editUrl: false
prev: false
next: false
sidebar:
  label: "RevAITranscriptionRequest"
  hidden: true
---

<a id="Vercel_AI_RevAI_RevAITranscriptionRequest"></a>

Namespace: [Vercel.AI.RevAI](/api/Vercel.AI.RevAI/)


```csharp
public sealed class RevAITranscriptionRequest
```


Extra fields and headers for one Rev.ai transcription.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L80)

## Properties

<a id="Vercel_AI_RevAI_RevAITranscriptionRequest_Config_"></a>
<a id="Vercel_AI_RevAI_RevAITranscriptionRequest_Config"></a>

### Config


```csharp
public JsonObject? Config { get; set; }
```


Additional JSON merged into the multipart `config` object.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L86)

<a id="Vercel_AI_RevAI_RevAITranscriptionRequest_Headers_"></a>
<a id="Vercel_AI_RevAI_RevAITranscriptionRequest_Headers"></a>

### Headers


```csharp
public IDictionary<string, string>? Headers { get; set; }
```


Headers merged over the provider headers for this call.

**Value:** [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L83)
