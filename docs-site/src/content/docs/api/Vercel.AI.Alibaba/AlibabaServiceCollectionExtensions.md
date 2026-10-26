---
title: "AlibabaServiceCollectionExtensions"
description: "Registers AlibabaProvider."
editUrl: false
prev: false
next: false
sidebar:
  label: "AlibabaServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Alibaba_AlibabaServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Alibaba](/api/Vercel.AI.Alibaba/)


```csharp
public static class AlibabaServiceCollectionExtensions
```


Registers [AlibabaProvider](/api/Vercel.AI.Alibaba/AlibabaProvider/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L291)

## Methods

<a id="Vercel_AI_Alibaba_AlibabaServiceCollectionExtensions_AddAlibaba_"></a>
<a id="Vercel_AI_Alibaba_AlibabaServiceCollectionExtensions_AddAlibaba_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions__"></a>

### `AddAlibaba(IServiceCollection, Action<OpenAICompatibleOptions>?)`


```csharp
public static IServiceCollection AddAlibaba(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
```


Adds the provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;OpenAICompatibleOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L294)
