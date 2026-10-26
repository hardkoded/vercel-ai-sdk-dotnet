---
title: "GoogleModelPath"
description: "Builds the resource path Gemini expects for a model id."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleModelPath"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleModelPath"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleModelPath
```


Builds the resource path Gemini expects for a model id.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleModelPath.cs#L8)

## Methods

<a id="Vercel_AI_Google_GoogleModelPath_Get_"></a>
<a id="Vercel_AI_Google_GoogleModelPath_Get_System_String_"></a>

### Get(string)


```csharp
public static string Get(string modelId)
```


Returns &lt;code class="paramref"&gt;modelId&lt;/code&gt; when it already contains a slash, such as `models/x` or `tunedModels/x`.
Otherwise prefixes `models/`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleModelPath.cs#L14)
