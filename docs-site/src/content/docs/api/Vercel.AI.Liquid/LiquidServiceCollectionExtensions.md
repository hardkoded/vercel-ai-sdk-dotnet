---
title: "LiquidServiceCollectionExtensions"
description: "Registers Liquid."
editUrl: false
prev: false
next: false
sidebar:
  label: "LiquidServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Liquid_LiquidServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Liquid](/api/Vercel.AI.Liquid/)


```csharp
public static class LiquidServiceCollectionExtensions
```


Registers Liquid.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidProvider.cs#L74)

## Methods

<a id="Vercel_AI_Liquid_LiquidServiceCollectionExtensions_AddLiquid_"></a>
<a id="Vercel_AI_Liquid_LiquidServiceCollectionExtensions_AddLiquid_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddLiquid(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddLiquid(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidProvider.cs#L77)
