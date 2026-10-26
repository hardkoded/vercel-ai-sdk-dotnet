---
title: "EmbeddingModelCall"
description: "Arguments for one doEmbed call."
editUrl: false
prev: false
next: false
sidebar:
  label: "EmbeddingModelCall"
  hidden: true
---

<a id="Vercel_AI_Operations_EmbeddingModelCall"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class EmbeddingModelCall
```


Arguments for one `doEmbed` call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L46)

## Constructors

<a id="Vercel_AI_Operations_EmbeddingModelCall__ctor_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCall__ctor_System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Nullable_System_Text_Json_JsonElement__System_Threading_CancellationToken_System_Nullable_System_Int32__"></a>

### `EmbeddingModelCall(IReadOnlyList<string>, IReadOnlyDictionary<string, string>, JsonElement?, CancellationToken, int?)`


```csharp
public EmbeddingModelCall(IReadOnlyList<string> values, IReadOnlyDictionary<string, string> headers, JsonElement? providerOptions, CancellationToken cancellationToken, int? dimensions = null)
```


Creates a model call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `values` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `providerOptions` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `dimensions` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L49)

## Properties

<a id="Vercel_AI_Operations_EmbeddingModelCall_CancellationToken_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCall_CancellationToken"></a>

### CancellationToken


```csharp
public CancellationToken CancellationToken { get; }
```


Cancellation token.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L71)

<a id="Vercel_AI_Operations_EmbeddingModelCall_Dimensions_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCall_Dimensions"></a>

### Dimensions


```csharp
public int? Dimensions { get; }
```


Requested output dimensions. Null when the caller did not ask for any.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L59)

<a id="Vercel_AI_Operations_EmbeddingModelCall_Headers_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCall_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Headers, including the user-agent suffix.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L65)

<a id="Vercel_AI_Operations_EmbeddingModelCall_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCall_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; }
```


Provider options for this chunk.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L68)

<a id="Vercel_AI_Operations_EmbeddingModelCall_Values_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCall_Values"></a>

### Values


```csharp
public IReadOnlyList<string> Values { get; }
```


Values in this call.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L62)
