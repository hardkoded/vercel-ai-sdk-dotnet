---
title: "BasetenServiceCollectionExtensions"
description: "Registers BasetenProvider."
editUrl: false
prev: false
next: false
sidebar:
  label: "BasetenServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Baseten_BasetenServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Baseten](/api/Vercel.AI.Baseten/)


```csharp
public static class BasetenServiceCollectionExtensions
```


Registers [BasetenProvider](/api/Vercel.AI.Baseten/BasetenProvider/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Baseten/BasetenProvider.cs#L124)

## Methods

<a id="Vercel_AI_Baseten_BasetenServiceCollectionExtensions_AddBaseten_"></a>
<a id="Vercel_AI_Baseten_BasetenServiceCollectionExtensions_AddBaseten_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddBaseten(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddBaseten(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Baseten/BasetenProvider.cs#L127)
