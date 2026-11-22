---
title: "GatewayProviderMetadata"
description: "Shape of the inner providerMetadata.gateway object returned by AI Gateway operations. Maps to GatewayProviderMetadata. Additional fields may be added without..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayProviderMetadata"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayProviderMetadata"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayProviderMetadata
```


Shape of the inner `providerMetadata.gateway` object returned by AI Gateway operations. Maps to
`GatewayProviderMetadata`. Additional fields may be added without a package update.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProviderMetadata.cs#L33)

## Properties

<a id="Vercel_AI_Gateway_GatewayProviderMetadata_AdditionalProperties_"></a>
<a id="Vercel_AI_Gateway_GatewayProviderMetadata_AdditionalProperties"></a>

### AdditionalProperties


```csharp
public IDictionary<string, JsonElement> AdditionalProperties { get; }
```


Gets the other fields of the gateway metadata (the index signature of the upstream type).

**Value:** [IDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProviderMetadata.cs#L39)

<a id="Vercel_AI_Gateway_GatewayProviderMetadata_AsyncJob_"></a>
<a id="Vercel_AI_Gateway_GatewayProviderMetadata_AsyncJob"></a>

### AsyncJob


```csharp
public GatewayAsyncJobMetadata? AsyncJob { get; set; }
```


Gets or sets the asynchronous job metadata.

**Value:** [GatewayAsyncJobMetadata](/api/Vercel.AI.Gateway/GatewayAsyncJobMetadata/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProviderMetadata.cs#L36)

## Methods

<a id="Vercel_AI_Gateway_GatewayProviderMetadata_From_"></a>
<a id="Vercel_AI_Gateway_GatewayProviderMetadata_From_System_Nullable_System_Text_Json_JsonElement__"></a>

### From(JsonElement?)


```csharp
public static GatewayProviderMetadata? From(JsonElement? providerMetadata)
```


Reads the `gateway` entry of a provider metadata value, or returns `null` when it has none.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [GatewayProviderMetadata](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProviderMetadata.cs#L45)
