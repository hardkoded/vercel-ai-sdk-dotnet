---
title: "MoonshotServiceCollectionExtensions"
description: "Registers MoonshotProvider."
editUrl: false
prev: false
next: false
sidebar:
  label: "MoonshotServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Moonshot_MoonshotServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Moonshot](/api/Vercel.AI.Moonshot/)


```csharp
public static class MoonshotServiceCollectionExtensions
```


Registers [MoonshotProvider](/api/Vercel.AI.Moonshot/MoonshotProvider/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Moonshot/MoonshotProvider.cs#L87)

## Methods

<a id="Vercel_AI_Moonshot_MoonshotServiceCollectionExtensions_AddMoonshot_"></a>
<a id="Vercel_AI_Moonshot_MoonshotServiceCollectionExtensions_AddMoonshot_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddMoonshot(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddMoonshot(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Moonshot/MoonshotProvider.cs#L90)
