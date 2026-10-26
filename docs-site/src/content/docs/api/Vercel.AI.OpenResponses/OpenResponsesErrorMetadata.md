---
title: "OpenResponsesErrorMetadata"
description: "Status and retry metadata read from an endpoint-specific response error."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenResponsesErrorMetadata"
  hidden: true
---

<a id="Vercel_AI_OpenResponses_OpenResponsesErrorMetadata"></a>

Namespace: [Vercel.AI.OpenResponses](/api/Vercel.AI.OpenResponses/)


```csharp
public sealed class OpenResponsesErrorMetadata
```


Status and retry metadata read from an endpoint-specific response error.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L51)

## Properties

<a id="Vercel_AI_OpenResponses_OpenResponsesErrorMetadata_IsRetryable_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesErrorMetadata_IsRetryable"></a>

### IsRetryable


```csharp
public bool? IsRetryable { get; set; }
```


Whether the call can be retried.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L57)

<a id="Vercel_AI_OpenResponses_OpenResponsesErrorMetadata_StatusCode_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesErrorMetadata_StatusCode"></a>

### StatusCode


```csharp
public int? StatusCode { get; set; }
```


HTTP status of the error.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesProvider.cs#L54)
