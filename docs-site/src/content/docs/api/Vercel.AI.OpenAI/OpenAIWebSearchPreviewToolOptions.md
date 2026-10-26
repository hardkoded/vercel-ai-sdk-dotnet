---
title: "OpenAIWebSearchPreviewToolOptions"
description: "Options of WebSearchPreview(OpenAIWebSearchPreviewToolOptions)."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIWebSearchPreviewToolOptions"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIWebSearchPreviewToolOptions"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIWebSearchPreviewToolOptions
```


Options of [WebSearchPreview(OpenAIWebSearchPreviewToolOptions)](/api/Vercel.AI.OpenAI/OpenAITools/#Vercel_AI_OpenAI_OpenAITools_WebSearchPreview_Vercel_AI_OpenAI_OpenAIWebSearchPreviewToolOptions_).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L233)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIWebSearchPreviewToolOptions_SearchContextSize_"></a>
<a id="Vercel_AI_OpenAI_OpenAIWebSearchPreviewToolOptions_SearchContextSize"></a>

### SearchContextSize


```csharp
public string? SearchContextSize { get; set; }
```


Search context size: `low`, `medium`, or `high`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L236)

<a id="Vercel_AI_OpenAI_OpenAIWebSearchPreviewToolOptions_UserLocation_"></a>
<a id="Vercel_AI_OpenAI_OpenAIWebSearchPreviewToolOptions_UserLocation"></a>

### UserLocation


```csharp
public OpenAIApproximateLocation? UserLocation { get; set; }
```


Approximate user location.

**Value:** [OpenAIApproximateLocation](/api/Vercel.AI.OpenAI/OpenAIApproximateLocation/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L239)
