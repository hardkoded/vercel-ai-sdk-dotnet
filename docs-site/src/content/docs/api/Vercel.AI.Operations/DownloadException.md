---
title: "DownloadException"
description: "Downloading generated media failed."
editUrl: false
prev: false
next: false
sidebar:
  label: "DownloadException"
  hidden: true
---

<a id="Vercel_AI_Operations_DownloadException"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class DownloadException : AiSdkException, ISerializable
```


Downloading generated media failed.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L309)

## Constructors

<a id="Vercel_AI_Operations_DownloadException__ctor_"></a>
<a id="Vercel_AI_Operations_DownloadException__ctor_System_String_System_Int32_System_String_"></a>

### DownloadException(string, int, string?)


```csharp
public DownloadException(string url, int statusCode, string? statusText)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `statusText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L312)

## Properties

<a id="Vercel_AI_Operations_DownloadException_StatusCode_"></a>
<a id="Vercel_AI_Operations_DownloadException_StatusCode"></a>

### StatusCode


```csharp
public int StatusCode { get; }
```


HTTP status.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L323)

<a id="Vercel_AI_Operations_DownloadException_Url_"></a>
<a id="Vercel_AI_Operations_DownloadException_Url"></a>

### Url


```csharp
public string Url { get; }
```


URL that failed.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L320)
