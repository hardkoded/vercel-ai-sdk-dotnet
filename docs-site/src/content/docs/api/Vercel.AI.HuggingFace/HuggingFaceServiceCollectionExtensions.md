---
title: "HuggingFaceServiceCollectionExtensions"
description: "Registers HuggingFaceProvider."
editUrl: false
prev: false
next: false
sidebar:
  label: "HuggingFaceServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_HuggingFace_HuggingFaceServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.HuggingFace](/api/Vercel.AI.HuggingFace/)


```csharp
public static class HuggingFaceServiceCollectionExtensions
```


Registers [HuggingFaceProvider](/api/Vercel.AI.HuggingFace/HuggingFaceProvider/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceProvider.cs#L84)

## Methods

<a id="Vercel_AI_HuggingFace_HuggingFaceServiceCollectionExtensions_AddHuggingFace_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceServiceCollectionExtensions_AddHuggingFace_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddHuggingFace(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddHuggingFace(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceProvider.cs#L87)
