---
title: "DeepSeekServiceCollectionExtensions"
description: "Registers DeepSeekProvider."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_DeepSeek_DeepSeekServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.DeepSeek](/api/Vercel.AI.DeepSeek/)


```csharp
public static class DeepSeekServiceCollectionExtensions
```


Registers [DeepSeekProvider](/api/Vercel.AI.DeepSeek/DeepSeekProvider/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekProvider.cs#L73)

## Methods

<a id="Vercel_AI_DeepSeek_DeepSeekServiceCollectionExtensions_AddDeepSeek_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekServiceCollectionExtensions_AddDeepSeek_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddDeepSeek(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddDeepSeek(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekProvider.cs#L76)
