---
title: "UploadFile"
description: "Uploads a file. Maps to uploadFile."
editUrl: false
prev: false
next: false
sidebar:
  label: "UploadFile"
  hidden: true
---

<a id="Vercel_AI_Operations_UploadFile"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class UploadFile
```


Uploads a file. Maps to `uploadFile`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L218)

## Methods

<a id="Vercel_AI_Operations_UploadFile_UploadFileAsync_"></a>
<a id="Vercel_AI_Operations_UploadFile_UploadFileAsync_Vercel_AI_Operations_UploadFileRequest_System_Threading_CancellationToken_"></a>

### UploadFileAsync(UploadFileRequest, CancellationToken)


```csharp
public static Task<UploadFileResult> UploadFileAsync(UploadFileRequest request, CancellationToken cancellationToken = default)
```


Uploads [Data](/api/Vercel.AI.Operations/UploadFileRequest/#Vercel_AI_Operations_UploadFileRequest_Data).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [UploadFileRequest](/api/Vercel.AI.Operations/UploadFileRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;UploadFileResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L221)
