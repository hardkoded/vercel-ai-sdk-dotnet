---
title: "ParallelServiceCollectionExtensions"
description: "Registers Parallel."
editUrl: false
prev: false
next: false
sidebar:
  label: "ParallelServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Parallel_ParallelServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Parallel](/api/Vercel.AI.Parallel/)


```csharp
public static class ParallelServiceCollectionExtensions
```


Registers Parallel.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelProvider.cs#L131)

## Methods

<a id="Vercel_AI_Parallel_ParallelServiceCollectionExtensions_AddParallel_"></a>
<a id="Vercel_AI_Parallel_ParallelServiceCollectionExtensions_AddParallel_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_Parallel_ParallelProviderSettings__"></a>

### `AddParallel(IServiceCollection, Action<ParallelProviderSettings>?)`


```csharp
public static IServiceCollection AddParallel(this IServiceCollection services, Action<ParallelProviderSettings>? configure = null)
```


Registers [ParallelProvider](/api/Vercel.AI.Parallel/ParallelProvider/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;ParallelProviderSettings&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Parallel/ParallelProvider.cs#L134)
