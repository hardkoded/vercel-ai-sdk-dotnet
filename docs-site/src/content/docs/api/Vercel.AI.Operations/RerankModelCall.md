---
title: "RerankModelCall"
description: "Arguments for doRerank."
editUrl: false
prev: false
next: false
sidebar:
  label: "RerankModelCall"
  hidden: true
---

<a id="Vercel_AI_Operations_RerankModelCall"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class RerankModelCall
```


Arguments for `doRerank`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L69)

## Constructors

<a id="Vercel_AI_Operations_RerankModelCall__ctor_"></a>
<a id="Vercel_AI_Operations_RerankModelCall__ctor_Vercel_AI_Operations_RerankModelDocuments_System_String_System_Nullable_System_Int32__System_Nullable_System_Text_Json_JsonElement__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `RerankModelCall(RerankModelDocuments, string, int?, JsonElement?, IReadOnlyDictionary<string, string>?, CancellationToken)`


```csharp
public RerankModelCall(RerankModelDocuments documents, string query, int? topN, JsonElement? providerOptions, IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken)
```


Creates a model call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `documents` | [RerankModelDocuments](/api/Vercel.AI.Operations/RerankModelDocuments/) |  |
| `query` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `topN` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `providerOptions` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L72)

## Properties

<a id="Vercel_AI_Operations_RerankModelCall_CancellationToken_"></a>
<a id="Vercel_AI_Operations_RerankModelCall_CancellationToken"></a>

### CancellationToken


```csharp
public CancellationToken CancellationToken { get; }
```


Cancellation token.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L98)

<a id="Vercel_AI_Operations_RerankModelCall_Documents_"></a>
<a id="Vercel_AI_Operations_RerankModelCall_Documents"></a>

### Documents


```csharp
public RerankModelDocuments Documents { get; }
```


Documents.

**Value:** [RerankModelDocuments](/api/Vercel.AI.Operations/RerankModelDocuments/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L83)

<a id="Vercel_AI_Operations_RerankModelCall_Headers_"></a>
<a id="Vercel_AI_Operations_RerankModelCall_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; }
```


Request headers. Rerank does not append a user-agent.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L95)

<a id="Vercel_AI_Operations_RerankModelCall_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_RerankModelCall_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; }
```


Provider options.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L92)

<a id="Vercel_AI_Operations_RerankModelCall_Query_"></a>
<a id="Vercel_AI_Operations_RerankModelCall_Query"></a>

### Query


```csharp
public string Query { get; }
```


Query.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L86)

<a id="Vercel_AI_Operations_RerankModelCall_TopN_"></a>
<a id="Vercel_AI_Operations_RerankModelCall_TopN"></a>

### TopN


```csharp
public int? TopN { get; }
```


Maximum results.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L89)
