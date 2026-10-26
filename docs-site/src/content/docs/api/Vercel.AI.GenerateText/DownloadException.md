---
title: "DownloadException"
description: "A download failed or the URL is not safe to fetch. Maps to DownloadError."
editUrl: false
prev: false
next: false
sidebar:
  label: "DownloadException"
  hidden: true
---

<a id="Vercel_AI_GenerateText_DownloadException"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class DownloadException : AiSdkException, ISerializable
```


A download failed or the URL is not safe to fetch. Maps to `DownloadError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/DownloadUrl.cs#L29)

## Constructors

<a id="Vercel_AI_GenerateText_DownloadException__ctor_"></a>
<a id="Vercel_AI_GenerateText_DownloadException__ctor_System_String_System_String_System_Nullable_System_Int32__"></a>

### DownloadException(string, string, int?)


```csharp
public DownloadException(string url, string message, int? statusCode = null)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/DownloadUrl.cs#L32)

## Properties

<a id="Vercel_AI_GenerateText_DownloadException_Name_"></a>
<a id="Vercel_AI_GenerateText_DownloadException_Name"></a>

### Name


```csharp
public string Name { get; }
```


Stable error name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/DownloadUrl.cs#L40)

<a id="Vercel_AI_GenerateText_DownloadException_StatusCode_"></a>
<a id="Vercel_AI_GenerateText_DownloadException_StatusCode"></a>

### StatusCode


```csharp
public int? StatusCode { get; }
```


HTTP status, when the failure came from a response.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/DownloadUrl.cs#L46)

<a id="Vercel_AI_GenerateText_DownloadException_Url_"></a>
<a id="Vercel_AI_GenerateText_DownloadException_Url"></a>

### Url


```csharp
public string Url { get; }
```


URL that was rejected or that failed.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/DownloadUrl.cs#L43)
