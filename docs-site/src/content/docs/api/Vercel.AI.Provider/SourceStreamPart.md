---
title: "SourceStreamPart"
description: "A cited URL."
editUrl: false
prev: false
next: false
sidebar:
  label: "SourceStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_SourceStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class SourceStreamPart : LanguageModelStreamPart
```


A cited URL.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1880)

## Constructors

<a id="Vercel_AI_Provider_SourceStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_SourceStreamPart__ctor_System_String_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### SourceStreamPart(string, string, string?, JsonElement?)


```csharp
public SourceStreamPart(string id, string url, string? title, JsonElement? providerMetadata = null)
```


Creates a source part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `title` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1883)

## Properties

<a id="Vercel_AI_Provider_SourceStreamPart_Id_"></a>
<a id="Vercel_AI_Provider_SourceStreamPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Source id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1893)

<a id="Vercel_AI_Provider_SourceStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_SourceStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider-specific source metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1902)

<a id="Vercel_AI_Provider_SourceStreamPart_Title_"></a>
<a id="Vercel_AI_Provider_SourceStreamPart_Title"></a>

### Title


```csharp
public string? Title { get; }
```


Optional title.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1899)

<a id="Vercel_AI_Provider_SourceStreamPart_Url_"></a>
<a id="Vercel_AI_Provider_SourceStreamPart_Url"></a>

### Url


```csharp
public string Url { get; }
```


Source URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1896)
