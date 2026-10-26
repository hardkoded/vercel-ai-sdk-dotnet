---
title: "ProdiaServiceCollectionExtensions"
description: "Registers Prodia."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProdiaServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Prodia_ProdiaServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Prodia](/api/Vercel.AI.Prodia/)


```csharp
public static class ProdiaServiceCollectionExtensions
```


Registers Prodia.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaProvider.cs#L68)

## Methods

<a id="Vercel_AI_Prodia_ProdiaServiceCollectionExtensions_AddProdia_"></a>
<a id="Vercel_AI_Prodia_ProdiaServiceCollectionExtensions_AddProdia_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddProdia(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddProdia(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaProvider.cs#L71)
