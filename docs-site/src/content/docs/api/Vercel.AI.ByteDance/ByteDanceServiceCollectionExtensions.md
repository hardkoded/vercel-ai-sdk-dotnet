---
title: "ByteDanceServiceCollectionExtensions"
description: "Registers ByteDance."
editUrl: false
prev: false
next: false
sidebar:
  label: "ByteDanceServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_ByteDance_ByteDanceServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.ByteDance](/api/Vercel.AI.ByteDance/)


```csharp
public static class ByteDanceServiceCollectionExtensions
```


Registers ByteDance.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceProvider.cs#L69)

## Methods

<a id="Vercel_AI_ByteDance_ByteDanceServiceCollectionExtensions_AddByteDance_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceServiceCollectionExtensions_AddByteDance_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddByteDance(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddByteDance(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceProvider.cs#L72)
