---
title: "OpenAISpeechResult"
description: "Speech audio plus the response metadata recorded for the call."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAISpeechResult"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAISpeechResult"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAISpeechResult
```


Speech audio plus the response metadata recorded for the call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L47)

## Properties

<a id="Vercel_AI_OpenAI_OpenAISpeechResult_Audio_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechResult_Audio"></a>

### Audio


```csharp
public byte[] Audio { get; }
```


Audio bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L60)

<a id="Vercel_AI_OpenAI_OpenAISpeechResult_Headers_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechResult_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


HTTP response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L75)

<a id="Vercel_AI_OpenAI_OpenAISpeechResult_MediaType_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechResult_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type derived from the request format.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L63)

<a id="Vercel_AI_OpenAI_OpenAISpeechResult_ModelId_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechResult_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id used for the call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L69)

<a id="Vercel_AI_OpenAI_OpenAISpeechResult_Timestamp_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechResult_Timestamp"></a>

### Timestamp


```csharp
public DateTimeOffset Timestamp { get; }
```


Timestamp recorded when the response was read.

**Value:** [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L72)

<a id="Vercel_AI_OpenAI_OpenAISpeechResult_Warnings_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OpenAICallWarning> Warnings { get; }
```


Warnings produced while preparing the request.

**Value:** [IReadOnlyList&lt;OpenAICallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L66)
