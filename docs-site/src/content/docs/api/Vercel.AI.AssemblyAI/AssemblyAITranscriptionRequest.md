---
title: "AssemblyAITranscriptionRequest"
description: "Provider options and headers for one AssemblyAI transcription."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAITranscriptionRequest"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionRequest"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public sealed class AssemblyAITranscriptionRequest
```


Provider options and headers for one AssemblyAI transcription.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L81)

## Properties

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionRequest_Headers_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionRequest_Headers"></a>

### Headers


```csharp
public IDictionary<string, string>? Headers { get; set; }
```


Headers merged over the provider headers for this call.

**Value:** [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L84)

<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionRequest_ProviderOptions_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAITranscriptionRequest_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Provider options. The `assemblyai` object is read, with camelCase keys.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L87)
