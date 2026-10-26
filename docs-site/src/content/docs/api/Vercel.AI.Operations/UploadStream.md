---
title: "UploadStream"
description: "A stream the provider may take ownership of."
editUrl: false
prev: false
next: false
sidebar:
  label: "UploadStream"
  hidden: true
---

<a id="Vercel_AI_Operations_UploadStream"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class UploadStream
```


A stream the provider may take ownership of.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L27)

## Properties

<a id="Vercel_AI_Operations_UploadStream_CancelError_"></a>
<a id="Vercel_AI_Operations_UploadStream_CancelError"></a>

### CancelError


```csharp
public Exception? CancelError { get; }
```


Reason passed to [Cancel(Exception)](#Vercel_AI_Operations_UploadStream_Cancel_System_Exception_).

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L33)

<a id="Vercel_AI_Operations_UploadStream_Cancelled_"></a>
<a id="Vercel_AI_Operations_UploadStream_Cancelled"></a>

### Cancelled


```csharp
public bool Cancelled { get; }
```


True after [Cancel(Exception)](#Vercel_AI_Operations_UploadStream_Cancel_System_Exception_).

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L30)

## Methods

<a id="Vercel_AI_Operations_UploadStream_Cancel_"></a>
<a id="Vercel_AI_Operations_UploadStream_Cancel_System_Exception_"></a>

### Cancel(Exception)


```csharp
public void Cancel(Exception error)
```


Releases the stream after a failed upload.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L36)
