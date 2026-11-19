---
title: "ImageFileReferenceContentPart"
description: "A deprecated image-file-reference part inside a tool result. Use JsonElement%7d) with the image media type instead. Maps to { type: 'image-file-reference', p..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ImageFileReferenceContentPart"
  hidden: true
---

<a id="Vercel_AI_Provider_ImageFileReferenceContentPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ImageFileReferenceContentPart : UserContentPart
```


A deprecated `image-file-reference` part inside a tool result. Use `JsonElement%7d)` with the `image` media type instead.
Maps to `{ type: 'image-file-reference', providerReference, providerOptions }` in the tool result content.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UserContentPart](/api/Vercel.AI.Provider/UserContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1087)

## Constructors

<a id="Vercel_AI_Provider_ImageFileReferenceContentPart__ctor_"></a>
<a id="Vercel_AI_Provider_ImageFileReferenceContentPart__ctor_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `ImageFileReferenceContentPart(IReadOnlyDictionary<string, string>)`


```csharp
public ImageFileReferenceContentPart(IReadOnlyDictionary<string, string> providerReference)
```


Creates an image-file-reference part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerReference` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1090)

## Properties

<a id="Vercel_AI_Provider_ImageFileReferenceContentPart_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_ImageFileReferenceContentPart_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
```


Provider-specific options for this part.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1101)

<a id="Vercel_AI_Provider_ImageFileReferenceContentPart_ProviderReference_"></a>
<a id="Vercel_AI_Provider_ImageFileReferenceContentPart_ProviderReference"></a>

### ProviderReference


```csharp
public IReadOnlyDictionary<string, string> ProviderReference { get; }
```


Map from provider name to the provider's file id.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1098)
