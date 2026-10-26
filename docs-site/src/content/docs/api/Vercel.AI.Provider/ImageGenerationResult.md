---
title: "ImageGenerationResult"
description: "Image generation result."
editUrl: false
prev: false
next: false
sidebar:
  label: "ImageGenerationResult"
  hidden: true
---

<a id="Vercel_AI_Provider_ImageGenerationResult"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public class ImageGenerationResult
```


Image generation result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [LumaImageGeneration](/api/Vercel.AI.Luma/LumaImageGeneration/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L137)

## Constructors

<a id="Vercel_AI_Provider_ImageGenerationResult__ctor_"></a>
<a id="Vercel_AI_Provider_ImageGenerationResult__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_GeneratedImage__System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_CallWarning__Vercel_AI_Provider_ImageResponseInfo_Vercel_AI_Provider_LanguageModelUsage_System_Nullable_System_Text_Json_JsonElement__System_Nullable_System_Boolean__"></a>

### `ImageGenerationResult(IReadOnlyList<GeneratedImage>, IReadOnlyList<CallWarning>?, ImageResponseInfo?, LanguageModelUsage?, JsonElement?, bool?)`


```csharp
public ImageGenerationResult(IReadOnlyList<GeneratedImage> images, IReadOnlyList<CallWarning>? warnings = null, ImageResponseInfo? response = null, LanguageModelUsage? usage = null, JsonElement? providerMetadata = null, bool? isRetryable = null)
```


Creates an image result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `images` | [IReadOnlyList&lt;GeneratedImage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `warnings` | [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `response` | [ImageResponseInfo](/api/Vercel.AI.Provider/ImageResponseInfo/) |  |
| `usage` | [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `isRetryable` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L140)

## Properties

<a id="Vercel_AI_Provider_ImageGenerationResult_CallWarnings_"></a>
<a id="Vercel_AI_Provider_ImageGenerationResult_CallWarnings"></a>

### CallWarnings


```csharp
public IReadOnlyList<CallWarning> CallWarnings { get; }
```


Warnings of this call. Maps to `warnings`.

**Value:** [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L154)

<a id="Vercel_AI_Provider_ImageGenerationResult_Images_"></a>
<a id="Vercel_AI_Provider_ImageGenerationResult_Images"></a>

### Images


```csharp
public IReadOnlyList<GeneratedImage> Images { get; }
```


Generated images.

**Value:** [IReadOnlyList&lt;GeneratedImage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L151)

<a id="Vercel_AI_Provider_ImageGenerationResult_IsRetryable_"></a>
<a id="Vercel_AI_Provider_ImageGenerationResult_IsRetryable"></a>

### IsRetryable


```csharp
public bool? IsRetryable { get; }
```


&lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool"&gt;false&lt;/a&gt; when an empty result must not be retried. Maps to `isRetryable`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L166)

<a id="Vercel_AI_Provider_ImageGenerationResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_ImageGenerationResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata of this call, keyed by provider name. Maps to `providerMetadata`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L163)

<a id="Vercel_AI_Provider_ImageGenerationResult_Response_"></a>
<a id="Vercel_AI_Provider_ImageGenerationResult_Response"></a>

### Response


```csharp
public ImageResponseInfo? Response { get; }
```


Response metadata of this call, when the model reports it.

**Value:** [ImageResponseInfo](/api/Vercel.AI.Provider/ImageResponseInfo/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L157)

<a id="Vercel_AI_Provider_ImageGenerationResult_Usage_"></a>
<a id="Vercel_AI_Provider_ImageGenerationResult_Usage"></a>

### Usage


```csharp
public LanguageModelUsage? Usage { get; }
```


Token usage, when the response included it.

**Value:** [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L160)
