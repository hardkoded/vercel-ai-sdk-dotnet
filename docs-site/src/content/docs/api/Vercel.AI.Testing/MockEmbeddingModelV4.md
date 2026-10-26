---
title: "MockEmbeddingModelV4"
description: "Embedding model v4 test double. Array-backed results are returned in order, starting at the first entry."
editUrl: false
prev: false
next: false
sidebar:
  label: "MockEmbeddingModelV4"
  hidden: true
---

<a id="Vercel_AI_Testing_MockEmbeddingModelV4"></a>

Namespace: [Vercel.AI.Testing](/api/Vercel.AI.Testing/)


```csharp
public sealed class MockEmbeddingModelV4
```


Embedding model v4 test double. Array-backed results are returned in order, starting at the first entry.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L372)

## Constructors

<a id="Vercel_AI_Testing_MockEmbeddingModelV4__ctor_"></a>
<a id="Vercel_AI_Testing_MockEmbeddingModelV4__ctor_System_String_System_String_System_Nullable_System_Int32__System_Nullable_System_Int32__System_Boolean_System_Collections_Generic_IReadOnlyList_Vercel_AI_Testing_MockEmbedResult__"></a>

### `MockEmbeddingModelV4(string, string, int?, int?, bool, IReadOnlyList<MockEmbedResult>?)`


```csharp
public MockEmbeddingModelV4(string provider = "mock-provider", string modelId = "mock-model-id", int? maxEmbeddingsPerCall = 1, int? maxInputBytesPerCall = null, bool supportsParallelCalls = false, IReadOnlyList<MockEmbedResult>? doEmbed = null)
```


Creates a model. List results are consumed from the first entry.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `maxEmbeddingsPerCall` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `maxInputBytesPerCall` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `supportsParallelCalls` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `doEmbed` | [IReadOnlyList&lt;MockEmbedResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L377)

## Properties

<a id="Vercel_AI_Testing_MockEmbeddingModelV4_DoEmbedCalls_"></a>
<a id="Vercel_AI_Testing_MockEmbeddingModelV4_DoEmbedCalls"></a>

### DoEmbedCalls


```csharp
public List<MockEmbedOptions> DoEmbedCalls { get; }
```


Embed calls in order.

**Value:** [List&lt;MockEmbedOptions&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L423)

<a id="Vercel_AI_Testing_MockEmbeddingModelV4_MaxEmbeddingsPerCall_"></a>
<a id="Vercel_AI_Testing_MockEmbeddingModelV4_MaxEmbeddingsPerCall"></a>

### MaxEmbeddingsPerCall


```csharp
public int? MaxEmbeddingsPerCall { get; }
```


Maximum embeddings per call.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L408)

<a id="Vercel_AI_Testing_MockEmbeddingModelV4_MaxInputBytesPerCall_"></a>
<a id="Vercel_AI_Testing_MockEmbeddingModelV4_MaxInputBytesPerCall"></a>

### MaxInputBytesPerCall


```csharp
public int? MaxInputBytesPerCall { get; }
```


Maximum input bytes per call.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L414)

<a id="Vercel_AI_Testing_MockEmbeddingModelV4_ModelId_"></a>
<a id="Vercel_AI_Testing_MockEmbeddingModelV4_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L402)

<a id="Vercel_AI_Testing_MockEmbeddingModelV4_Provider_"></a>
<a id="Vercel_AI_Testing_MockEmbeddingModelV4_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L396)

<a id="Vercel_AI_Testing_MockEmbeddingModelV4_SpecificationVersion_"></a>
<a id="Vercel_AI_Testing_MockEmbeddingModelV4_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L390)

<a id="Vercel_AI_Testing_MockEmbeddingModelV4_SupportsParallelCalls_"></a>
<a id="Vercel_AI_Testing_MockEmbeddingModelV4_SupportsParallelCalls"></a>

### SupportsParallelCalls


```csharp
public bool SupportsParallelCalls { get; }
```


Whether calls may run in parallel.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L417)

## Methods

<a id="Vercel_AI_Testing_MockEmbeddingModelV4_DoEmbedAsync_"></a>
<a id="Vercel_AI_Testing_MockEmbeddingModelV4_DoEmbedAsync_Vercel_AI_Testing_MockEmbedOptions_"></a>

### DoEmbedAsync(MockEmbedOptions)


```csharp
public Task<MockEmbedResult> DoEmbedAsync(MockEmbedOptions options)
```


Returns the next scripted embed result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [MockEmbedOptions](/api/Vercel.AI.Testing/MockEmbedOptions/) |  |

**Returns:** [Task&lt;MockEmbedResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L429)
