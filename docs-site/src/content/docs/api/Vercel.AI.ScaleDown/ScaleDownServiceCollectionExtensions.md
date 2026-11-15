---
title: "ScaleDownServiceCollectionExtensions"
description: "Registers ScaleDown."
editUrl: false
prev: false
next: false
sidebar:
  label: "ScaleDownServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_ScaleDown_ScaleDownServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.ScaleDown](/api/Vercel.AI.ScaleDown/)


```csharp
public static class ScaleDownServiceCollectionExtensions
```


Registers ScaleDown.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownProvider.cs#L113)

## Methods

<a id="Vercel_AI_ScaleDown_ScaleDownServiceCollectionExtensions_AddScaleDown_"></a>
<a id="Vercel_AI_ScaleDown_ScaleDownServiceCollectionExtensions_AddScaleDown_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_ScaleDown_ScaleDownProviderSettings__"></a>

### `AddScaleDown(IServiceCollection, Action<ScaleDownProviderSettings>?)`


```csharp
public static IServiceCollection AddScaleDown(this IServiceCollection services, Action<ScaleDownProviderSettings>? configure = null)
```


Registers [ScaleDownProvider](/api/Vercel.AI.ScaleDown/ScaleDownProvider/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;ScaleDownProviderSettings&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ScaleDown/ScaleDownProvider.cs#L116)
