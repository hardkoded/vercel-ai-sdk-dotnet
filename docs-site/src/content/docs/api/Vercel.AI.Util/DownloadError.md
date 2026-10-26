---
title: "DownloadError"
description: "Failure while downloading a URL."
editUrl: false
prev: false
next: false
sidebar:
  label: "DownloadError"
  hidden: true
---

<a id="Vercel_AI_Util_DownloadError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class DownloadError : AiSdkError, ISerializable, IErrorMarkers
```


Failure while downloading a URL.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L13)

## Constructors

<a id="Vercel_AI_Util_DownloadError__ctor_"></a>
<a id="Vercel_AI_Util_DownloadError__ctor_System_String_System_String_System_Nullable_System_Int32__System_String_System_Exception_"></a>

### DownloadError(string, string, int?, string?, Exception?)


```csharp
public DownloadError(string url, string message, int? statusCode = null, string? statusText = null, Exception? cause = null)
```


Creates a download error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `statusText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L19)

## Fields

<a id="Vercel_AI_Util_DownloadError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_DownloadError"
```


Marker for [IsInstance(object)](#Vercel_AI_Util_DownloadError_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L16)

## Properties

<a id="Vercel_AI_Util_DownloadError_StatusCode_"></a>
<a id="Vercel_AI_Util_DownloadError_StatusCode"></a>

### StatusCode


```csharp
public int? StatusCode { get; }
```


HTTP status, when the response was received.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L32)

<a id="Vercel_AI_Util_DownloadError_StatusText_"></a>
<a id="Vercel_AI_Util_DownloadError_StatusText"></a>

### StatusText


```csharp
public string? StatusText { get; }
```


HTTP status text, when the response was received.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L35)

<a id="Vercel_AI_Util_DownloadError_Url_"></a>
<a id="Vercel_AI_Util_DownloadError_Url"></a>

### Url


```csharp
public string Url { get; }
```


URL that failed.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L29)

## Methods

<a id="Vercel_AI_Util_DownloadError_FromCause_"></a>
<a id="Vercel_AI_Util_DownloadError_FromCause_System_String_System_Exception_"></a>

### FromCause(string, Exception)


```csharp
public static DownloadError FromCause(string url, Exception cause)
```


Builds the message used when the transport throws.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

**Returns:** [DownloadError](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L50)

<a id="Vercel_AI_Util_DownloadError_FromStatus_"></a>
<a id="Vercel_AI_Util_DownloadError_FromStatus_System_String_System_Int32_System_String_"></a>

### FromStatus(string, int, string)


```csharp
public static DownloadError FromStatus(string url, int statusCode, string statusText)
```


Builds the message used for a non-success HTTP response.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `statusText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [DownloadError](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L44)

<a id="Vercel_AI_Util_DownloadError_IsInstance_"></a>
<a id="Vercel_AI_Util_DownloadError_IsInstance_System_Object_"></a>

### IsInstance(object?)


```csharp
public static bool IsInstance(object? error)
```


Returns whether &lt;code class="paramref"&gt;error&lt;/code&gt; carries this error's marker.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L38)
