---
title: "DeepInfraServiceCollectionExtensions"
description: "Registers DeepInfraProvider."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepInfraServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_DeepInfra_DeepInfraServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.DeepInfra](/api/Vercel.AI.DeepInfra/)


```csharp
public static class DeepInfraServiceCollectionExtensions
```


Registers [DeepInfraProvider](/api/Vercel.AI.DeepInfra/DeepInfraProvider/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepInfra/DeepInfraProvider.cs#L114)

## Methods

<a id="Vercel_AI_DeepInfra_DeepInfraServiceCollectionExtensions_AddDeepInfra_"></a>
<a id="Vercel_AI_DeepInfra_DeepInfraServiceCollectionExtensions_AddDeepInfra_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddDeepInfra(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddDeepInfra(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepInfra/DeepInfraProvider.cs#L117)
