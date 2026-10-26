---
title: "DocumentSourceStreamPart"
description: "A retrieved document, such as a file a search tool returned."
editUrl: false
prev: false
next: false
sidebar:
  label: "DocumentSourceStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_DocumentSourceStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class DocumentSourceStreamPart : LanguageModelStreamPart
```


A retrieved document, such as a file a search tool returned.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1906)

## Constructors

<a id="Vercel_AI_Provider_DocumentSourceStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_DocumentSourceStreamPart__ctor_System_String_System_String_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### DocumentSourceStreamPart(string, string, string, string?, JsonElement?)


```csharp
public DocumentSourceStreamPart(string id, string mediaType, string title, string? filename = null, JsonElement? providerMetadata = null)
```


Creates a document source part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `title` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `filename` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1909)

## Properties

<a id="Vercel_AI_Provider_DocumentSourceStreamPart_Filename_"></a>
<a id="Vercel_AI_Provider_DocumentSourceStreamPart_Filename"></a>

### Filename


```csharp
public string? Filename { get; }
```


Optional file name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1929)

<a id="Vercel_AI_Provider_DocumentSourceStreamPart_Id_"></a>
<a id="Vercel_AI_Provider_DocumentSourceStreamPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Source id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1920)

<a id="Vercel_AI_Provider_DocumentSourceStreamPart_MediaType_"></a>
<a id="Vercel_AI_Provider_DocumentSourceStreamPart_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type of the document.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1923)

<a id="Vercel_AI_Provider_DocumentSourceStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_DocumentSourceStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider-specific source metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1932)

<a id="Vercel_AI_Provider_DocumentSourceStreamPart_Title_"></a>
<a id="Vercel_AI_Provider_DocumentSourceStreamPart_Title"></a>

### Title


```csharp
public string Title { get; }
```


Document title.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1926)
