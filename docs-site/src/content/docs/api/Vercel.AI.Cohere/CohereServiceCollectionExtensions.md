---
title: "CohereServiceCollectionExtensions"
description: "Registers Cohere."
editUrl: false
prev: false
next: false
sidebar:
  label: "CohereServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Cohere_CohereServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Cohere](/api/Vercel.AI.Cohere/)


```csharp
public static class CohereServiceCollectionExtensions
```


Registers Cohere.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L391)

## Methods

<a id="Vercel_AI_Cohere_CohereServiceCollectionExtensions_AddCohere_"></a>
<a id="Vercel_AI_Cohere_CohereServiceCollectionExtensions_AddCohere_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_Cohere_CohereOptions__"></a>

### `AddCohere(IServiceCollection, Action<CohereOptions>?)`


```csharp
public static IServiceCollection AddCohere(this IServiceCollection services, Action<CohereOptions>? configure = null)
```


Adds [CohereProvider](/api/Vercel.AI.Cohere/CohereProvider/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;CohereOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L394)
