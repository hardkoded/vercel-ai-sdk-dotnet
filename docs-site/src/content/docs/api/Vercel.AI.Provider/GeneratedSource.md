---
title: "GeneratedSource"
description: "A URL source cited by the model."
editUrl: false
prev: false
next: false
sidebar:
  label: "GeneratedSource"
  hidden: true
---

<a id="Vercel_AI_Provider_GeneratedSource"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class GeneratedSource : GeneratedContent
```


A URL source cited by the model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L302)

## Constructors

<a id="Vercel_AI_Provider_GeneratedSource__ctor_"></a>
<a id="Vercel_AI_Provider_GeneratedSource__ctor_System_String_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### GeneratedSource(string, string, string?, JsonElement?)


```csharp
public GeneratedSource(string id, string url, string? title, JsonElement? providerMetadata = null)
```


Creates a source.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `title` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L305)

## Properties

<a id="Vercel_AI_Provider_GeneratedSource_Id_"></a>
<a id="Vercel_AI_Provider_GeneratedSource_Id"></a>

### Id


```csharp
public string Id { get; }
```


Source id. Search results keep the provider result id so citations can be correlated.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L315)

<a id="Vercel_AI_Provider_GeneratedSource_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_GeneratedSource_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider-specific source metadata, such as a snippet or search-result id.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L324)

<a id="Vercel_AI_Provider_GeneratedSource_Title_"></a>
<a id="Vercel_AI_Provider_GeneratedSource_Title"></a>

### Title


```csharp
public string? Title { get; }
```


Optional title.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L321)

<a id="Vercel_AI_Provider_GeneratedSource_Url_"></a>
<a id="Vercel_AI_Provider_GeneratedSource_Url"></a>

### Url


```csharp
public string Url { get; }
```


Source URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L318)
