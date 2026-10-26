---
title: "CerebrasServiceCollectionExtensions"
description: "Registers CerebrasProvider."
editUrl: false
prev: false
next: false
sidebar:
  label: "CerebrasServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Cerebras_CerebrasServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Cerebras](/api/Vercel.AI.Cerebras/)


```csharp
public static class CerebrasServiceCollectionExtensions
```


Registers [CerebrasProvider](/api/Vercel.AI.Cerebras/CerebrasProvider/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasProvider.cs#L69)

## Methods

<a id="Vercel_AI_Cerebras_CerebrasServiceCollectionExtensions_AddCerebras_"></a>
<a id="Vercel_AI_Cerebras_CerebrasServiceCollectionExtensions_AddCerebras_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddCerebras(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddCerebras(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasProvider.cs#L72)
