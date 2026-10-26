---
title: "GatewayLanguageModel"
description: "Gateway language model. The request body is the V4 call options."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayLanguageModel"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayLanguageModel"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayLanguageModel : ILanguageModel
```


Gateway language model. The request body is the V4 call options.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L216)

## Constructors

<a id="Vercel_AI_Gateway_GatewayLanguageModel__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayLanguageModel__ctor_Vercel_AI_Gateway_GatewayProvider_System_String_"></a>

### GatewayLanguageModel(GatewayProvider, string)


```csharp
public GatewayLanguageModel(GatewayProvider provider, string modelId)
```


Creates a Gateway language model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GatewayProvider](/api/Vercel.AI.Gateway/GatewayProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L221)

## Properties

<a id="Vercel_AI_Gateway_GatewayLanguageModel_ModelId_"></a>
<a id="Vercel_AI_Gateway_GatewayLanguageModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L234)

<a id="Vercel_AI_Gateway_GatewayLanguageModel_Provider_"></a>
<a id="Vercel_AI_Gateway_GatewayLanguageModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `openai`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L231)

<a id="Vercel_AI_Gateway_GatewayLanguageModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Gateway_GatewayLanguageModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L228)

## Methods

<a id="Vercel_AI_Gateway_GatewayLanguageModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayLanguageModel_DoGenerateAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L237)

<a id="Vercel_AI_Gateway_GatewayLanguageModel_DoStreamAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayLanguageModel_DoStreamAsync_Vercel_AI_Provider_LanguageModelCallOptions_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L258)
