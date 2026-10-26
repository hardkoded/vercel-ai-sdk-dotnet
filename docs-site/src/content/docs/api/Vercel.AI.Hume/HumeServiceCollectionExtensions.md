---
title: "HumeServiceCollectionExtensions"
description: "Registers Hume."
editUrl: false
prev: false
next: false
sidebar:
  label: "HumeServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Hume_HumeServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Hume](/api/Vercel.AI.Hume/)


```csharp
public static class HumeServiceCollectionExtensions
```


Registers Hume.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L325)

## Methods

<a id="Vercel_AI_Hume_HumeServiceCollectionExtensions_AddHume_"></a>
<a id="Vercel_AI_Hume_HumeServiceCollectionExtensions_AddHume_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddHume(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddHume(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L328)
