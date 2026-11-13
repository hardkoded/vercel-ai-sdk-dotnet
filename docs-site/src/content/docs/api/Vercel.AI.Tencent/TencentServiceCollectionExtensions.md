---
title: "TencentServiceCollectionExtensions"
description: "Registers Tencent."
editUrl: false
prev: false
next: false
sidebar:
  label: "TencentServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Tencent_TencentServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Tencent](/api/Vercel.AI.Tencent/)


```csharp
public static class TencentServiceCollectionExtensions
```


Registers Tencent.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L188)

## Methods

<a id="Vercel_AI_Tencent_TencentServiceCollectionExtensions_AddTencent_"></a>
<a id="Vercel_AI_Tencent_TencentServiceCollectionExtensions_AddTencent_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_Tencent_TencentOptions__"></a>

### `AddTencent(IServiceCollection, Action<TencentOptions>?)`


```csharp
public static IServiceCollection AddTencent(this IServiceCollection services, Action<TencentOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;TencentOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L191)
