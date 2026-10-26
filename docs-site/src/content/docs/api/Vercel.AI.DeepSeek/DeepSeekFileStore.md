---
title: "DeepSeekFileStore"
description: "Uploads images to POST /files with purpose userdata."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekFileStore"
  hidden: true
---

<a id="Vercel_AI_DeepSeek_DeepSeekFileStore"></a>

Namespace: [Vercel.AI.DeepSeek](/api/Vercel.AI.DeepSeek/)


```csharp
public sealed class DeepSeekFileStore : IFileStore, IFileStore
```


Uploads images to `POST /files` with purpose `user_data`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IFileStore](/api/Vercel.AI.Provider/IFileStore/), [IFileStore](/api/Vercel.AI.Operations/IFileStore/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekFileStore.cs#L17)

## Constructors

<a id="Vercel_AI_DeepSeek_DeepSeekFileStore__ctor_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekFileStore__ctor_Vercel_AI_DeepSeek_DeepSeekProvider_"></a>

### DeepSeekFileStore(DeepSeekProvider)


```csharp
public DeepSeekFileStore(DeepSeekProvider provider)
```


Creates a file store.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [DeepSeekProvider](/api/Vercel.AI.DeepSeek/DeepSeekProvider/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekFileStore.cs#L34)

## Fields

<a id="Vercel_AI_DeepSeek_DeepSeekFileStore_MaxBytes"></a>

### MaxBytes


```csharp
public const long MaxBytes = 67108864
```


Maximum upload size, 64 MiB.

**Value:** [long](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekFileStore.cs#L20)

<a id="Vercel_AI_DeepSeek_DeepSeekFileStore_MaxExpirySeconds"></a>

### MaxExpirySeconds


```csharp
public const int MaxExpirySeconds = 2592000
```


Longest accepted expiry, in seconds.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekFileStore.cs#L29)

<a id="Vercel_AI_DeepSeek_DeepSeekFileStore_MaxFileNameLength"></a>

### MaxFileNameLength


```csharp
public const int MaxFileNameLength = 512
```


Maximum filename length.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekFileStore.cs#L23)

<a id="Vercel_AI_DeepSeek_DeepSeekFileStore_MinExpirySeconds"></a>

### MinExpirySeconds


```csharp
public const int MinExpirySeconds = 3600
```


Shortest accepted expiry, in seconds.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekFileStore.cs#L26)

## Properties

<a id="Vercel_AI_DeepSeek_DeepSeekFileStore_ExpiresAfterSeconds_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekFileStore_ExpiresAfterSeconds"></a>

### ExpiresAfterSeconds


```csharp
public int? ExpiresAfterSeconds { get; set; }
```


When set, sent as `expires_after[seconds]` with anchor `created_at`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekFileStore.cs#L40)

<a id="Vercel_AI_DeepSeek_DeepSeekFileStore_Provider_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekFileStore_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekFileStore.cs#L43)

<a id="Vercel_AI_DeepSeek_DeepSeekFileStore_SpecificationVersion_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekFileStore_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Files specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekFileStore.cs#L46)

## Methods

<a id="Vercel_AI_DeepSeek_DeepSeekFileStore_UploadFileAsync_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekFileStore_UploadFileAsync_System_String_System_Byte___System_String_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekFileStore.cs#L49)

<a id="Vercel_AI_DeepSeek_DeepSeekFileStore_UploadFileAsync_Vercel_AI_Operations_UploadFileCall_System_Threading_CancellationToken_"></a>

### UploadFileAsync(UploadFileCall, CancellationToken)


```csharp
public Task<UploadFileModelResult> UploadFileAsync(UploadFileCall call, CancellationToken cancellationToken)
```


Uploads inline data. Streams are rejected. `deepseek.expiresAfter` overrides [ExpiresAfterSeconds](#Vercel_AI_DeepSeek_DeepSeekFileStore_ExpiresAfterSeconds).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [UploadFileCall](/api/Vercel.AI.Operations/UploadFileCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;UploadFileModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekFileStore.cs#L58)

<a id="Vercel_AI_DeepSeek_DeepSeekFileStore_Validate_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekFileStore_Validate_System_Int64_System_String_System_String_System_Byte___"></a>

### Validate(long, string?, string, byte[]?)


```csharp
public static void Validate(long length, string? fileName, string mediaType, byte[]? data)
```


Checks size, filename, media type, and magic bytes before any request is sent.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `length` | [long](https://learn.microsoft.com/dotnet/api/system.int64) |  |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekFileStore.cs#L202)
