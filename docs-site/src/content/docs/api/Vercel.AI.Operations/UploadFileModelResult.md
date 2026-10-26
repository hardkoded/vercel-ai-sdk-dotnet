---
title: "UploadFileModelResult"
description: "Provider upload result."
editUrl: false
prev: false
next: false
sidebar:
  label: "UploadFileModelResult"
  hidden: true
---

<a id="Vercel_AI_Operations_UploadFileModelResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class UploadFileModelResult
```


Provider upload result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L94)

## Constructors

<a id="Vercel_AI_Operations_UploadFileModelResult__ctor_"></a>
<a id="Vercel_AI_Operations_UploadFileModelResult__ctor_Vercel_AI_Operations_ProviderFileReference_System_String_System_String_System_Nullable_System_Int64__System_Nullable_System_DateTime__System_Nullable_System_DateTime__System_Nullable_System_Text_Json_JsonElement__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__"></a>

### `UploadFileModelResult(ProviderFileReference, string?, string?, long?, DateTime?, DateTime?, JsonElement?, IReadOnlyList<OperationWarning>?)`


```csharp
public UploadFileModelResult(ProviderFileReference providerReference, string? mediaType = null, string? filename = null, long? byteSize = null, DateTime? createdAt = null, DateTime? expiresAt = null, JsonElement? providerMetadata = null, IReadOnlyList<OperationWarning>? warnings = null)
```


Creates a provider result.

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L97)

## Properties

<a id="Vercel_AI_Operations_UploadFileModelResult_ByteSize_"></a>
<a id="Vercel_AI_Operations_UploadFileModelResult_ByteSize"></a>

### ByteSize


```csharp
public long? ByteSize { get; }
```


Byte size.

**Value:** [long?](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L119)

<a id="Vercel_AI_Operations_UploadFileModelResult_CreatedAt_"></a>
<a id="Vercel_AI_Operations_UploadFileModelResult_CreatedAt"></a>

### CreatedAt


```csharp
public DateTime? CreatedAt { get; }
```


Creation time.

**Value:** [DateTime?](https://learn.microsoft.com/dotnet/api/system.datetime)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L122)

<a id="Vercel_AI_Operations_UploadFileModelResult_ExpiresAt_"></a>
<a id="Vercel_AI_Operations_UploadFileModelResult_ExpiresAt"></a>

### ExpiresAt


```csharp
public DateTime? ExpiresAt { get; }
```


Expiry time.

**Value:** [DateTime?](https://learn.microsoft.com/dotnet/api/system.datetime)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L125)

<a id="Vercel_AI_Operations_UploadFileModelResult_Filename_"></a>
<a id="Vercel_AI_Operations_UploadFileModelResult_Filename"></a>

### Filename


```csharp
public string? Filename { get; }
```


Filename reported by the provider.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L116)

<a id="Vercel_AI_Operations_UploadFileModelResult_MediaType_"></a>
<a id="Vercel_AI_Operations_UploadFileModelResult_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; }
```


Media type reported by the provider.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L113)

<a id="Vercel_AI_Operations_UploadFileModelResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_UploadFileModelResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L128)

<a id="Vercel_AI_Operations_UploadFileModelResult_ProviderReference_"></a>
<a id="Vercel_AI_Operations_UploadFileModelResult_ProviderReference"></a>

### ProviderReference


```csharp
public ProviderFileReference ProviderReference { get; }
```


Provider reference.

**Value:** [ProviderFileReference](/api/Vercel.AI.Operations/ProviderFileReference/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L110)

<a id="Vercel_AI_Operations_UploadFileModelResult_Warnings_"></a>
<a id="Vercel_AI_Operations_UploadFileModelResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L131)
