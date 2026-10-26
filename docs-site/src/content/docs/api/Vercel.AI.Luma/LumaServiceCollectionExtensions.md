---
title: "LumaServiceCollectionExtensions"
description: "Registers Luma."
editUrl: false
prev: false
next: false
sidebar:
  label: "LumaServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Luma_LumaServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Luma](/api/Vercel.AI.Luma/)


```csharp
public static class LumaServiceCollectionExtensions
```


Registers Luma.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaProvider.cs#L82)

## Methods

<a id="Vercel_AI_Luma_LumaServiceCollectionExtensions_AddLuma_"></a>
<a id="Vercel_AI_Luma_LumaServiceCollectionExtensions_AddLuma_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddLuma(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddLuma(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaProvider.cs#L85)
