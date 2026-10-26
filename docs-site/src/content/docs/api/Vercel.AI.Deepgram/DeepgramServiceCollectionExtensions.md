---
title: "DeepgramServiceCollectionExtensions"
description: "Registers Deepgram."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepgramServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Deepgram_DeepgramServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Deepgram](/api/Vercel.AI.Deepgram/)


```csharp
public static class DeepgramServiceCollectionExtensions
```


Registers Deepgram.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L808)

## Methods

<a id="Vercel_AI_Deepgram_DeepgramServiceCollectionExtensions_AddDeepgram_"></a>
<a id="Vercel_AI_Deepgram_DeepgramServiceCollectionExtensions_AddDeepgram_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddDeepgram(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddDeepgram(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L811)
