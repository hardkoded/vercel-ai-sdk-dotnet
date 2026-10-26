---
title: "GoogleInteractionsModel"
description: "Language model that posts to the Gemini Interactions API."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleInteractionsModel"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleInteractionsModel"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleInteractionsModel : ILanguageModel
```


Language model that posts to the Gemini Interactions API.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractions.cs#L188)

## Constructors

<a id="Vercel_AI_Google_GoogleInteractionsModel__ctor_"></a>
<a id="Vercel_AI_Google_GoogleInteractionsModel__ctor_Vercel_AI_Google_GoogleProvider_System_String_"></a>

### GoogleInteractionsModel(GoogleProvider, string)


```csharp
public GoogleInteractionsModel(GoogleProvider provider, string modelId)
```


Creates an interactions model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GoogleProvider](/api/Vercel.AI.Google/GoogleProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractions.cs#L194)

<a id="Vercel_AI_Google_GoogleInteractionsModel__ctor_Vercel_AI_Google_GoogleProvider_Vercel_AI_Google_GoogleInteractionsAgent_"></a>

### GoogleInteractionsModel(GoogleProvider, GoogleInteractionsAgent)


```csharp
public GoogleInteractionsModel(GoogleProvider provider, GoogleInteractionsAgent agent)
```


Creates an interactions model that calls an agent. The request sends `agent` instead of `model`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GoogleProvider](/api/Vercel.AI.Google/GoogleProvider/) |  |
| `agent` | [GoogleInteractionsAgent](/api/Vercel.AI.Google/GoogleInteractionsAgent/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractions.cs#L201)

## Properties

<a id="Vercel_AI_Google_GoogleInteractionsModel_ModelId_"></a>
<a id="Vercel_AI_Google_GoogleInteractionsModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractions.cs#L225)

<a id="Vercel_AI_Google_GoogleInteractionsModel_Provider_"></a>
<a id="Vercel_AI_Google_GoogleInteractionsModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractions.cs#L214)

<a id="Vercel_AI_Google_GoogleInteractionsModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Google_GoogleInteractionsModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractions.cs#L208)

## Methods

<a id="Vercel_AI_Google_GoogleInteractionsModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Google_GoogleInteractionsModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractions.cs#L228)

<a id="Vercel_AI_Google_GoogleInteractionsModel_DoStreamAsync_"></a>
<a id="Vercel_AI_Google_GoogleInteractionsModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractions.cs#L530)
