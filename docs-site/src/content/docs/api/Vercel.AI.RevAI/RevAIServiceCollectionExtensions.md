---
title: "RevAIServiceCollectionExtensions"
description: "Registers Rev.ai."
editUrl: false
prev: false
next: false
sidebar:
  label: "RevAIServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_RevAI_RevAIServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.RevAI](/api/Vercel.AI.RevAI/)


```csharp
public static class RevAIServiceCollectionExtensions
```


Registers Rev.ai.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L549)

## Methods

<a id="Vercel_AI_RevAI_RevAIServiceCollectionExtensions_AddRevAI_"></a>
<a id="Vercel_AI_RevAI_RevAIServiceCollectionExtensions_AddRevAI_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddRevAI(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddRevAI(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L552)
