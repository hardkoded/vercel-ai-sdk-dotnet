---
title: "KlingAIServiceCollectionExtensions"
description: "Registers KlingAI."
editUrl: false
prev: false
next: false
sidebar:
  label: "KlingAIServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_KlingAI_KlingAIServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.KlingAI](/api/Vercel.AI.KlingAI/)


```csharp
public static class KlingAIServiceCollectionExtensions
```


Registers KlingAI.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIProvider.cs#L119)

## Methods

<a id="Vercel_AI_KlingAI_KlingAIServiceCollectionExtensions_AddKlingAI_"></a>
<a id="Vercel_AI_KlingAI_KlingAIServiceCollectionExtensions_AddKlingAI_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_KlingAI_KlingAIOptions__"></a>

### `AddKlingAI(IServiceCollection, Action<KlingAIOptions>?)`


```csharp
public static IServiceCollection AddKlingAI(this IServiceCollection services, Action<KlingAIOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;KlingAIOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIProvider.cs#L122)
