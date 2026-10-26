---
title: "MockLanguageModelV3"
description: "Language model v3 test double. Array-backed results are returned in order, starting at the first entry."
editUrl: false
prev: false
next: false
sidebar:
  label: "MockLanguageModelV3"
  hidden: true
---

<a id="Vercel_AI_Testing_MockLanguageModelV3"></a>

Namespace: [Vercel.AI.Testing](/api/Vercel.AI.Testing/)


```csharp
public sealed class MockLanguageModelV3
```


Language model v3 test double. Array-backed results are returned in order, starting at the first entry.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L194)

## Constructors

<a id="Vercel_AI_Testing_MockLanguageModelV3__ctor_"></a>
<a id="Vercel_AI_Testing_MockLanguageModelV3__ctor_System_String_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Testing_MockGenerateResult__System_Collections_Generic_IReadOnlyList_Vercel_AI_Testing_MockStreamResult__"></a>

### `MockLanguageModelV3(string, string, IReadOnlyList<MockGenerateResult>?, IReadOnlyList<MockStreamResult>?)`


```csharp
public MockLanguageModelV3(string provider = "mock-provider", string modelId = "mock-model-id", IReadOnlyList<MockGenerateResult>? doGenerate = null, IReadOnlyList<MockStreamResult>? doStream = null)
```


Creates a model. List results are consumed from the first entry.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `doGenerate` | [IReadOnlyList&lt;MockGenerateResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `doStream` | [IReadOnlyList&lt;MockStreamResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L199)

## Properties

<a id="Vercel_AI_Testing_MockLanguageModelV3_DoGenerateCalls_"></a>
<a id="Vercel_AI_Testing_MockLanguageModelV3_DoGenerateCalls"></a>

### DoGenerateCalls


```csharp
public List<MockLanguageModelCallOptions> DoGenerateCalls { get; }
```


Generate calls in order.

**Value:** [List&lt;MockLanguageModelCallOptions&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L227)

<a id="Vercel_AI_Testing_MockLanguageModelV3_DoStreamCalls_"></a>
<a id="Vercel_AI_Testing_MockLanguageModelV3_DoStreamCalls"></a>

### DoStreamCalls


```csharp
public List<MockLanguageModelCallOptions> DoStreamCalls { get; }
```


Stream calls in order.

**Value:** [List&lt;MockLanguageModelCallOptions&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L233)

<a id="Vercel_AI_Testing_MockLanguageModelV3_ModelId_"></a>
<a id="Vercel_AI_Testing_MockLanguageModelV3_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L221)

<a id="Vercel_AI_Testing_MockLanguageModelV3_Provider_"></a>
<a id="Vercel_AI_Testing_MockLanguageModelV3_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L215)

<a id="Vercel_AI_Testing_MockLanguageModelV3_SpecificationVersion_"></a>
<a id="Vercel_AI_Testing_MockLanguageModelV3_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L209)

## Methods

<a id="Vercel_AI_Testing_MockLanguageModelV3_DoGenerateAsync_"></a>
<a id="Vercel_AI_Testing_MockLanguageModelV3_DoGenerateAsync_Vercel_AI_Testing_MockLanguageModelCallOptions_"></a>

### DoGenerateAsync(MockLanguageModelCallOptions)


```csharp
public Task<MockGenerateResult> DoGenerateAsync(MockLanguageModelCallOptions options)
```


Returns the next scripted generate result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [MockLanguageModelCallOptions](/api/Vercel.AI.Testing/MockLanguageModelCallOptions/) |  |

**Returns:** [Task&lt;MockGenerateResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L239)

<a id="Vercel_AI_Testing_MockLanguageModelV3_DoStreamAsync_"></a>
<a id="Vercel_AI_Testing_MockLanguageModelV3_DoStreamAsync_Vercel_AI_Testing_MockLanguageModelCallOptions_"></a>

### DoStreamAsync(MockLanguageModelCallOptions)


```csharp
public Task<MockStreamResult> DoStreamAsync(MockLanguageModelCallOptions options)
```


Returns the next scripted stream result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [MockLanguageModelCallOptions](/api/Vercel.AI.Testing/MockLanguageModelCallOptions/) |  |

**Returns:** [Task&lt;MockStreamResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L245)
