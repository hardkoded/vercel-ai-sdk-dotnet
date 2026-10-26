---
title: "XaiFileMetadata"
description: "Metadata stored under providerMetadata.xai. A field is null when the response did not return it."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiFileMetadata"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiFileMetadata"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiFileMetadata
```


Metadata stored under `providerMetadata.xai`. A field is null when the response did not return it.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L46)

## Constructors

<a id="Vercel_AI_Xai_XaiFileMetadata__ctor_"></a>
<a id="Vercel_AI_Xai_XaiFileMetadata__ctor_System_String_System_String_System_Nullable_System_Int64__System_Nullable_System_Int64__System_String_System_Nullable_System_Int64__"></a>

### XaiFileMetadata(string?, string?, long?, long?, string?, long?)


```csharp
public XaiFileMetadata(string? fileName, string? purpose, long? bytes, long? createdAt, string? status, long? expiresAt)
```


Creates metadata from the fields the response returned.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `purpose` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `bytes` | [long?](https://learn.microsoft.com/dotnet/api/system.int64) |  |
| `createdAt` | [long?](https://learn.microsoft.com/dotnet/api/system.int64) |  |
| `status` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `expiresAt` | [long?](https://learn.microsoft.com/dotnet/api/system.int64) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L49)

## Properties

<a id="Vercel_AI_Xai_XaiFileMetadata_Bytes_"></a>
<a id="Vercel_AI_Xai_XaiFileMetadata_Bytes"></a>

### Bytes


```csharp
public long? Bytes { get; }
```


`bytes` from the response.

**Value:** [long?](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L66)

<a id="Vercel_AI_Xai_XaiFileMetadata_CreatedAt_"></a>
<a id="Vercel_AI_Xai_XaiFileMetadata_CreatedAt"></a>

### CreatedAt


```csharp
public long? CreatedAt { get; }
```


`created_at` from the response, in unix seconds.

**Value:** [long?](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L69)

<a id="Vercel_AI_Xai_XaiFileMetadata_ExpiresAt_"></a>
<a id="Vercel_AI_Xai_XaiFileMetadata_ExpiresAt"></a>

### ExpiresAt


```csharp
public long? ExpiresAt { get; }
```


`expires_at` from the response, in unix seconds.

**Value:** [long?](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L75)

<a id="Vercel_AI_Xai_XaiFileMetadata_FileName_"></a>
<a id="Vercel_AI_Xai_XaiFileMetadata_FileName"></a>

### FileName


```csharp
public string? FileName { get; }
```


`filename` from the response.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L60)

<a id="Vercel_AI_Xai_XaiFileMetadata_Purpose_"></a>
<a id="Vercel_AI_Xai_XaiFileMetadata_Purpose"></a>

### Purpose


```csharp
public string? Purpose { get; }
```


`purpose` from the response.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L63)

<a id="Vercel_AI_Xai_XaiFileMetadata_Status_"></a>
<a id="Vercel_AI_Xai_XaiFileMetadata_Status"></a>

### Status


```csharp
public string? Status { get; }
```


`status` from the response.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L72)
