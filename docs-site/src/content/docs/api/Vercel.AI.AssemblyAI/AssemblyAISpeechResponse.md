---
title: "AssemblyAISpeechResponse"
description: "Metadata returned with synthesized AssemblyAI speech."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAISpeechResponse"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResponse"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public sealed class AssemblyAISpeechResponse
```


Metadata returned with synthesized AssemblyAI speech.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L256)

## Constructors

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResponse__ctor_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResponse__ctor_System_DateTimeOffset_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `AssemblyAISpeechResponse(DateTimeOffset, string, IReadOnlyDictionary<string, string>)`


```csharp
public AssemblyAISpeechResponse(DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string> headers)
```


Creates response metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `timestamp` | [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L259)

## Properties

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResponse_Headers_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResponse_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L277)

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResponse_ModelId_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResponse_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L274)

<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResponse_Timestamp_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAISpeechResponse_Timestamp"></a>

### Timestamp


```csharp
public DateTimeOffset Timestamp { get; }
```


Timestamp captured when the call started.

**Value:** [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L271)
