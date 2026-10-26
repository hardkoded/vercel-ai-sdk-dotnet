---
title: "AzureOpenAIServiceCollectionExtensions"
description: "Registers Azure OpenAI."
editUrl: false
prev: false
next: false
sidebar:
  label: "AzureOpenAIServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Azure_AzureOpenAIServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Azure](/api/Vercel.AI.Azure/)


```csharp
public static class AzureOpenAIServiceCollectionExtensions
```


Registers Azure OpenAI.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L221)

## Methods

<a id="Vercel_AI_Azure_AzureOpenAIServiceCollectionExtensions_AddAzureOpenAI_"></a>
<a id="Vercel_AI_Azure_AzureOpenAIServiceCollectionExtensions_AddAzureOpenAI_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_Azure_AzureOpenAIOptions__"></a>

### `AddAzureOpenAI(IServiceCollection, Action<AzureOpenAIOptions>?)`


```csharp
public static IServiceCollection AddAzureOpenAI(this IServiceCollection services, Action<AzureOpenAIOptions>? configure = null)
```


Adds [AzureOpenAIProvider](/api/Vercel.AI.Azure/AzureOpenAIProvider/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;AzureOpenAIOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAIProvider.cs#L224)
