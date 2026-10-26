---
title: "ReplicateServiceCollectionExtensions"
description: "Registers Replicate."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReplicateServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Replicate_ReplicateServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Replicate](/api/Vercel.AI.Replicate/)


```csharp
public static class ReplicateServiceCollectionExtensions
```


Registers Replicate.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateProvider.cs#L84)

## Methods

<a id="Vercel_AI_Replicate_ReplicateServiceCollectionExtensions_AddReplicate_"></a>
<a id="Vercel_AI_Replicate_ReplicateServiceCollectionExtensions_AddReplicate_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddReplicate(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddReplicate(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Replicate/ReplicateProvider.cs#L87)
