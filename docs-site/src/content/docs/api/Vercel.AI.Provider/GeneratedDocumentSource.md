---
title: "GeneratedDocumentSource"
description: "A document source, such as a retrieved file or a cited document."
editUrl: false
prev: false
next: false
sidebar:
  label: "GeneratedDocumentSource"
  hidden: true
---

<a id="Vercel_AI_Provider_GeneratedDocumentSource"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class GeneratedDocumentSource : GeneratedContent
```


A document source, such as a retrieved file or a cited document.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L328)

## Constructors

<a id="Vercel_AI_Provider_GeneratedDocumentSource__ctor_"></a>
<a id="Vercel_AI_Provider_GeneratedDocumentSource__ctor_System_String_System_String_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### GeneratedDocumentSource(string, string, string, string?, JsonElement?)


```csharp
public GeneratedDocumentSource(string id, string mediaType, string title, string? filename = null, JsonElement? providerMetadata = null)
```


Creates a document source.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `title` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `filename` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L331)

## Properties

<a id="Vercel_AI_Provider_GeneratedDocumentSource_Filename_"></a>
<a id="Vercel_AI_Provider_GeneratedDocumentSource_Filename"></a>

### Filename


```csharp
public string? Filename { get; }
```


Optional file name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L351)

<a id="Vercel_AI_Provider_GeneratedDocumentSource_Id_"></a>
<a id="Vercel_AI_Provider_GeneratedDocumentSource_Id"></a>

### Id


```csharp
public string Id { get; }
```


Source id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L342)

<a id="Vercel_AI_Provider_GeneratedDocumentSource_MediaType_"></a>
<a id="Vercel_AI_Provider_GeneratedDocumentSource_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type of the document.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L345)

<a id="Vercel_AI_Provider_GeneratedDocumentSource_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_GeneratedDocumentSource_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider-specific source metadata, such as a file id or a page range.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L354)

<a id="Vercel_AI_Provider_GeneratedDocumentSource_Title_"></a>
<a id="Vercel_AI_Provider_GeneratedDocumentSource_Title"></a>

### Title


```csharp
public string Title { get; }
```


Document title.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L348)
