---
title: "GatewayServiceCollectionExtensions"
description: "Registers the Gateway provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public static class GatewayServiceCollectionExtensions
```


Registers the Gateway provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L658)

## Methods

<a id="Vercel_AI_Gateway_GatewayServiceCollectionExtensions_AddGateway_"></a>
<a id="Vercel_AI_Gateway_GatewayServiceCollectionExtensions_AddGateway_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_Gateway_GatewayOptions__"></a>

### `AddGateway(IServiceCollection, Action<GatewayOptions>?)`


```csharp
public static IServiceCollection AddGateway(this IServiceCollection services, Action<GatewayOptions>? configure = null)
```


Registers [GatewayProvider](/api/Vercel.AI.Gateway/GatewayProvider/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;GatewayOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L661)
