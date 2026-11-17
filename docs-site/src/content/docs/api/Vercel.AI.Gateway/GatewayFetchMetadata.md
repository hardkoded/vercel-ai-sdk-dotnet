---
title: "GatewayFetchMetadata"
description: "Fetches the available models (GET /config) and the credits (GET /v1/credits) from the Gateway."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayFetchMetadata"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayFetchMetadata"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayFetchMetadata
```


Fetches the available models (`GET /config`) and the credits (`GET /v1/credits`) from the Gateway.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayFetchMetadata.cs#L27)

## Constructors

<a id="Vercel_AI_Gateway_GatewayFetchMetadata__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayFetchMetadata__ctor_Vercel_AI_Gateway_GatewayConfig_"></a>

### GatewayFetchMetadata(GatewayConfig)


```csharp
public GatewayFetchMetadata(GatewayConfig config)
```


Initializes a new instance of the [GatewayFetchMetadata](./) class.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `config` | [GatewayConfig](/api/Vercel.AI.Gateway/GatewayConfig/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayFetchMetadata.cs#L32)

## Methods

<a id="Vercel_AI_Gateway_GatewayFetchMetadata_GetAvailableModelsAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayFetchMetadata_GetAvailableModelsAsync_System_Threading_CancellationToken_"></a>

### GetAvailableModelsAsync(CancellationToken)


```csharp
public Task<GatewayFetchMetadataResponse> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
```


Gets the models the Gateway offers.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;GatewayFetchMetadataResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayFetchMetadata.cs#L38)

<a id="Vercel_AI_Gateway_GatewayFetchMetadata_GetCreditsAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayFetchMetadata_GetCreditsAsync_System_Threading_CancellationToken_"></a>

### GetCreditsAsync(CancellationToken)


```csharp
public Task<GatewayCreditsResponse> GetCreditsAsync(CancellationToken cancellationToken = default)
```


Gets the credit balance of the account.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;GatewayCreditsResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayFetchMetadata.cs#L63)
