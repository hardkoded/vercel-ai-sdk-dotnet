---
title: "AnthropicServiceCollectionExtensions"
description: "Registers Anthropic."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public static class AnthropicServiceCollectionExtensions
```


Registers Anthropic.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L660)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicServiceCollectionExtensions_AddAnthropic_"></a>
<a id="Vercel_AI_Anthropic_AnthropicServiceCollectionExtensions_AddAnthropic_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_Anthropic_AnthropicOptions__"></a>

### `AddAnthropic(IServiceCollection, Action<AnthropicOptions>?)`


```csharp
public static IServiceCollection AddAnthropic(this IServiceCollection services, Action<AnthropicOptions>? configure = null)
```


Adds [AnthropicProvider](/api/Vercel.AI.Anthropic/AnthropicProvider/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;AnthropicOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L663)

<a id="Vercel_AI_Anthropic_AnthropicServiceCollectionExtensions_AddAnthropicAws_"></a>
<a id="Vercel_AI_Anthropic_AnthropicServiceCollectionExtensions_AddAnthropicAws_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_Anthropic_AnthropicOptions__"></a>

### `AddAnthropicAws(IServiceCollection, Action<AnthropicOptions>?)`


```csharp
public static IServiceCollection AddAnthropicAws(this IServiceCollection services, Action<AnthropicOptions>? configure = null)
```


Adds [AnthropicAwsProvider](/api/Vercel.AI.Anthropic/AnthropicAwsProvider/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;AnthropicOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicProvider.cs#L676)
