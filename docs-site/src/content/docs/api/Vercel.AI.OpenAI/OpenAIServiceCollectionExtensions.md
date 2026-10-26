---
title: "OpenAIServiceCollectionExtensions"
description: "Registers OpenAIProvider."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public static class OpenAIServiceCollectionExtensions
```


Registers [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L516)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIServiceCollectionExtensions_AddOpenAI_"></a>
<a id="Vercel_AI_OpenAI_OpenAIServiceCollectionExtensions_AddOpenAI_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAI_OpenAIOptions__"></a>

### `AddOpenAI(IServiceCollection, Action<OpenAIOptions>?)`


```csharp
public static IServiceCollection AddOpenAI(this IServiceCollection services, Action<OpenAIOptions>? configure = null)
```


Adds the OpenAI provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAIOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L519)
