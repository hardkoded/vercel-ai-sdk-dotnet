---
title: "TestLanguageModel"
description: "Language model that returns scripted generate and stream results."
editUrl: false
prev: false
next: false
sidebar:
  label: "TestLanguageModel"
  hidden: true
---

<a id="Vercel_AI_Testing_TestLanguageModel"></a>

Namespace: [Vercel.AI.Testing](/api/Vercel.AI.Testing/)


```csharp
public sealed class TestLanguageModel : ILanguageModel
```


Language model that returns scripted generate and stream results.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/TestLanguageModel.cs#L11)

## Constructors

<a id="Vercel_AI_Testing_TestLanguageModel__ctor_"></a>
<a id="Vercel_AI_Testing_TestLanguageModel__ctor_System_String_"></a>

### TestLanguageModel(string)


```csharp
public TestLanguageModel(string modelId = "test")
```


Creates a model with id &lt;code class="paramref"&gt;modelId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/TestLanguageModel.cs#L14)

## Properties

<a id="Vercel_AI_Testing_TestLanguageModel_Calls_"></a>
<a id="Vercel_AI_Testing_TestLanguageModel_Calls"></a>

### Calls


```csharp
public List<LanguageModelCallOptions> Calls { get; }
```


Calls observed by generate and stream.

**Value:** [List&lt;LanguageModelCallOptions&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/TestLanguageModel.cs#L29)

<a id="Vercel_AI_Testing_TestLanguageModel_ModelId_"></a>
<a id="Vercel_AI_Testing_TestLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/TestLanguageModel.cs#L26)

<a id="Vercel_AI_Testing_TestLanguageModel_OnGenerate_"></a>
<a id="Vercel_AI_Testing_TestLanguageModel_OnGenerate"></a>

### OnGenerate


```csharp
public Func<LanguageModelCallOptions, LanguageModelGenerateResult>? OnGenerate { get; set; }
```


When set, generate returns this function's result.

**Value:** [Func&lt;LanguageModelCallOptions, LanguageModelGenerateResult&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/TestLanguageModel.cs#L32)

<a id="Vercel_AI_Testing_TestLanguageModel_Provider_"></a>
<a id="Vercel_AI_Testing_TestLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/TestLanguageModel.cs#L23)

<a id="Vercel_AI_Testing_TestLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Testing_TestLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/TestLanguageModel.cs#L20)

<a id="Vercel_AI_Testing_TestLanguageModel_StreamParts_"></a>
<a id="Vercel_AI_Testing_TestLanguageModel_StreamParts"></a>

### StreamParts


```csharp
public IReadOnlyList<LanguageModelStreamPart>? StreamParts { get; set; }
```


When set, stream yields these parts instead of a single text delta.

**Value:** [IReadOnlyList&lt;LanguageModelStreamPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/TestLanguageModel.cs#L35)

<a id="Vercel_AI_Testing_TestLanguageModel_StreamRequest_"></a>
<a id="Vercel_AI_Testing_TestLanguageModel_StreamRequest"></a>

### StreamRequest


```csharp
public LanguageModelStreamRequest? StreamRequest { get; set; }
```


The request info stream calls report.

**Value:** [LanguageModelStreamRequest](/api/Vercel.AI.Provider/LanguageModelStreamRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/TestLanguageModel.cs#L38)

<a id="Vercel_AI_Testing_TestLanguageModel_StreamResponse_"></a>
<a id="Vercel_AI_Testing_TestLanguageModel_StreamResponse"></a>

### StreamResponse


```csharp
public LanguageModelStreamResponse? StreamResponse { get; set; }
```


The response info stream calls report.

**Value:** [LanguageModelStreamResponse](/api/Vercel.AI.Provider/LanguageModelStreamResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/TestLanguageModel.cs#L41)

## Methods

<a id="Vercel_AI_Testing_TestLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Testing_TestLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(LanguageModelCallOptions, CancellationToken)


```csharp
public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
```


Generates a complete result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;LanguageModelGenerateResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/TestLanguageModel.cs#L54)

<a id="Vercel_AI_Testing_TestLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_Testing_TestLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

### DoStreamAsync(LanguageModelCallOptions, CancellationToken)


```csharp
public Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
```


Streams partial results. The result carries the stream with the request and response info.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;LanguageModelStreamResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/TestLanguageModel.cs#L63)

<a id="Vercel_AI_Testing_TestLanguageModel_Text_"></a>
<a id="Vercel_AI_Testing_TestLanguageModel_Text_System_String_"></a>

### Text(string)


```csharp
public static LanguageModelGenerateResult Text(string text)
```


A generate result whose only content is &lt;code class="paramref"&gt;text&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [LanguageModelGenerateResult](/api/Vercel.AI.Provider/LanguageModelGenerateResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/TestLanguageModel.cs#L44)
