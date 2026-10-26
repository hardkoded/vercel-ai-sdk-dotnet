---
title: "GoogleServiceCollectionExtensions"
description: "Registers Google and Vertex."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleServiceCollectionExtensions
```


Registers Google and Vertex.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L342)

## Methods

<a id="Vercel_AI_Google_GoogleServiceCollectionExtensions_AddGoogle_"></a>
<a id="Vercel_AI_Google_GoogleServiceCollectionExtensions_AddGoogle_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_Google_GoogleOptions__"></a>

### `AddGoogle(IServiceCollection, Action<GoogleOptions>?)`


```csharp
public static IServiceCollection AddGoogle(this IServiceCollection services, Action<GoogleOptions>? configure = null)
```


Adds [GoogleProvider](/api/Vercel.AI.Google/GoogleProvider/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;GoogleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L345)

<a id="Vercel_AI_Google_GoogleServiceCollectionExtensions_AddGoogleVertex_"></a>
<a id="Vercel_AI_Google_GoogleServiceCollectionExtensions_AddGoogleVertex_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_Google_VertexOptions__"></a>

### `AddGoogleVertex(IServiceCollection, Action<VertexOptions>?)`


```csharp
public static IServiceCollection AddGoogleVertex(this IServiceCollection services, Action<VertexOptions>? configure = null)
```


Adds [GoogleVertexProvider](/api/Vercel.AI.Google/GoogleVertexProvider/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;VertexOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L358)
