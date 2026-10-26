---
title: "AmazonBedrockServiceCollectionExtensions"
description: "Registers Bedrock."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockServiceCollectionExtensions"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockServiceCollectionExtensions"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public static class AmazonBedrockServiceCollectionExtensions
```


Registers Bedrock.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L1381)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockServiceCollectionExtensions_AddAmazonBedrock_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockServiceCollectionExtensions_AddAmazonBedrock_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Vercel_AI_AmazonBedrock_AmazonBedrockOptions__"></a>

### `AddAmazonBedrock(IServiceCollection, Action<AmazonBedrockOptions>?)`


```csharp
public static IServiceCollection AddAmazonBedrock(this IServiceCollection services, Action<AmazonBedrockOptions>? configure = null)
```


Adds [AmazonBedrockProvider](/api/Vercel.AI.AmazonBedrock/AmazonBedrockProvider/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `services` | [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection) |  |
| `configure` | [Action&lt;AmazonBedrockOptions&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

**Returns:** [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockProvider.cs#L1384)
