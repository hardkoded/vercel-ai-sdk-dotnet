---
title: "OpenAIFileStore"
description: "OpenAI file store."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIFileStore"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIFileStore"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIFileStore : IFileStore
```


OpenAI file store.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IFileStore](/api/Vercel.AI.Provider/IFileStore/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L63)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIFileStore__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileStore__ctor_Vercel_AI_OpenAI_OpenAIProvider_"></a>

### OpenAIFileStore(OpenAIProvider)


```csharp
public OpenAIFileStore(OpenAIProvider provider)
```


Creates a file store.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L68)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIFileStore_Provider_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileStore_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L74)

<a id="Vercel_AI_OpenAI_OpenAIFileStore_SpecificationVersion_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileStore_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L77)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIFileStore_DeleteAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileStore_DeleteAsync_Vercel_AI_OpenAI_OpenAIFileReference_System_Threading_CancellationToken_"></a>

### DeleteAsync(OpenAIFileReference, CancellationToken)


```csharp
public Task DeleteAsync(OpenAIFileReference file, CancellationToken cancellationToken)
```


Deletes a file.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `file` | [OpenAIFileReference](/api/Vercel.AI.OpenAI/OpenAIFileReference/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L174)

<a id="Vercel_AI_OpenAI_OpenAIFileStore_DownloadAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileStore_DownloadAsync_Vercel_AI_OpenAI_OpenAIFileReference_System_Threading_CancellationToken_"></a>

### DownloadAsync(OpenAIFileReference, CancellationToken)


```csharp
public Task<(byte[] Data, string? MediaType)> DownloadAsync(OpenAIFileReference file, CancellationToken cancellationToken)
```


Downloads file bytes. The media type has parameters removed.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `file` | [OpenAIFileReference](/api/Vercel.AI.OpenAI/OpenAIFileReference/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;(byte\[\] Data, string MediaType)&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L161)

<a id="Vercel_AI_OpenAI_OpenAIFileStore_GetMetadataAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileStore_GetMetadataAsync_Vercel_AI_OpenAI_OpenAIFileReference_System_Threading_CancellationToken_"></a>

### GetMetadataAsync(OpenAIFileReference, CancellationToken)


```csharp
public Task<OpenAIFileMetadata> GetMetadataAsync(OpenAIFileReference file, CancellationToken cancellationToken)
```


Reads file metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `file` | [OpenAIFileReference](/api/Vercel.AI.OpenAI/OpenAIFileReference/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;OpenAIFileMetadata&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L148)

<a id="Vercel_AI_OpenAI_OpenAIFileStore_RequireId_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileStore_RequireId_Vercel_AI_OpenAI_OpenAIFileReference_"></a>

### RequireId(OpenAIFileReference)


```csharp
public static string RequireId(OpenAIFileReference file)
```


Returns the OpenAI file id or throws when it is missing.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `file` | [OpenAIFileReference](/api/Vercel.AI.OpenAI/OpenAIFileReference/) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L187)

<a id="Vercel_AI_OpenAI_OpenAIFileStore_UploadAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileStore_UploadAsync_System_IO_Stream_System_String_System_String_System_String_System_Nullable_System_Int32__System_Threading_CancellationToken_"></a>

### UploadAsync(Stream, string?, string, string?, int?, CancellationToken)


```csharp
public Task<OpenAIFileMetadata> UploadAsync(Stream data, string? fileName, string mediaType, string? purpose, int? expiresAfterSeconds, CancellationToken cancellationToken)
```


Uploads a file from a stream without buffering it, and disposes the stream.
The fields precede the file part, and the file name defaults to `blob`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [Stream](https://learn.microsoft.com/dotnet/api/system.io.stream) |  |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `purpose` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `expiresAfterSeconds` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;OpenAIFileMetadata&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L105)

<a id="Vercel_AI_OpenAI_OpenAIFileStore_UploadAsync_System_String_System_Byte___System_String_System_String_System_Nullable_System_Int32__System_Threading_CancellationToken_"></a>

### UploadAsync(string, byte[], string, string?, int?, CancellationToken)


```csharp
public Task<OpenAIFileMetadata> UploadAsync(string fileName, byte[] data, string mediaType, string? purpose, int? expiresAfterSeconds, CancellationToken cancellationToken)
```


Uploads a file. &lt;code class="paramref"&gt;expiresAfterSeconds&lt;/code&gt; sends `expires_after`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `purpose` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `expiresAfterSeconds` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;OpenAIFileMetadata&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L87)

<a id="Vercel_AI_OpenAI_OpenAIFileStore_UploadFileAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileStore_UploadFileAsync_System_String_System_Byte___System_String_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L80)
