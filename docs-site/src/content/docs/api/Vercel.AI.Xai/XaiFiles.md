---
title: "XaiFiles"
description: "xAI Files API at {base}/files."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiFiles"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiFiles"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiFiles : IFileStore
```


xAI Files API at `{base}/files`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IFileStore](/api/Vercel.AI.Provider/IFileStore/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L169)

## Constructors

<a id="Vercel_AI_Xai_XaiFiles__ctor_"></a>
<a id="Vercel_AI_Xai_XaiFiles__ctor_System_Net_Http_HttpClient_System_String_System_Func_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String___System_String_"></a>

### `XaiFiles(HttpClient, string, Func<IReadOnlyDictionary<string, string?>>?, string?)`


```csharp
public XaiFiles(HttpClient httpClient, string baseUrl, Func<IReadOnlyDictionary<string, string?>>? headers, string? provider = null)
```


Creates a file client.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [Func&lt;IReadOnlyDictionary&lt;string, string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L182)

## Fields

<a id="Vercel_AI_Xai_XaiFiles_MaximumExpiresAfter"></a>

### MaximumExpiresAfter


```csharp
public const int MaximumExpiresAfter = 2592000
```


Longest accepted `expires_after`, in seconds.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L175)

<a id="Vercel_AI_Xai_XaiFiles_MinimumExpiresAfter"></a>

### MinimumExpiresAfter


```csharp
public const int MinimumExpiresAfter = 3600
```


Shortest accepted `expires_after`, in seconds.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L172)

## Properties

<a id="Vercel_AI_Xai_XaiFiles_Provider_"></a>
<a id="Vercel_AI_Xai_XaiFiles_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L197)

<a id="Vercel_AI_Xai_XaiFiles_SpecificationVersion_"></a>
<a id="Vercel_AI_Xai_XaiFiles_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L191)

## Methods

<a id="Vercel_AI_Xai_XaiFiles_DeleteAsync_"></a>
<a id="Vercel_AI_Xai_XaiFiles_DeleteAsync_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `DeleteAsync(IReadOnlyDictionary<string, string?>, CancellationToken)`


```csharp
public Task<XaiFileDeleteResult> DeleteAsync(IReadOnlyDictionary<string, string?> file, CancellationToken cancellationToken)
```


Deletes a file.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `file` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;XaiFileDeleteResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L297)

<a id="Vercel_AI_Xai_XaiFiles_DownloadAsync_"></a>
<a id="Vercel_AI_Xai_XaiFiles_DownloadAsync_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `DownloadAsync(IReadOnlyDictionary<string, string?>, CancellationToken)`


```csharp
public Task<XaiFileDownload> DownloadAsync(IReadOnlyDictionary<string, string?> file, CancellationToken cancellationToken)
```


Downloads file bytes.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `file` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;XaiFileDownload&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L271)

<a id="Vercel_AI_Xai_XaiFiles_GetMetadataAsync_"></a>
<a id="Vercel_AI_Xai_XaiFiles_GetMetadataAsync_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `GetMetadataAsync(IReadOnlyDictionary<string, string?>, CancellationToken)`


```csharp
public Task<XaiFileResult> GetMetadataAsync(IReadOnlyDictionary<string, string?> file, CancellationToken cancellationToken)
```


Reads file metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `file` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;XaiFileResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L257)

<a id="Vercel_AI_Xai_XaiFiles_UploadAsync_"></a>
<a id="Vercel_AI_Xai_XaiFiles_UploadAsync_Vercel_AI_Xai_XaiFileUpload_System_Threading_CancellationToken_"></a>

### UploadAsync(XaiFileUpload, CancellationToken)


```csharp
public Task<XaiFileResult> UploadAsync(XaiFileUpload upload, CancellationToken cancellationToken)
```


Posts multipart file contents to `/files`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `upload` | [XaiFileUpload](/api/Vercel.AI.Xai/XaiFileUpload/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;XaiFileResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L209)

<a id="Vercel_AI_Xai_XaiFiles_UploadFileAsync_"></a>
<a id="Vercel_AI_Xai_XaiFiles_UploadFileAsync_System_String_System_Byte___System_String_System_Threading_CancellationToken_"></a>

### UploadFileAsync(string, byte[], string, CancellationToken)


```csharp
public Task<UploadedFile> UploadFileAsync(string fileName, byte[] data, string mediaType, CancellationToken cancellationToken)
```


Uploads a file.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;UploadedFile&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L200)
