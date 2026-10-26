---
title: "FireworksServiceCollectionExtensions"
description: "Registers FireworksProvider."
editUrl: false
prev: false
next: false
sidebar:
  label: "FireworksServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Fireworks_FireworksServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Fireworks](/api/Vercel.AI.Fireworks/)


```csharp
public static class FireworksServiceCollectionExtensions
```


Registers [FireworksProvider](/api/Vercel.AI.Fireworks/FireworksProvider/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fireworks/FireworksProvider.cs#L75)

## Methods

<a id="Vercel_AI_Fireworks_FireworksServiceCollectionExtensions_AddFireworks_"></a>
<a id="Vercel_AI_Fireworks_FireworksServiceCollectionExtensions_AddFireworks_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddFireworks(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddFireworks(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fireworks/FireworksProvider.cs#L78)
