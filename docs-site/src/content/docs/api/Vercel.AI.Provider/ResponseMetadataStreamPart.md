---
title: "ResponseMetadataStreamPart"
description: "Identifies the provider response."
editUrl: false
prev: false
next: false
sidebar:
  label: "ResponseMetadataStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_ResponseMetadataStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ResponseMetadataStreamPart : LanguageModelStreamPart
```


Identifies the provider response.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2090)

## Constructors

<a id="Vercel_AI_Provider_ResponseMetadataStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_ResponseMetadataStreamPart__ctor_System_String_System_String_System_Nullable_System_DateTimeOffset__"></a>

### ResponseMetadataStreamPart(string?, string?, DateTimeOffset?)


```csharp
public ResponseMetadataStreamPart(string? id, string? modelId, DateTimeOffset? timestamp)
```


Creates a response-metadata part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `timestamp` | [DateTimeOffset?](https://learn.microsoft.com/dotnet/api/system.datetimeoffset) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2093)

## Properties

<a id="Vercel_AI_Provider_ResponseMetadataStreamPart_Id_"></a>
<a id="Vercel_AI_Provider_ResponseMetadataStreamPart_Id"></a>

### Id


```csharp
public string? Id { get; }
```


Provider response id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2102)

<a id="Vercel_AI_Provider_ResponseMetadataStreamPart_ModelId_"></a>
<a id="Vercel_AI_Provider_ResponseMetadataStreamPart_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; }
```


Provider model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2105)

<a id="Vercel_AI_Provider_ResponseMetadataStreamPart_Timestamp_"></a>
<a id="Vercel_AI_Provider_ResponseMetadataStreamPart_Timestamp"></a>

### Timestamp


```csharp
public DateTimeOffset? Timestamp { get; }
```


Provider timestamp.

**Value:** [DateTimeOffset?](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2108)
