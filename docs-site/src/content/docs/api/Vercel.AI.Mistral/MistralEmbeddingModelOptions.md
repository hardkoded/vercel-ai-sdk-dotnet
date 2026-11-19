---
title: "MistralEmbeddingModelOptions"
description: "Mistral embedding provider options. Maps to MistralEmbeddingModelOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralEmbeddingModelOptions"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralEmbeddingModelOptions"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralEmbeddingModelOptions
```


Mistral embedding provider options. Maps to `MistralEmbeddingModelOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralEmbeddingModelOptions.cs#L13)

## Properties

<a id="Vercel_AI_Mistral_MistralEmbeddingModelOptions_Metadata_"></a>
<a id="Vercel_AI_Mistral_MistralEmbeddingModelOptions_Metadata"></a>

### Metadata


```csharp
public IReadOnlyDictionary<string, JsonNode?>? Metadata { get; init; }
```


Additional metadata to attach to the embedding request.

**Value:** [IReadOnlyDictionary&lt;string, JsonNode&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralEmbeddingModelOptions.cs#L16)

<a id="Vercel_AI_Mistral_MistralEmbeddingModelOptions_OutputDimension_"></a>
<a id="Vercel_AI_Mistral_MistralEmbeddingModelOptions_OutputDimension"></a>

### OutputDimension


```csharp
public int? OutputDimension { get; init; }
```


The dimension of the output embeddings when supported by the model.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralEmbeddingModelOptions.cs#L19)

<a id="Vercel_AI_Mistral_MistralEmbeddingModelOptions_OutputDtype_"></a>
<a id="Vercel_AI_Mistral_MistralEmbeddingModelOptions_OutputDtype"></a>

### OutputDtype


```csharp
public string? OutputDtype { get; init; }
```


The data type of the output embeddings when supported by the model: `float`, `int8`, `uint8`, `binary` or `ubinary`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralEmbeddingModelOptions.cs#L22)

## Methods

<a id="Vercel_AI_Mistral_MistralEmbeddingModelOptions_Parse_"></a>
<a id="Vercel_AI_Mistral_MistralEmbeddingModelOptions_Parse_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__"></a>

### `Parse(IReadOnlyDictionary<string, JsonElement>?)`


```csharp
public static MistralEmbeddingModelOptions? Parse(IReadOnlyDictionary<string, JsonElement>? providerOptions)
```


Parses `providerOptions.mistral`. Maps to `parseProviderOptions` with `mistralEmbeddingModelOptions`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [MistralEmbeddingModelOptions](./)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/) | An option has the wrong shape or is out of range. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralEmbeddingModelOptions.cs#L26)
