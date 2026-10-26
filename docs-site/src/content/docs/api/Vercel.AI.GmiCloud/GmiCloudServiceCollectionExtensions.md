---
title: "GmiCloudServiceCollectionExtensions"
description: "Registers GmiCloudProvider."
editUrl: false
prev: false
next: false
sidebar:
  label: "GmiCloudServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_GmiCloud_GmiCloudServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.GmiCloud](/api/Vercel.AI.GmiCloud/)


```csharp
public static class GmiCloudServiceCollectionExtensions
```


Registers [GmiCloudProvider](/api/Vercel.AI.GmiCloud/GmiCloudProvider/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GmiCloud/GmiCloudProvider.cs#L67)

## Methods

<a id="Vercel_AI_GmiCloud_GmiCloudServiceCollectionExtensions_AddGmiCloud_"></a>
<a id="Vercel_AI_GmiCloud_GmiCloudServiceCollectionExtensions_AddGmiCloud_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddGmiCloud(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddGmiCloud(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GmiCloud/GmiCloudProvider.cs#L70)
