---
title: "AssemblyAISpeechRequest"
description: "One AssemblyAI text-to-speech call."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAISpeechRequest"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechRequest"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public sealed class AssemblyAISpeechRequest
```


One AssemblyAI text-to-speech call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L223)

## Constructors

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechRequest__ctor_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechRequest__ctor_System_String_"></a>

### AssemblyAISpeechRequest(string)


```csharp
public AssemblyAISpeechRequest(string text)
```


Creates a speech request.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L226)

## Properties

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechRequest_Headers_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechRequest_Headers"></a>

### Headers


```csharp
public IDictionary<string, string>? Headers { get; set; }
```


Headers merged over the provider headers for this call.

**Value:** [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L253)

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechRequest_Instructions_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechRequest_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; set; }
```


Ignored. AssemblyAI does not accept instructions; a warning is returned.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L241)

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechRequest_Language_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechRequest_Language"></a>

### Language


```csharp
public string? Language { get; set; }
```


ISO 639-1 code (`de`, `en`, `es`, `fr`, `it`, `pt`) or the full language name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L247)

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechRequest_OutputFormat_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechRequest_OutputFormat"></a>

### OutputFormat


```csharp
public string? OutputFormat { get; set; }
```


Output format: `mp3` (default), `wav`, `pcm`, `mulaw`, or `alaw`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L238)

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechRequest_ProviderOptions_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechRequest_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Provider options. The `assemblyai` object is read: `encoding`, `container`, `sampleRate`, `bitRate`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L250)

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechRequest_Speed_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechRequest_Speed"></a>

### Speed


```csharp
public double? Speed { get; set; }
```


Speaking rate. Only the default speed is supported; another value returns a warning.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L244)

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechRequest_Text_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechRequest_Text"></a>

### Text


```csharp
public string Text { get; }
```


Text to speak.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L232)

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechRequest_Voice_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechRequest_Voice"></a>

### Voice


```csharp
public string? Voice { get; set; }
```


Voice id, sent as `voice_id`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L235)
