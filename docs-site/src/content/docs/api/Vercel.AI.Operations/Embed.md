---
title: "Embed"
description: "Embeds one value. Maps to embed."
editUrl: false
prev: false
next: false
sidebar:
  label: "Embed"
  hidden: true
---

<a id="Vercel_AI_Operations_Embed"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class Embed
```


Embeds one value. Maps to `embed`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L403)

## Methods

<a id="Vercel_AI_Operations_Embed_EmbedAsync_"></a>
<a id="Vercel_AI_Operations_Embed_EmbedAsync_Vercel_AI_Operations_EmbedRequest_System_Threading_CancellationToken_"></a>

### EmbedAsync(EmbedRequest, CancellationToken)


```csharp
public static Task<EmbedResult> EmbedAsync(EmbedRequest request, CancellationToken cancellationToken = default)
```


Embeds [Value](/api/Vercel.AI.Operations/EmbedRequest/#Vercel_AI_Operations_EmbedRequest_Value).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [EmbedRequest](/api/Vercel.AI.Operations/EmbedRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;EmbedResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L406)

<a id="Vercel_AI_Operations_Embed_ValidateEmbeddingDimensions_"></a>
<a id="Vercel_AI_Operations_Embed_ValidateEmbeddingDimensions_System_Nullable_System_Double__"></a>

### ValidateEmbeddingDimensions(double?)


```csharp
public static void ValidateEmbeddingDimensions(double? dimensions)
```


Throws when &lt;code class="paramref"&gt;dimensions&lt;/code&gt; is set and is not a positive integer.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `dimensions` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L468)
