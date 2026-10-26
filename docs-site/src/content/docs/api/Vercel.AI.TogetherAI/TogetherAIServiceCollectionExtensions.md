---
title: "TogetherAIServiceCollectionExtensions"
description: "Registers TogetherAIProvider."
editUrl: false
prev: false
next: false
sidebar:
  label: "TogetherAIServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_TogetherAI_TogetherAIServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.TogetherAI](/api/Vercel.AI.TogetherAI/)


```csharp
public static class TogetherAIServiceCollectionExtensions
```


Registers [TogetherAIProvider](/api/Vercel.AI.TogetherAI/TogetherAIProvider/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIProvider.cs#L137)

## Methods

<a id="Vercel_AI_TogetherAI_TogetherAIServiceCollectionExtensions_AddTogetherAI_"></a>
<a id="Vercel_AI_TogetherAI_TogetherAIServiceCollectionExtensions_AddTogetherAI_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddTogetherAI(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddTogetherAI(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIProvider.cs#L140)
