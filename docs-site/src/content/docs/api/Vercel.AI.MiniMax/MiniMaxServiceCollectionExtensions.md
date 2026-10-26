---
title: "MiniMaxServiceCollectionExtensions"
description: "Registers MiniMaxProvider."
editUrl: false
prev: false
next: false
sidebar:
  label: "MiniMaxServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_MiniMax_MiniMaxServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.MiniMax](/api/Vercel.AI.MiniMax/)


```csharp
public static class MiniMaxServiceCollectionExtensions
```


Registers [MiniMaxProvider](/api/Vercel.AI.MiniMax/MiniMaxProvider/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L106)

## Methods

<a id="Vercel_AI_MiniMax_MiniMaxServiceCollectionExtensions_AddMiniMax_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxServiceCollectionExtensions_AddMiniMax_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_MiniMax_MiniMaxOptions__"></a>

### `AddMiniMax(IServiceCollection, Action<MiniMaxOptions>?)`


```csharp
public static IServiceCollection AddMiniMax(this IServiceCollection services, Action<MiniMaxOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;MiniMaxOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L109)
