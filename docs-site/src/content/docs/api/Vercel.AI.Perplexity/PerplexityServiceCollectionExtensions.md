---
title: "PerplexityServiceCollectionExtensions"
description: "Registers PerplexityProvider."
editUrl: false
prev: false
next: false
sidebar:
  label: "PerplexityServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Perplexity_PerplexityServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Perplexity](/api/Vercel.AI.Perplexity/)


```csharp
public static class PerplexityServiceCollectionExtensions
```


Registers [PerplexityProvider](/api/Vercel.AI.Perplexity/PerplexityProvider/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityProvider.cs#L129)

## Methods

<a id="Vercel_AI_Perplexity_PerplexityServiceCollectionExtensions_AddPerplexity_"></a>
<a id="Vercel_AI_Perplexity_PerplexityServiceCollectionExtensions_AddPerplexity_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddPerplexity(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddPerplexity(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityProvider.cs#L132)
