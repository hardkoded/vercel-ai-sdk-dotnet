---
title: "XaiServiceCollectionExtensions"
description: "Registers XaiProvider."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public static class XaiServiceCollectionExtensions
```


Registers [XaiProvider](/api/Vercel.AI.Xai/XaiProvider/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiProvider.cs#L106)

## Methods

<a id="Vercel_AI_Xai_XaiServiceCollectionExtensions_AddXai_"></a>
<a id="Vercel_AI_Xai_XaiServiceCollectionExtensions_AddXai_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddXai(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddXai(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiProvider.cs#L109)
