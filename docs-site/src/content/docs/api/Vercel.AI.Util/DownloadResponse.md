---
title: "DownloadResponse"
description: "One HTTP response seen by Download."
editUrl: false
prev: false
next: false
sidebar:
  label: "DownloadResponse"
  hidden: true
---

<a id="Vercel_AI_Util_DownloadResponse"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class DownloadResponse
```


One HTTP response seen by [Download](/api/Vercel.AI.Util/Download/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L110)

## Constructors

<a id="Vercel_AI_Util_DownloadResponse__ctor_"></a>
<a id="Vercel_AI_Util_DownloadResponse__ctor_System_Int32_System_String_Vercel_AI_Util_HeaderCollection_System_Byte___System_String_"></a>

### DownloadResponse(int, string?, HeaderCollection?, byte[]?, string?)


```csharp
public DownloadResponse(int status, string? statusText, HeaderCollection? headers, byte[]? body, string? type = "basic")
```


Creates a response.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `status` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `statusText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/) |  |
| `body` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L113)

## Properties

<a id="Vercel_AI_Util_DownloadResponse_Body_"></a>
<a id="Vercel_AI_Util_DownloadResponse_Body"></a>

### Body


```csharp
public byte[]? Body { get; }
```


Response body. May be null.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L132)

<a id="Vercel_AI_Util_DownloadResponse_Cancelled_"></a>
<a id="Vercel_AI_Util_DownloadResponse_Cancelled"></a>

### Cancelled


```csharp
public bool Cancelled { get; }
```


Whether [Cancel()](#Vercel_AI_Util_DownloadResponse_Cancel) has run.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L147)

<a id="Vercel_AI_Util_DownloadResponse_Headers_"></a>
<a id="Vercel_AI_Util_DownloadResponse_Headers"></a>

### Headers


```csharp
public HeaderCollection Headers { get; }
```


Response headers.

**Value:** [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L129)

<a id="Vercel_AI_Util_DownloadResponse_Ok_"></a>
<a id="Vercel_AI_Util_DownloadResponse_Ok"></a>

### Ok


```csharp
public bool Ok { get; }
```


Whether the status is 200-299.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L138)

<a id="Vercel_AI_Util_DownloadResponse_OnCancel_"></a>
<a id="Vercel_AI_Util_DownloadResponse_OnCancel"></a>

### OnCancel


```csharp
public Action? OnCancel { get; set; }
```


Called when the body is cancelled.

**Value:** [Action](https://learn.microsoft.com/dotnet/api/system.action)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L144)

<a id="Vercel_AI_Util_DownloadResponse_Status_"></a>
<a id="Vercel_AI_Util_DownloadResponse_Status"></a>

### Status


```csharp
public int Status { get; }
```


HTTP status.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L123)

<a id="Vercel_AI_Util_DownloadResponse_StatusText_"></a>
<a id="Vercel_AI_Util_DownloadResponse_StatusText"></a>

### StatusText


```csharp
public string StatusText { get; }
```


HTTP status text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L126)

<a id="Vercel_AI_Util_DownloadResponse_Type_"></a>
<a id="Vercel_AI_Util_DownloadResponse_Type"></a>

### Type


```csharp
public string Type { get; }
```


`basic` or `opaqueredirect`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L135)

## Methods

<a id="Vercel_AI_Util_DownloadResponse_Bytes_"></a>
<a id="Vercel_AI_Util_DownloadResponse_Bytes_System_Byte___System_String_System_Int32_"></a>

### Bytes(byte[]?, string?, int)


```csharp
public static DownloadResponse Bytes(byte[]? body, string? mediaType, int status = 200)
```


A successful body response.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `status` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

**Returns:** [DownloadResponse](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L167)

<a id="Vercel_AI_Util_DownloadResponse_Cancel_"></a>
<a id="Vercel_AI_Util_DownloadResponse_Cancel"></a>

### Cancel()


```csharp
public void Cancel()
```


Releases the body. Failures are ignored.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L150)

<a id="Vercel_AI_Util_DownloadResponse_Text_"></a>
<a id="Vercel_AI_Util_DownloadResponse_Text_System_String_System_String_"></a>

### Text(string?, string?)


```csharp
public static DownloadResponse Text(string? text, string? mediaType = "text/plain")
```


A successful UTF-8 text response.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [DownloadResponse](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Download.cs#L179)
