---
title: "AiSdkServiceCollectionExtensions"
description: "Dependency injection for IAiClient."
editUrl: false
prev: false
next: false
sidebar:
  label: "AiSdkServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_AiSdkServiceCollectionExtensions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public static class AiSdkServiceCollectionExtensions
```


Dependency injection for [IAiClient](/api/Vercel.AI/IAiClient/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L548)

## Fields

<a id="Vercel_AI_AiSdkServiceCollectionExtensions_HttpClientName"></a>

### HttpClientName


```csharp
public const string HttpClientName = "Vercel.AI"
```


Named HTTP client used by `AiSdkOptions%7d)`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L551)

## Methods

<a id="Vercel_AI_AiSdkServiceCollectionExtensions_AddAiSdk_"></a>
<a id="Vercel_AI_AiSdkServiceCollectionExtensions_AddAiSdk_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_AiSdkOptions__"></a>

### `AddAiSdk(IServiceCollection, Action<AiSdkOptions>?)`


```csharp
public static IServiceCollection AddAiSdk(this IServiceCollection services, Action<AiSdkOptions>? configure = null)
```


Registers [IAiClient](/api/Vercel.AI/IAiClient/) and the Gateway HTTP client.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;AiSdkOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L554)
