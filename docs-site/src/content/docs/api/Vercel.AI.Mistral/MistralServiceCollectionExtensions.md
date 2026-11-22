---
title: "MistralServiceCollectionExtensions"
description: "Registers MistralProvider."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public static class MistralServiceCollectionExtensions
```


Registers [MistralProvider](/api/Vercel.AI.Mistral/MistralProvider/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralProvider.cs#L172)

## Methods

<a id="Vercel_AI_Mistral_MistralServiceCollectionExtensions_AddMistral_"></a>
<a id="Vercel_AI_Mistral_MistralServiceCollectionExtensions_AddMistral_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_Mistral_MistralOptions__"></a>

### `AddMistral(IServiceCollection, Action<MistralOptions>?)`


```csharp
public static IServiceCollection AddMistral(this IServiceCollection services, Action<MistralOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;MistralOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralProvider.cs#L175)
