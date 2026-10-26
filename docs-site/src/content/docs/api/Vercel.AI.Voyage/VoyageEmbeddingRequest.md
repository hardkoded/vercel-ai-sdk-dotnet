---
title: "VoyageEmbeddingRequest"
description: "Optional Voyage embedding fields."
editUrl: false
prev: false
next: false
sidebar:
  label: "VoyageEmbeddingRequest"
  hidden: true
---

<a id="Vercel_AI_Voyage_VoyageEmbeddingRequest"></a>

Namespace: [Vercel.AI.Voyage](/api/Vercel.AI.Voyage/)


```csharp
public sealed class VoyageEmbeddingRequest
```


Optional Voyage embedding fields.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L15)

## Constructors

<a id="Vercel_AI_Voyage_VoyageEmbeddingRequest__ctor_"></a>
<a id="Vercel_AI_Voyage_VoyageEmbeddingRequest__ctor_System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `VoyageEmbeddingRequest(IReadOnlyList<string>)`


```csharp
public VoyageEmbeddingRequest(IReadOnlyList<string> values)
```


Creates a request for &lt;code class="paramref"&gt;values&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `values` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L18)

## Properties

<a id="Vercel_AI_Voyage_VoyageEmbeddingRequest_Headers_"></a>
<a id="Vercel_AI_Voyage_VoyageEmbeddingRequest_Headers"></a>

### Headers


```csharp
public IDictionary<string, string>? Headers { get; set; }
```


Headers merged over the provider headers.

**Value:** [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L39)

<a id="Vercel_AI_Voyage_VoyageEmbeddingRequest_InputType_"></a>
<a id="Vercel_AI_Voyage_VoyageEmbeddingRequest_InputType"></a>

### InputType


```csharp
public string? InputType { get; set; }
```


`input_type`, when set.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L27)

<a id="Vercel_AI_Voyage_VoyageEmbeddingRequest_OutputDimension_"></a>
<a id="Vercel_AI_Voyage_VoyageEmbeddingRequest_OutputDimension"></a>

### OutputDimension


```csharp
public int? OutputDimension { get; set; }
```


`output_dimension`, when set.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L30)

<a id="Vercel_AI_Voyage_VoyageEmbeddingRequest_OutputDtype_"></a>
<a id="Vercel_AI_Voyage_VoyageEmbeddingRequest_OutputDtype"></a>

### OutputDtype


```csharp
public string? OutputDtype { get; set; }
```


`output_dtype`, when set.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L36)

<a id="Vercel_AI_Voyage_VoyageEmbeddingRequest_Truncation_"></a>
<a id="Vercel_AI_Voyage_VoyageEmbeddingRequest_Truncation"></a>

### Truncation


```csharp
public bool? Truncation { get; set; }
```


`truncation`, when set.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L33)

<a id="Vercel_AI_Voyage_VoyageEmbeddingRequest_Values_"></a>
<a id="Vercel_AI_Voyage_VoyageEmbeddingRequest_Values"></a>

### Values


```csharp
public IReadOnlyList<string> Values { get; }
```


Texts to embed.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L24)
