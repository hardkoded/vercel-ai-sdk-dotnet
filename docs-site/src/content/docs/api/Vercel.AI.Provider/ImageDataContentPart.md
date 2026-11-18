---
title: "ImageDataContentPart"
description: "A deprecated image-data part inside a tool result. Use FileContentPart with inline data instead. Maps to { type: 'image-data', data, mediaType, providerOptio..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ImageDataContentPart"
  hidden: true
---

<a id="Vercel_AI_Provider_ImageDataContentPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ImageDataContentPart : UserContentPart
```


A deprecated `image-data` part inside a tool result. Use [FileContentPart](/api/Vercel.AI.Provider/FileContentPart/) with inline data instead.
Maps to `{ type: 'image-data', data, mediaType, providerOptions }` in the tool result content.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UserContentPart](/api/Vercel.AI.Provider/UserContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1048)

## Constructors

<a id="Vercel_AI_Provider_ImageDataContentPart__ctor_"></a>
<a id="Vercel_AI_Provider_ImageDataContentPart__ctor_System_String_System_String_"></a>

### ImageDataContentPart(string, string)


```csharp
public ImageDataContentPart(string data, string mediaType)
```


Creates an image-data part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1051)

## Properties

<a id="Vercel_AI_Provider_ImageDataContentPart_Data_"></a>
<a id="Vercel_AI_Provider_ImageDataContentPart_Data"></a>

### Data


```csharp
public string Data { get; }
```


Base64-encoded image data.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1059)

<a id="Vercel_AI_Provider_ImageDataContentPart_MediaType_"></a>
<a id="Vercel_AI_Provider_ImageDataContentPart_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1062)

<a id="Vercel_AI_Provider_ImageDataContentPart_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_ImageDataContentPart_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
```


Provider-specific options for this part.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1065)
