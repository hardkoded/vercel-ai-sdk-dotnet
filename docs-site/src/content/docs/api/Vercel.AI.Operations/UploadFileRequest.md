---
title: "UploadFileRequest"
description: "Options for CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "UploadFileRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_UploadFileRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class UploadFileRequest
```


Options for `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L190)

## Properties

<a id="Vercel_AI_Operations_UploadFileRequest_Api_"></a>
<a id="Vercel_AI_Operations_UploadFileRequest_Api"></a>

### Api


```csharp
public ApiOrProvider<IFileStore, IFileStoreProvider>? Api { get; set; }
```


Files API or a provider that exposes one.

**Value:** `ApiOrProvider&lt;IFileStore, IFileStoreProvider&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L193)

<a id="Vercel_AI_Operations_UploadFileRequest_CancellationToken_"></a>
<a id="Vercel_AI_Operations_UploadFileRequest_CancellationToken"></a>

### CancellationToken


```csharp
public CancellationToken CancellationToken { get; set; }
```


Cancellation token.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L205)

<a id="Vercel_AI_Operations_UploadFileRequest_Data_"></a>
<a id="Vercel_AI_Operations_UploadFileRequest_Data"></a>

### Data


```csharp
public object? Data { get; set; }
```


Tagged data, bytes, or a string shorthand.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L196)

<a id="Vercel_AI_Operations_UploadFileRequest_Filename_"></a>
<a id="Vercel_AI_Operations_UploadFileRequest_Filename"></a>

### Filename


```csharp
public string? Filename { get; set; }
```


Optional filename.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L202)

<a id="Vercel_AI_Operations_UploadFileRequest_Headers_"></a>
<a id="Vercel_AI_Operations_UploadFileRequest_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; set; }
```


Request headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L208)

<a id="Vercel_AI_Operations_UploadFileRequest_MediaType_"></a>
<a id="Vercel_AI_Operations_UploadFileRequest_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; set; }
```


Optional media type. Detected when omitted.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L199)

<a id="Vercel_AI_Operations_UploadFileRequest_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_UploadFileRequest_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Provider options. Null when omitted.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L211)

<a id="Vercel_AI_Operations_UploadFileRequest_ProviderOptionsSpecified_"></a>
<a id="Vercel_AI_Operations_UploadFileRequest_ProviderOptionsSpecified"></a>

### ProviderOptionsSpecified


```csharp
public bool ProviderOptionsSpecified { get; set; }
```


True when the caller set [ProviderOptions](#Vercel_AI_Operations_UploadFileRequest_ProviderOptions), including an explicit null.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L214)
