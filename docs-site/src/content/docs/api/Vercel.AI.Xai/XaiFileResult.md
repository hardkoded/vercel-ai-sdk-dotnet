---
title: "XaiFileResult"
description: "Result of an xAI file upload or metadata read."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiFileResult"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiFileResult"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiFileResult
```


Result of an xAI file upload or metadata read.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L79)

## Constructors

<a id="Vercel_AI_Xai_XaiFileResult__ctor_"></a>
<a id="Vercel_AI_Xai_XaiFileResult__ctor_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__Vercel_AI_Xai_XaiFileMetadata_System_String_System_String_System_Nullable_System_Int64__System_Nullable_System_DateTimeOffset__System_Nullable_System_DateTimeOffset__"></a>

### `XaiFileResult(IReadOnlyDictionary<string, string>, XaiFileMetadata, string?, string?, long?, DateTimeOffset?, DateTimeOffset?)`


```csharp
public XaiFileResult(IReadOnlyDictionary<string, string> providerReference, XaiFileMetadata metadata, string? fileName, string? mediaType, long? byteSize, DateTimeOffset? createdAt, DateTimeOffset? expiresAt)
```


Creates a file result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerReference` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `metadata` | [XaiFileMetadata](/api/Vercel.AI.Xai/XaiFileMetadata/) |  |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `byteSize` | [long?](https://learn.microsoft.com/dotnet/api/system.int64) |  |
| `createdAt` | [DateTimeOffset?](https://learn.microsoft.com/dotnet/api/system.datetimeoffset) |  |
| `expiresAt` | [DateTimeOffset?](https://learn.microsoft.com/dotnet/api/system.datetimeoffset) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L82)

## Properties

<a id="Vercel_AI_Xai_XaiFileResult_ByteSize_"></a>
<a id="Vercel_AI_Xai_XaiFileResult_ByteSize"></a>

### ByteSize


```csharp
public long? ByteSize { get; }
```


Size in bytes, when the response includes it.

**Value:** [long?](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L117)

<a id="Vercel_AI_Xai_XaiFileResult_CreatedAt_"></a>
<a id="Vercel_AI_Xai_XaiFileResult_CreatedAt"></a>

### CreatedAt


```csharp
public DateTimeOffset? CreatedAt { get; }
```


Creation time.

**Value:** [DateTimeOffset?](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L120)

<a id="Vercel_AI_Xai_XaiFileResult_ExpiresAt_"></a>
<a id="Vercel_AI_Xai_XaiFileResult_ExpiresAt"></a>

### ExpiresAt


```csharp
public DateTimeOffset? ExpiresAt { get; }
```


Expiration time.

**Value:** [DateTimeOffset?](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L123)

<a id="Vercel_AI_Xai_XaiFileResult_FileName_"></a>
<a id="Vercel_AI_Xai_XaiFileResult_FileName"></a>

### FileName


```csharp
public string? FileName { get; }
```


File name from the response, or the name that was uploaded.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L111)

<a id="Vercel_AI_Xai_XaiFileResult_MediaType_"></a>
<a id="Vercel_AI_Xai_XaiFileResult_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; }
```


Media type supplied by the caller.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L114)

<a id="Vercel_AI_Xai_XaiFileResult_Metadata_"></a>
<a id="Vercel_AI_Xai_XaiFileResult_Metadata"></a>

### Metadata


```csharp
public XaiFileMetadata Metadata { get; }
```


Response fields. Fields the response did not return are null.

**Value:** [XaiFileMetadata](/api/Vercel.AI.Xai/XaiFileMetadata/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L108)

<a id="Vercel_AI_Xai_XaiFileResult_ProviderReference_"></a>
<a id="Vercel_AI_Xai_XaiFileResult_ProviderReference"></a>

### ProviderReference


```csharp
public IReadOnlyDictionary<string, string> ProviderReference { get; }
```


Reference containing the `xai` file id.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L105)

<a id="Vercel_AI_Xai_XaiFileResult_Warnings_"></a>
<a id="Vercel_AI_Xai_XaiFileResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<ModelWarning> Warnings { get; }
```


Always empty.

**Value:** [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L102)
