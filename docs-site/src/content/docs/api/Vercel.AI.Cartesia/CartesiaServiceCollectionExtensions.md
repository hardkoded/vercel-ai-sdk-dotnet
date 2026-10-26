---
title: "CartesiaServiceCollectionExtensions"
description: "Registers Cartesia."
editUrl: false
prev: false
next: false
sidebar:
  label: "CartesiaServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Cartesia_CartesiaServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Cartesia](/api/Vercel.AI.Cartesia/)


```csharp
public static class CartesiaServiceCollectionExtensions
```


Registers Cartesia.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L405)

## Methods

<a id="Vercel_AI_Cartesia_CartesiaServiceCollectionExtensions_AddCartesia_"></a>
<a id="Vercel_AI_Cartesia_CartesiaServiceCollectionExtensions_AddCartesia_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddCartesia(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddCartesia(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L408)
