---
title: "FalServiceCollectionExtensions"
description: "Registers Fal."
editUrl: false
prev: false
next: false
sidebar:
  label: "FalServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Fal_FalServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Fal](/api/Vercel.AI.Fal/)


```csharp
public static class FalServiceCollectionExtensions
```


Registers Fal.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalProvider.cs#L105)

## Methods

<a id="Vercel_AI_Fal_FalServiceCollectionExtensions_AddFal_"></a>
<a id="Vercel_AI_Fal_FalServiceCollectionExtensions_AddFal_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddFal(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddFal(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalProvider.cs#L108)
