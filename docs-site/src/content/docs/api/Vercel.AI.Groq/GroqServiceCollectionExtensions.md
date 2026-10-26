---
title: "GroqServiceCollectionExtensions"
description: "Registers GroqProvider."
editUrl: false
prev: false
next: false
sidebar:
  label: "GroqServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Groq_GroqServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Groq](/api/Vercel.AI.Groq/)


```csharp
public static class GroqServiceCollectionExtensions
```


Registers [GroqProvider](/api/Vercel.AI.Groq/GroqProvider/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Groq/GroqProvider.cs#L69)

## Methods

<a id="Vercel_AI_Groq_GroqServiceCollectionExtensions_AddGroq_"></a>
<a id="Vercel_AI_Groq_GroqServiceCollectionExtensions_AddGroq_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddGroq(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddGroq(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Groq/GroqProvider.cs#L72)
