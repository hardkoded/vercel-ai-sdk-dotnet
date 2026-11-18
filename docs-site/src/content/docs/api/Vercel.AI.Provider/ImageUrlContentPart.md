---
title: "ImageUrlContentPart"
description: "A deprecated image-url part inside a tool result. Use FileContentPart with the image media type and a URL instead. Maps to { type: 'image-url', url, provider..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ImageUrlContentPart"
  hidden: true
---

<a id="Vercel_AI_Provider_ImageUrlContentPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ImageUrlContentPart : UserContentPart
```


A deprecated `image-url` part inside a tool result. Use [FileContentPart](/api/Vercel.AI.Provider/FileContentPart/) with the `image` media type and a URL instead.
Maps to `{ type: 'image-url', url, providerOptions }` in the tool result content.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UserContentPart](/api/Vercel.AI.Provider/UserContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L949)

## Constructors

<a id="Vercel_AI_Provider_ImageUrlContentPart__ctor_"></a>
<a id="Vercel_AI_Provider_ImageUrlContentPart__ctor_System_String_"></a>

### ImageUrlContentPart(string)


```csharp
public ImageUrlContentPart(string url)
```


Creates an image-url part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L952)

## Properties

<a id="Vercel_AI_Provider_ImageUrlContentPart_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_ImageUrlContentPart_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
```


Provider-specific options for this part.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L962)

<a id="Vercel_AI_Provider_ImageUrlContentPart_Url_"></a>
<a id="Vercel_AI_Provider_ImageUrlContentPart_Url"></a>

### Url


```csharp
public string Url { get; }
```


Remote URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L959)
