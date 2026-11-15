---
title: "TranscriptionStreamStart"
description: "A started transcription stream."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranscriptionStreamStart"
  hidden: true
---

<a id="Vercel_AI_Operations_TranscriptionStreamStart"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class TranscriptionStreamStart
```


A started transcription stream.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L126)

## Constructors

<a id="Vercel_AI_Operations_TranscriptionStreamStart__ctor_"></a>
<a id="Vercel_AI_Operations_TranscriptionStreamStart__ctor_Vercel_AI_Operations_ModelPartStream_Vercel_AI_Operations_ProviderResponse_Vercel_AI_Operations_ProviderRequest_"></a>

### TranscriptionStreamStart(ModelPartStream, ProviderResponse?, ProviderRequest?)


```csharp
public TranscriptionStreamStart(ModelPartStream stream, ProviderResponse? response = null, ProviderRequest? request = null)
```


Creates a start payload.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stream` | [ModelPartStream](/api/Vercel.AI.Operations/ModelPartStream/) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |
| `request` | [ProviderRequest](/api/Vercel.AI.Operations/ProviderRequest/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L129)

## Properties

<a id="Vercel_AI_Operations_TranscriptionStreamStart_Request_"></a>
<a id="Vercel_AI_Operations_TranscriptionStreamStart_Request"></a>

### Request


```csharp
public ProviderRequest? Request { get; }
```


Request metadata. [Body](/api/Vercel.AI.Operations/ProviderRequest/#Vercel_AI_Operations_ProviderRequest_Body) never carries credentials.

**Value:** [ProviderRequest](/api/Vercel.AI.Operations/ProviderRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L137)

<a id="Vercel_AI_Operations_TranscriptionStreamStart_Response_"></a>
<a id="Vercel_AI_Operations_TranscriptionStreamStart_Response"></a>

### Response


```csharp
public ProviderResponse? Response { get; }
```


Initial response metadata.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L143)

<a id="Vercel_AI_Operations_TranscriptionStreamStart_Stream_"></a>
<a id="Vercel_AI_Operations_TranscriptionStreamStart_Stream"></a>

### Stream


```csharp
public ModelPartStream Stream { get; }
```


Model parts.

**Value:** [ModelPartStream](/api/Vercel.AI.Operations/ModelPartStream/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L140)
