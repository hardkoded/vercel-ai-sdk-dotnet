---
title: "AnthropicAwsProvider"
description: "Anthropic on AWS. Same Messages body, with regional hosts, workspace ids, and SigV4."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicAwsProvider"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicAwsProvider"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicAwsProvider : AnthropicProvider, IBatchProvider, IDecisionProvider, IEvaluationProvider
```


Anthropic on AWS. Same Messages body, with regional hosts, workspace ids, and SigV4.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [AnthropicProvider](/api/Vercel.AI.Anthropic/AnthropicProvider/)

**Implements:** [IBatchProvider](/api/Vercel.AI.Operations/IBatchProvider/), [IDecisionProvider](/api/Vercel.AI.Operations/IDecisionProvider/), [IEvaluationProvider](/api/Vercel.AI.Operations/IEvaluationProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L612)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicAwsProvider__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_Anthropic_AnthropicOptions_"></a>

### AnthropicAwsProvider(HttpClient, AnthropicOptions?)


```csharp
public AnthropicAwsProvider(HttpClient httpClient, AnthropicOptions? options = null)
```


Creates an Anthropic on AWS provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [AnthropicOptions](/api/Vercel.AI.Anthropic/AnthropicOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L615)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicAwsProvider_Create_"></a>
<a id="Vercel_AI_Anthropic_AnthropicAwsProvider_Create_Vercel_AI_Anthropic_AnthropicOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(AnthropicOptions?, HttpMessageHandler?)


```csharp
public static AnthropicAwsProvider Create(AnthropicOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [AnthropicOptions](/api/Vercel.AI.Anthropic/AnthropicOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [AnthropicAwsProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L652)
