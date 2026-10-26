---
title: "BlackForestLabsServiceCollectionExtensions"
description: "Registers BlackForestLabs."
editUrl: false
prev: false
next: false
sidebar:
  label: "BlackForestLabsServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.BlackForestLabs](/api/Vercel.AI.BlackForestLabs/)


```csharp
public static class BlackForestLabsServiceCollectionExtensions
```


Registers BlackForestLabs.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsProvider.cs#L139)

## Methods

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsServiceCollectionExtensions_AddBlackForestLabs_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsServiceCollectionExtensions_AddBlackForestLabs_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddBlackForestLabs(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddBlackForestLabs(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsProvider.cs#L142)
