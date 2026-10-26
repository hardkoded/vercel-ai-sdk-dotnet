---
title: "ElevenLabsServiceCollectionExtensions"
description: "Registers ElevenLabs."
editUrl: false
prev: false
next: false
sidebar:
  label: "ElevenLabsServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_ElevenLabs_ElevenLabsServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.ElevenLabs](/api/Vercel.AI.ElevenLabs/)


```csharp
public static class ElevenLabsServiceCollectionExtensions
```


Registers ElevenLabs.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L576)

## Methods

<a id="Vercel_AI_ElevenLabs_ElevenLabsServiceCollectionExtensions_AddElevenLabs_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsServiceCollectionExtensions_AddElevenLabs_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddElevenLabs(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddElevenLabs(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L579)
