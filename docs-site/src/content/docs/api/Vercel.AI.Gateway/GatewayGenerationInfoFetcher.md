---
title: "GatewayGenerationInfoFetcher"
description: "Fetches generation info (GET /v1/generation?id=) from the Gateway."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayGenerationInfoFetcher"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayGenerationInfoFetcher"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayGenerationInfoFetcher
```


Fetches generation info (`GET /v1/generation?id=`) from the Gateway.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L142)

## Constructors

<a id="Vercel_AI_Gateway_GatewayGenerationInfoFetcher__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfoFetcher__ctor_Vercel_AI_Gateway_GatewayConfig_"></a>

### GatewayGenerationInfoFetcher(GatewayConfig)


```csharp
public GatewayGenerationInfoFetcher(GatewayConfig config)
```


Initializes a new instance of the [GatewayGenerationInfoFetcher](./) class.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `config` | [GatewayConfig](/api/Vercel.AI.Gateway/GatewayConfig/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L147)

## Methods

<a id="Vercel_AI_Gateway_GatewayGenerationInfoFetcher_GetGenerationInfoAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayGenerationInfoFetcher_GetGenerationInfoAsync_Vercel_AI_Gateway_GatewayGenerationInfoParams_System_Threading_CancellationToken_"></a>

### GetGenerationInfoAsync(GatewayGenerationInfoParams, CancellationToken)


```csharp
public Task<GatewayGenerationInfo> GetGenerationInfoAsync(GatewayGenerationInfoParams parameters, CancellationToken cancellationToken = default)
```


Gets the info of one generation.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `parameters` | [GatewayGenerationInfoParams](/api/Vercel.AI.Gateway/GatewayGenerationInfoParams/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;GatewayGenerationInfo&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayGenerationInfo.cs#L153)
