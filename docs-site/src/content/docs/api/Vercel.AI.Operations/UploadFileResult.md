---
title: "UploadFileResult"
description: "Result of CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "UploadFileResult"
  hidden: true
---

<a id="Vercel_AI_Operations_UploadFileResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class UploadFileResult
```


Result of `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L149)

## Constructors

<a id="Vercel_AI_Operations_UploadFileResult__ctor_"></a>
<a id="Vercel_AI_Operations_UploadFileResult__ctor_Vercel_AI_Operations_ProviderFileReference_System_String_System_String_System_Nullable_System_Int64__System_Nullable_System_DateTime__System_Nullable_System_DateTime__System_Nullable_System_Text_Json_JsonElement__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__"></a>

### `UploadFileResult(ProviderFileReference, string?, string?, long?, DateTime?, DateTime?, JsonElement?, IReadOnlyList<OperationWarning>)`


```csharp
public UploadFileResult(ProviderFileReference providerReference, string? mediaType, string? filename, long? byteSize, DateTime? createdAt, DateTime? expiresAt, JsonElement? providerMetadata, IReadOnlyList<OperationWarning> warnings)
```


Creates an upload result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerReference` | [ProviderFileReference](/api/Vercel.AI.Operations/ProviderFileReference/) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `filename` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `byteSize` | [long?](https://learn.microsoft.com/dotnet/api/system.int64) |  |
| `createdAt` | [DateTime?](https://learn.microsoft.com/dotnet/api/system.datetime) |  |
| `expiresAt` | [DateTime?](https://learn.microsoft.com/dotnet/api/system.datetime) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L152)

## Properties

<a id="Vercel_AI_Operations_UploadFileResult_ByteSize_"></a>
<a id="Vercel_AI_Operations_UploadFileResult_ByteSize"></a>

### ByteSize


```csharp
public long? ByteSize { get; }
```


Byte size.

**Value:** [long?](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L174)

<a id="Vercel_AI_Operations_UploadFileResult_CreatedAt_"></a>
<a id="Vercel_AI_Operations_UploadFileResult_CreatedAt"></a>

### CreatedAt


```csharp
public DateTime? CreatedAt { get; }
```


Creation time.

**Value:** [DateTime?](https://learn.microsoft.com/dotnet/api/system.datetime)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L177)

<a id="Vercel_AI_Operations_UploadFileResult_ExpiresAt_"></a>
<a id="Vercel_AI_Operations_UploadFileResult_ExpiresAt"></a>

### ExpiresAt


```csharp
public DateTime? ExpiresAt { get; }
```


Expiry time.

**Value:** [DateTime?](https://learn.microsoft.com/dotnet/api/system.datetime)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L180)

<a id="Vercel_AI_Operations_UploadFileResult_Filename_"></a>
<a id="Vercel_AI_Operations_UploadFileResult_Filename"></a>

### Filename


```csharp
public string? Filename { get; }
```


Filename.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L171)

<a id="Vercel_AI_Operations_UploadFileResult_MediaType_"></a>
<a id="Vercel_AI_Operations_UploadFileResult_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; }
```


Media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L168)

<a id="Vercel_AI_Operations_UploadFileResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_UploadFileResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L183)

<a id="Vercel_AI_Operations_UploadFileResult_ProviderReference_"></a>
<a id="Vercel_AI_Operations_UploadFileResult_ProviderReference"></a>

### ProviderReference


```csharp
public ProviderFileReference ProviderReference { get; }
```


Provider reference.

**Value:** [ProviderFileReference](/api/Vercel.AI.Operations/ProviderFileReference/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L165)

<a id="Vercel_AI_Operations_UploadFileResult_Warnings_"></a>
<a id="Vercel_AI_Operations_UploadFileResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L186)
