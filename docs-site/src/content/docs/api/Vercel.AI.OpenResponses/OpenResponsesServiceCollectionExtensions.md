---
title: "OpenResponsesServiceCollectionExtensions"
description: "Registers Open Responses."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenResponsesServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_OpenResponses_OpenResponsesServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.OpenResponses](/api/Vercel.AI.OpenResponses/)


```csharp
public static class OpenResponsesServiceCollectionExtensions
```


Registers Open Responses.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L591)

## Methods

<a id="Vercel_AI_OpenResponses_OpenResponsesServiceCollectionExtensions_AddOpenResponses_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesServiceCollectionExtensions_AddOpenResponses_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenResponses_OpenResponsesOptions__"></a>

### `AddOpenResponses(IServiceCollection, Action<OpenResponsesOptions>?)`


```csharp
public static IServiceCollection AddOpenResponses(this IServiceCollection services, Action<OpenResponsesOptions>? configure = null)
```


Adds [OpenResponsesProvider](/api/Vercel.AI.OpenResponses/OpenResponsesProvider/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenResponsesOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L594)
