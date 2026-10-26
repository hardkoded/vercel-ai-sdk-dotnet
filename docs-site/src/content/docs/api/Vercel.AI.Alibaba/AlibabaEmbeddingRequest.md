---
title: "AlibabaEmbeddingRequest"
description: "Alibaba embedding request."
editUrl: false
prev: false
next: false
sidebar:
  label: "AlibabaEmbeddingRequest"
  hidden: true
---

<a id="Vercel_AI_Alibaba_AlibabaEmbeddingRequest"></a>

Namespace: [Vercel.AI.Alibaba](/api/Vercel.AI.Alibaba/)


```csharp
public sealed class AlibabaEmbeddingRequest
```


Alibaba embedding request.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L245)

## Constructors

<a id="Vercel_AI_Alibaba_AlibabaEmbeddingRequest__ctor_"></a>
<a id="Vercel_AI_Alibaba_AlibabaEmbeddingRequest__ctor_System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `AlibabaEmbeddingRequest(IReadOnlyList<string>)`


```csharp
public AlibabaEmbeddingRequest(IReadOnlyList<string> values)
```


Creates a request for &lt;code class="paramref"&gt;values&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `values` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L248)

## Properties

<a id="Vercel_AI_Alibaba_AlibabaEmbeddingRequest_Dimension_"></a>
<a id="Vercel_AI_Alibaba_AlibabaEmbeddingRequest_Dimension"></a>

### Dimension


```csharp
public int? Dimension { get; set; }
```


`parameters.dimension`, when set.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L260)

<a id="Vercel_AI_Alibaba_AlibabaEmbeddingRequest_Headers_"></a>
<a id="Vercel_AI_Alibaba_AlibabaEmbeddingRequest_Headers"></a>

### Headers


```csharp
public IDictionary<string, string>? Headers { get; set; }
```


Headers merged over the provider headers.

**Value:** [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L266)

<a id="Vercel_AI_Alibaba_AlibabaEmbeddingRequest_OutputType_"></a>
<a id="Vercel_AI_Alibaba_AlibabaEmbeddingRequest_OutputType"></a>

### OutputType


```csharp
public string? OutputType { get; set; }
```


`parameters.output_type`. `sparse` is rejected before the request.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L263)

<a id="Vercel_AI_Alibaba_AlibabaEmbeddingRequest_TextType_"></a>
<a id="Vercel_AI_Alibaba_AlibabaEmbeddingRequest_TextType"></a>

### TextType


```csharp
public string? TextType { get; set; }
```


`parameters.text_type`, when set.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L257)

<a id="Vercel_AI_Alibaba_AlibabaEmbeddingRequest_Values_"></a>
<a id="Vercel_AI_Alibaba_AlibabaEmbeddingRequest_Values"></a>

### Values


```csharp
public IReadOnlyList<string> Values { get; }
```


Texts to embed.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L254)
