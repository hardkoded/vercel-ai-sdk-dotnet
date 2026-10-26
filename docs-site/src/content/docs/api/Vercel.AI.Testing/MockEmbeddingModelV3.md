---
title: "MockEmbeddingModelV3"
description: "Embedding model v3 test double. Array-backed results are returned in order, starting at the first entry."
editUrl: false
prev: false
next: false
sidebar:
  label: "MockEmbeddingModelV3"
  hidden: true
---

<a id="Vercel_AI_Testing_MockEmbeddingModelV3"></a>

Namespace: [Vercel.AI.Testing](/api/Vercel.AI.Testing/)


```csharp
public sealed class MockEmbeddingModelV3
```


Embedding model v3 test double. Array-backed results are returned in order, starting at the first entry.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L310)

## Constructors

<a id="Vercel_AI_Testing_MockEmbeddingModelV3__ctor_"></a>
<a id="Vercel_AI_Testing_MockEmbeddingModelV3__ctor_System_String_System_String_System_Nullable_System_Int32__System_Boolean_System_Collections_Generic_IReadOnlyList_Vercel_AI_Testing_MockEmbedResult__System_Func_Vercel_AI_Testing_MockEmbedOptions_System_Threading_Tasks_Task_Vercel_AI_Testing_MockEmbedResult___"></a>

### `MockEmbeddingModelV3(string, string, int?, bool, IReadOnlyList<MockEmbedResult>?, Func<MockEmbedOptions?, Task<MockEmbedResult>>?)`


```csharp
public MockEmbeddingModelV3(string provider = "mock-provider", string modelId = "mock-model-id", int? maxEmbeddingsPerCall = 1, bool supportsParallelCalls = false, IReadOnlyList<MockEmbedResult>? doEmbed = null, Func<MockEmbedOptions?, Task<MockEmbedResult>>? embed = null)
```


Creates a model. List results are consumed from the first entry.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `maxEmbeddingsPerCall` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `supportsParallelCalls` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `doEmbed` | [IReadOnlyList&lt;MockEmbedResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `embed` | [Func&lt;MockEmbedOptions, Task&lt;MockEmbedResult&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L316)

## Properties

<a id="Vercel_AI_Testing_MockEmbeddingModelV3_DoEmbedCalls_"></a>
<a id="Vercel_AI_Testing_MockEmbeddingModelV3_DoEmbedCalls"></a>

### DoEmbedCalls


```csharp
public List<MockEmbedOptions> DoEmbedCalls { get; }
```


Embed calls in order.

**Value:** [List&lt;MockEmbedOptions&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L351)

<a id="Vercel_AI_Testing_MockEmbeddingModelV3_MaxEmbeddingsPerCall_"></a>
<a id="Vercel_AI_Testing_MockEmbeddingModelV3_MaxEmbeddingsPerCall"></a>

### MaxEmbeddingsPerCall


```csharp
public int? MaxEmbeddingsPerCall { get; }
```


Maximum embeddings per call.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L345)

<a id="Vercel_AI_Testing_MockEmbeddingModelV3_ModelId_"></a>
<a id="Vercel_AI_Testing_MockEmbeddingModelV3_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L342)

<a id="Vercel_AI_Testing_MockEmbeddingModelV3_Provider_"></a>
<a id="Vercel_AI_Testing_MockEmbeddingModelV3_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L339)

<a id="Vercel_AI_Testing_MockEmbeddingModelV3_SpecificationVersion_"></a>
<a id="Vercel_AI_Testing_MockEmbeddingModelV3_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L333)

<a id="Vercel_AI_Testing_MockEmbeddingModelV3_SupportsParallelCalls_"></a>
<a id="Vercel_AI_Testing_MockEmbeddingModelV3_SupportsParallelCalls"></a>

### SupportsParallelCalls


```csharp
public bool SupportsParallelCalls { get; }
```


Whether calls may run in parallel.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L348)

## Methods

<a id="Vercel_AI_Testing_MockEmbeddingModelV3_DoEmbedAsync_"></a>
<a id="Vercel_AI_Testing_MockEmbeddingModelV3_DoEmbedAsync_Vercel_AI_Testing_MockEmbedOptions_"></a>

### DoEmbedAsync(MockEmbedOptions?)


```csharp
public Task<MockEmbedResult> DoEmbedAsync(MockEmbedOptions? options)
```


Returns the next scripted embed result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [MockEmbedOptions](/api/Vercel.AI.Testing/MockEmbedOptions/) |  |

**Returns:** [Task&lt;MockEmbedResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L354)
