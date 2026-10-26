---
title: "AssemblyAIServiceCollectionExtensions"
description: "Registers AssemblyAI."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAIServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAIServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public static class AssemblyAIServiceCollectionExtensions
```


Registers AssemblyAI.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L555)

## Methods

<a id="Vercel_AI_AssemblyAI_AssemblyAIServiceCollectionExtensions_AddAssemblyAI_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIServiceCollectionExtensions_AddAssemblyAI_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddAssemblyAI(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddAssemblyAI(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L558)
