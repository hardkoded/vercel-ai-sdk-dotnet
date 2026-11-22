---
title: "GatewayAsyncJobMetadata"
description: "Metadata for an asynchronous AI Gateway job. Maps to GatewayAsyncJobMetadata."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayAsyncJobMetadata"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayAsyncJobMetadata"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayAsyncJobMetadata
```


Metadata for an asynchronous AI Gateway job. Maps to `GatewayAsyncJobMetadata`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProviderMetadata.cs#L10)

## Properties

<a id="Vercel_AI_Gateway_GatewayAsyncJobMetadata_AdditionalProperties_"></a>
<a id="Vercel_AI_Gateway_GatewayAsyncJobMetadata_AdditionalProperties"></a>

### AdditionalProperties


```csharp
public IDictionary<string, JsonElement> AdditionalProperties { get; }
```


Gets the other fields of the job metadata (the index signature of the upstream type).

**Value:** [IDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProviderMetadata.cs#L25)

<a id="Vercel_AI_Gateway_GatewayAsyncJobMetadata_JobId_"></a>
<a id="Vercel_AI_Gateway_GatewayAsyncJobMetadata_JobId"></a>

### JobId


```csharp
public string JobId { get; set; }
```


Gets or sets the job id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProviderMetadata.cs#L13)

<a id="Vercel_AI_Gateway_GatewayAsyncJobMetadata_Status_"></a>
<a id="Vercel_AI_Gateway_GatewayAsyncJobMetadata_Status"></a>

### Status


```csharp
public string Status { get; set; }
```


Gets or sets the job status.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProviderMetadata.cs#L16)

<a id="Vercel_AI_Gateway_GatewayAsyncJobMetadata_WebhookSigningSecret_"></a>
<a id="Vercel_AI_Gateway_GatewayAsyncJobMetadata_WebhookSigningSecret"></a>

### WebhookSigningSecret


```csharp
public string? WebhookSigningSecret { get; set; }
```


Gets or sets the secret for verifying customer webhook deliveries. This is sensitive, start-response-only
metadata and must not be logged or forwarded.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProviderMetadata.cs#L22)
