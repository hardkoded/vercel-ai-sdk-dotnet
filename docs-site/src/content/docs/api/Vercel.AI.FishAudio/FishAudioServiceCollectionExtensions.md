---
title: "FishAudioServiceCollectionExtensions"
description: "Registers FishAudio."
editUrl: false
prev: false
next: false
sidebar:
  label: "FishAudioServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_FishAudio_FishAudioServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.FishAudio](/api/Vercel.AI.FishAudio/)


```csharp
public static class FishAudioServiceCollectionExtensions
```


Registers FishAudio.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L395)

## Methods

<a id="Vercel_AI_FishAudio_FishAudioServiceCollectionExtensions_AddFishAudio_"></a>
<a id="Vercel_AI_FishAudio_FishAudioServiceCollectionExtensions_AddFishAudio_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddFishAudio(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddFishAudio(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L398)
