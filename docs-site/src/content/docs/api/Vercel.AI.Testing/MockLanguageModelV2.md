---
title: "MockLanguageModelV2"
description: "Language model v2 test double. Array-backed results are returned in order, starting at the first entry."
editUrl: false
prev: false
next: false
sidebar:
  label: "MockLanguageModelV2"
  hidden: true
---

<a id="Vercel_AI_Testing_MockLanguageModelV2"></a>

Namespace: [Vercel.AI.Testing](/api/Vercel.AI.Testing/)


```csharp
public sealed class MockLanguageModelV2
```


Language model v2 test double. Array-backed results are returned in order, starting at the first entry.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L116)

## Constructors

<a id="Vercel_AI_Testing_MockLanguageModelV2__ctor_"></a>
<a id="Vercel_AI_Testing_MockLanguageModelV2__ctor_System_String_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Testing_MockGenerateResult__System_Collections_Generic_IReadOnlyList_Vercel_AI_Testing_MockStreamResult__System_Func_Vercel_AI_Testing_MockLanguageModelCallOptions_System_Threading_Tasks_Task_Vercel_AI_Testing_MockGenerateResult___System_Func_Vercel_AI_Testing_MockLanguageModelCallOptions_System_Threading_Tasks_Task_Vercel_AI_Testing_MockStreamResult___"></a>

### `MockLanguageModelV2(string, string, IReadOnlyList<MockGenerateResult>?, IReadOnlyList<MockStreamResult>?, Func<MockLanguageModelCallOptions?, Task<MockGenerateResult>>?, Func<MockLanguageModelCallOptions?, Task<MockStreamResult>>?)`


```csharp
public MockLanguageModelV2(string provider = "mock-provider", string modelId = "mock-model-id", IReadOnlyList<MockGenerateResult>? doGenerate = null, IReadOnlyList<MockStreamResult>? doStream = null, Func<MockLanguageModelCallOptions?, Task<MockGenerateResult>>? generate = null, Func<MockLanguageModelCallOptions?, Task<MockStreamResult>>? stream = null)
```


Creates a model. List results are consumed from the first entry.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `doGenerate` | [IReadOnlyList&lt;MockGenerateResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `doStream` | [IReadOnlyList&lt;MockStreamResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `generate` | [Func&lt;MockLanguageModelCallOptions, Task&lt;MockGenerateResult&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `stream` | [Func&lt;MockLanguageModelCallOptions, Task&lt;MockStreamResult&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L124)

## Properties

<a id="Vercel_AI_Testing_MockLanguageModelV2_DoGenerateCalls_"></a>
<a id="Vercel_AI_Testing_MockLanguageModelV2_DoGenerateCalls"></a>

### DoGenerateCalls


```csharp
public List<MockLanguageModelCallOptions> DoGenerateCalls { get; }
```


Generate calls in order.

**Value:** [List&lt;MockLanguageModelCallOptions&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L153)

<a id="Vercel_AI_Testing_MockLanguageModelV2_DoStreamCalls_"></a>
<a id="Vercel_AI_Testing_MockLanguageModelV2_DoStreamCalls"></a>

### DoStreamCalls


```csharp
public List<MockLanguageModelCallOptions> DoStreamCalls { get; }
```


Stream calls in order.

**Value:** [List&lt;MockLanguageModelCallOptions&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L156)

<a id="Vercel_AI_Testing_MockLanguageModelV2_ModelId_"></a>
<a id="Vercel_AI_Testing_MockLanguageModelV2_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L150)

<a id="Vercel_AI_Testing_MockLanguageModelV2_Provider_"></a>
<a id="Vercel_AI_Testing_MockLanguageModelV2_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L147)

<a id="Vercel_AI_Testing_MockLanguageModelV2_SpecificationVersion_"></a>
<a id="Vercel_AI_Testing_MockLanguageModelV2_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L141)

## Methods

<a id="Vercel_AI_Testing_MockLanguageModelV2_DoGenerateAsync_"></a>
<a id="Vercel_AI_Testing_MockLanguageModelV2_DoGenerateAsync_Vercel_AI_Testing_MockLanguageModelCallOptions_"></a>

### DoGenerateAsync(MockLanguageModelCallOptions?)


```csharp
public Task<MockGenerateResult> DoGenerateAsync(MockLanguageModelCallOptions? options)
```


Returns the next scripted generate result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [MockLanguageModelCallOptions](/api/Vercel.AI.Testing/MockLanguageModelCallOptions/) |  |

**Returns:** [Task&lt;MockGenerateResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L159)

<a id="Vercel_AI_Testing_MockLanguageModelV2_DoStreamAsync_"></a>
<a id="Vercel_AI_Testing_MockLanguageModelV2_DoStreamAsync_Vercel_AI_Testing_MockLanguageModelCallOptions_"></a>

### DoStreamAsync(MockLanguageModelCallOptions?)


```csharp
public Task<MockStreamResult> DoStreamAsync(MockLanguageModelCallOptions? options)
```


Returns the next scripted stream result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [MockLanguageModelCallOptions](/api/Vercel.AI.Testing/MockLanguageModelCallOptions/) |  |

**Returns:** [Task&lt;MockStreamResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L176)
