---
title: "OpenAIFileMetadata"
description: "Metadata returned by the OpenAI Files API."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIFileMetadata"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIFileMetadata"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIFileMetadata
```


Metadata returned by the OpenAI Files API.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L27)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIFileMetadata_Bytes_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileMetadata_Bytes"></a>

### Bytes


```csharp
public int? Bytes { get; }
```


Size in bytes.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L50)

<a id="Vercel_AI_OpenAI_OpenAIFileMetadata_CreatedAt_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileMetadata_CreatedAt"></a>

### CreatedAt


```csharp
public DateTimeOffset? CreatedAt { get; }
```


Creation time.

**Value:** [DateTimeOffset?](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L53)

<a id="Vercel_AI_OpenAI_OpenAIFileMetadata_ExpiresAt_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileMetadata_ExpiresAt"></a>

### ExpiresAt


```csharp
public DateTimeOffset? ExpiresAt { get; }
```


Expiry time. Null when the upload has none.

**Value:** [DateTimeOffset?](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L59)

<a id="Vercel_AI_OpenAI_OpenAIFileMetadata_Filename_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileMetadata_Filename"></a>

### Filename


```csharp
public string? Filename { get; }
```


File name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L44)

<a id="Vercel_AI_OpenAI_OpenAIFileMetadata_Id_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileMetadata_Id"></a>

### Id


```csharp
public string Id { get; }
```


File id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L41)

<a id="Vercel_AI_OpenAI_OpenAIFileMetadata_Purpose_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileMetadata_Purpose"></a>

### Purpose


```csharp
public string? Purpose { get; }
```


Upload purpose.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L47)

<a id="Vercel_AI_OpenAI_OpenAIFileMetadata_Status_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileMetadata_Status"></a>

### Status


```csharp
public string? Status { get; }
```


Provider status.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L56)
