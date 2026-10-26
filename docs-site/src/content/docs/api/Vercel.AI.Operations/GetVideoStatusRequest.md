---
title: "GetVideoStatusRequest"
description: "Options for CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "GetVideoStatusRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_GetVideoStatusRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class GetVideoStatusRequest : OperationRequest
```


Options for `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OperationRequest](/api/Vercel.AI.Operations/OperationRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L547)

## Properties

<a id="Vercel_AI_Operations_GetVideoStatusRequest_Model_"></a>
<a id="Vercel_AI_Operations_GetVideoStatusRequest_Model"></a>

### Model


```csharp
public IVideoCaller? Model { get; set; }
```


Video model.

**Value:** [IVideoCaller](/api/Vercel.AI.Operations/IVideoCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L550)

<a id="Vercel_AI_Operations_GetVideoStatusRequest_Operation_"></a>
<a id="Vercel_AI_Operations_GetVideoStatusRequest_Operation"></a>

### Operation


```csharp
public object? Operation { get; set; }
```


Operation returned by start.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L553)
