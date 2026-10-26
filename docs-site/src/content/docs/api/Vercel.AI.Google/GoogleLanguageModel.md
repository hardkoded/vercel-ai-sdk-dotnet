---
title: "GoogleLanguageModel"
description: "Gemini language model. Posts to models/{id}:generateContent."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleLanguageModel"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleLanguageModel"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleLanguageModel : ILanguageModel
```


Gemini language model. Posts to `models/{id}:generateContent`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleLanguageModel.cs#L16)

## Constructors

<a id="Vercel_AI_Google_GoogleLanguageModel__ctor_"></a>
<a id="Vercel_AI_Google_GoogleLanguageModel__ctor_Vercel_AI_Google_GoogleProvider_System_String_"></a>

### GoogleLanguageModel(GoogleProvider, string)


```csharp
public GoogleLanguageModel(GoogleProvider provider, string modelId)
```


Creates a model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GoogleProvider](/api/Vercel.AI.Google/GoogleProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleLanguageModel.cs#L26)

## Properties

<a id="Vercel_AI_Google_GoogleLanguageModel_ModelId_"></a>
<a id="Vercel_AI_Google_GoogleLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleLanguageModel.cs#L45)

<a id="Vercel_AI_Google_GoogleLanguageModel_Provider_"></a>
<a id="Vercel_AI_Google_GoogleLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleLanguageModel.cs#L39)

<a id="Vercel_AI_Google_GoogleLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Google_GoogleLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleLanguageModel.cs#L33)

## Methods

<a id="Vercel_AI_Google_GoogleLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Google_GoogleLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleLanguageModel.cs#L54)

<a id="Vercel_AI_Google_GoogleLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_Google_GoogleLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleLanguageModel.cs#L148)

<a id="Vercel_AI_Google_GoogleLanguageModel_Prepare_"></a>
<a id="Vercel_AI_Google_GoogleLanguageModel_Prepare_Vercel_AI_Provider_LanguageModelCallOptions_System_Boolean_"></a>

### Prepare(LanguageModelCallOptions, bool)


```csharp
public GooglePreparedRequest Prepare(LanguageModelCallOptions options, bool streaming)
```


Builds the generateContent body without sending it.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `streaming` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [GooglePreparedRequest](/api/Vercel.AI.Google/GooglePreparedRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleLanguageModel.cs#L48)
