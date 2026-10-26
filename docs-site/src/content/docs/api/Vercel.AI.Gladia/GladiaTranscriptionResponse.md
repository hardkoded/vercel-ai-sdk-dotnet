---
title: "GladiaTranscriptionResponse"
description: "Gladia response metadata."
editUrl: false
prev: false
next: false
sidebar:
  label: "GladiaTranscriptionResponse"
  hidden: true
---

<a id="Vercel_AI_Gladia_GladiaTranscriptionResponse"></a>

Namespace: [Vercel.AI.Gladia](/api/Vercel.AI.Gladia/)


```csharp
public sealed class GladiaTranscriptionResponse
```


Gladia response metadata.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L110)

## Constructors

<a id="Vercel_AI_Gladia_GladiaTranscriptionResponse__ctor_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionResponse__ctor_System_DateTimeOffset_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `GladiaTranscriptionResponse(DateTimeOffset, string, IReadOnlyDictionary<string, string>)`


```csharp
public GladiaTranscriptionResponse(DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string> headers)
```


Creates response metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `timestamp` | [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L113)

## Properties

<a id="Vercel_AI_Gladia_GladiaTranscriptionResponse_Headers_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionResponse_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Headers from the result response.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L127)

<a id="Vercel_AI_Gladia_GladiaTranscriptionResponse_ModelId_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionResponse_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id. Gladia pre-recorded calls use `default`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L124)

<a id="Vercel_AI_Gladia_GladiaTranscriptionResponse_Timestamp_"></a>
<a id="Vercel_AI_Gladia_GladiaTranscriptionResponse_Timestamp"></a>

### Timestamp


```csharp
public DateTimeOffset Timestamp { get; }
```


Timestamp captured when the call started.

**Value:** [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L121)
