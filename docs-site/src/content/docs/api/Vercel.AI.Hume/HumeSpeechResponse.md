---
title: "HumeSpeechResponse"
description: "Hume speech response metadata."
editUrl: false
prev: false
next: false
sidebar:
  label: "HumeSpeechResponse"
  hidden: true
---

<a id="Vercel_AI_Hume_HumeSpeechResponse"></a>

Namespace: [Vercel.AI.Hume](/api/Vercel.AI.Hume/)


```csharp
public sealed class HumeSpeechResponse
```


Hume speech response metadata.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L126)

## Constructors

<a id="Vercel_AI_Hume_HumeSpeechResponse__ctor_"></a>
<a id="Vercel_AI_Hume_HumeSpeechResponse__ctor_System_DateTimeOffset_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `HumeSpeechResponse(DateTimeOffset, string, IReadOnlyDictionary<string, string>)`


```csharp
public HumeSpeechResponse(DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string> headers)
```


Creates response metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `timestamp` | [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L129)

## Properties

<a id="Vercel_AI_Hume_HumeSpeechResponse_Headers_"></a>
<a id="Vercel_AI_Hume_HumeSpeechResponse_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L143)

<a id="Vercel_AI_Hume_HumeSpeechResponse_ModelId_"></a>
<a id="Vercel_AI_Hume_HumeSpeechResponse_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id. Hume speech uses an empty id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L140)

<a id="Vercel_AI_Hume_HumeSpeechResponse_Timestamp_"></a>
<a id="Vercel_AI_Hume_HumeSpeechResponse_Timestamp"></a>

### Timestamp


```csharp
public DateTimeOffset Timestamp { get; }
```


Timestamp captured when the call started.

**Value:** [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L137)
