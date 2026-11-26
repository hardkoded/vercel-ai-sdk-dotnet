---
title: "QuiverAIModelCatalog"
description: "QuiverAI model catalog. Maps to quiverai-model-catalog.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "QuiverAIModelCatalog"
  hidden: true
---

<a id="Vercel_AI_QuiverAI_QuiverAIModelCatalog"></a>

Namespace: [Vercel.AI.QuiverAI](/api/Vercel.AI.QuiverAI/)


```csharp
public static class QuiverAIModelCatalog
```


QuiverAI model catalog. Maps to `quiverai-model-catalog.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L127)

## Properties

<a id="Vercel_AI_QuiverAI_QuiverAIModelCatalog_ImageMaxImagesPerCall_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIModelCatalog_ImageMaxImagesPerCall"></a>

### ImageMaxImagesPerCall


```csharp
public static int ImageMaxImagesPerCall { get; }
```


Maps to `quiveraiRequestDefaults.image.maxImagesPerCall`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L158)

<a id="Vercel_AI_QuiverAI_QuiverAIModelCatalog_ImageModelIds_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIModelCatalog_ImageModelIds"></a>

### ImageModelIds


```csharp
public static IReadOnlyList<string> ImageModelIds { get; }
```


Maps to `quiveraiImageModelIds`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L150)

<a id="Vercel_AI_QuiverAI_QuiverAIModelCatalog_LanguageModelIds_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIModelCatalog_LanguageModelIds"></a>

### LanguageModelIds


```csharp
public static IReadOnlyList<string> LanguageModelIds { get; }
```


Maps to `quiveraiLanguageModelIds`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L147)

<a id="Vercel_AI_QuiverAI_QuiverAIModelCatalog_LanguageRequestDefaults_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIModelCatalog_LanguageRequestDefaults"></a>

### LanguageRequestDefaults


```csharp
public static QuiverAILanguageRequestConfig LanguageRequestDefaults { get; }
```


Maps to `quiveraiRequestDefaults.language`.

**Value:** [QuiverAILanguageRequestConfig](/api/Vercel.AI.QuiverAI/QuiverAILanguageRequestConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L154)

## Methods

<a id="Vercel_AI_QuiverAI_QuiverAIModelCatalog_GetQuiverAIImageModelEntry_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIModelCatalog_GetQuiverAIImageModelEntry_System_String_"></a>

### GetQuiverAIImageModelEntry(string)


```csharp
public static QuiverAIImageModelEntry GetQuiverAIImageModelEntry(string modelId)
```


Returns the entry of an image model. An unknown model id gets `unknown` option support. Maps to `getQuiverAIImageModelEntry`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [QuiverAIImageModelEntry](/api/Vercel.AI.QuiverAI/QuiverAIImageModelEntry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L209)

<a id="Vercel_AI_QuiverAI_QuiverAIModelCatalog_GetQuiverAILanguageModelEntry_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIModelCatalog_GetQuiverAILanguageModelEntry_System_String_"></a>

### GetQuiverAILanguageModelEntry(string)


```csharp
public static QuiverAILanguageModelEntry GetQuiverAILanguageModelEntry(string modelId)
```


Returns the entry of a language model. An unknown model id gets `unknown` option support. Maps to `getQuiverAILanguageModelEntry`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [QuiverAILanguageModelEntry](/api/Vercel.AI.QuiverAI/QuiverAILanguageModelEntry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L168)
