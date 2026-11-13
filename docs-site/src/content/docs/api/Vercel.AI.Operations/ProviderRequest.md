---
title: "ProviderRequest"
description: "Request metadata returned by a provider call."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProviderRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_ProviderRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ProviderRequest
```


Request metadata returned by a provider call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L366)

## Constructors

<a id="Vercel_AI_Operations_ProviderRequest__ctor_"></a>
<a id="Vercel_AI_Operations_ProviderRequest__ctor_System_String_"></a>

### ProviderRequest(string?)


```csharp
public ProviderRequest(string? body = null)
```


Creates request metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L369)

## Properties

<a id="Vercel_AI_Operations_ProviderRequest_Body_"></a>
<a id="Vercel_AI_Operations_ProviderRequest_Body"></a>

### Body


```csharp
public string? Body { get; }
```


Request body, or the request URL for a WebSocket call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L375)
