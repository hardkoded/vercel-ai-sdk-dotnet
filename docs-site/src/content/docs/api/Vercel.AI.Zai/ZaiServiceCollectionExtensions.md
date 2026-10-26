---
title: "ZaiServiceCollectionExtensions"
description: "Registers ZaiProvider."
editUrl: false
prev: false
next: false
sidebar:
  label: "ZaiServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Zai_ZaiServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Zai](/api/Vercel.AI.Zai/)


```csharp
public static class ZaiServiceCollectionExtensions
```


Registers [ZaiProvider](/api/Vercel.AI.Zai/ZaiProvider/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiProvider.cs#L68)

## Methods

<a id="Vercel_AI_Zai_ZaiServiceCollectionExtensions_AddZai_"></a>
<a id="Vercel_AI_Zai_ZaiServiceCollectionExtensions_AddZai_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddZai(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddZai(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiProvider.cs#L71)
