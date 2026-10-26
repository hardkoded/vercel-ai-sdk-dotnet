---
title: "EmbedOptions"
description: "Embedding options."
editUrl: false
prev: false
next: false
sidebar:
  label: "EmbedOptions"
  hidden: true
---

<a id="Vercel_AI_EmbedOptions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class EmbedOptions
```


Embedding options.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L309)

## Properties

<a id="Vercel_AI_EmbedOptions_Dimensions_"></a>
<a id="Vercel_AI_EmbedOptions_Dimensions"></a>

### Dimensions


```csharp
public int? Dimensions { get; set; }
```


Requested output dimensions. Must be a positive integer. Support depends on the model.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L321)

<a id="Vercel_AI_EmbedOptions_Headers_"></a>
<a id="Vercel_AI_EmbedOptions_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; set; }
```


Headers for this call. They override the provider headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L327)

<a id="Vercel_AI_EmbedOptions_Model_"></a>
<a id="Vercel_AI_EmbedOptions_Model"></a>

### Model


```csharp
public IEmbeddingModel? Model { get; set; }
```


Explicit embedding model.

**Value:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L312)

<a id="Vercel_AI_EmbedOptions_ModelId_"></a>
<a id="Vercel_AI_EmbedOptions_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Gateway embedding model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L315)

<a id="Vercel_AI_EmbedOptions_ProviderOptions_"></a>
<a id="Vercel_AI_EmbedOptions_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Provider options keyed by provider id.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L324)

<a id="Vercel_AI_EmbedOptions_Values_"></a>
<a id="Vercel_AI_EmbedOptions_Values"></a>

### Values


```csharp
public IReadOnlyList<string> Values { get; set; }
```


Values to embed.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L318)
