---
title: "OperationRequest"
description: "Shared request fields for an operation call."
editUrl: false
prev: false
next: false
sidebar:
  label: "OperationRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_OperationRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public class OperationRequest
```


Shared request fields for an operation call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [BatchRequestOptions](/api/Vercel.AI.Operations/BatchRequestOptions/), [DecideRequest](/api/Vercel.AI.Operations/DecideRequest/), [EmbedManyRequest](/api/Vercel.AI.Operations/EmbedManyRequest/), [EmbedRequest](/api/Vercel.AI.Operations/EmbedRequest/), [EvaluateRequest](/api/Vercel.AI.Operations/EvaluateRequest/), [GenerateImageRequest](/api/Vercel.AI.Operations/GenerateImageRequest/), [GenerateSpeechRequest](/api/Vercel.AI.Operations/GenerateSpeechRequest/), [GenerateVideoRequest](/api/Vercel.AI.Operations/GenerateVideoRequest/), [GetVideoStatusRequest](/api/Vercel.AI.Operations/GetVideoStatusRequest/), [RerankRequest](/api/Vercel.AI.Operations/RerankRequest/), [StreamSpeechRequest](/api/Vercel.AI.Operations/StreamSpeechRequest/), [StreamTranscribeRequest](/api/Vercel.AI.Operations/StreamTranscribeRequest/), [StreamTranslateRequest](/api/Vercel.AI.Operations/StreamTranslateRequest/), [TranscribeRequest](/api/Vercel.AI.Operations/TranscribeRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L443)

## Properties

<a id="Vercel_AI_Operations_OperationRequest_AbortReason_"></a>
<a id="Vercel_AI_Operations_OperationRequest_AbortReason"></a>

### AbortReason


```csharp
public Exception? AbortReason { get; set; }
```


Exception thrown when [CancellationToken](#Vercel_AI_Operations_OperationRequest_CancellationToken) is already cancelled.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L452)

<a id="Vercel_AI_Operations_OperationRequest_CancellationToken_"></a>
<a id="Vercel_AI_Operations_OperationRequest_CancellationToken"></a>

### CancellationToken


```csharp
public CancellationToken CancellationToken { get; set; }
```


Cancels the call.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L449)

<a id="Vercel_AI_Operations_OperationRequest_GenerateCallId_"></a>
<a id="Vercel_AI_Operations_OperationRequest_GenerateCallId"></a>

### GenerateCallId


```csharp
public Func<string>? GenerateCallId { get; set; }
```


Supplies call ids. Tests pass a constant generator.

**Value:** [Func&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L464)

<a id="Vercel_AI_Operations_OperationRequest_Headers_"></a>
<a id="Vercel_AI_Operations_OperationRequest_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; set; }
```


Extra HTTP headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L455)

<a id="Vercel_AI_Operations_OperationRequest_MaxRetries_"></a>
<a id="Vercel_AI_Operations_OperationRequest_MaxRetries"></a>

### MaxRetries


```csharp
public int? MaxRetries { get; set; }
```


Maximum retries after the first attempt. Default is 2.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L446)

<a id="Vercel_AI_Operations_OperationRequest_Now_"></a>
<a id="Vercel_AI_Operations_OperationRequest_Now"></a>

### Now


```csharp
public Func<DateTime>? Now { get; set; }
```


Clock used when a provider omits a timestamp.

**Value:** [Func&lt;DateTime&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L467)

<a id="Vercel_AI_Operations_OperationRequest_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_OperationRequest_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Provider-specific options.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L458)

<a id="Vercel_AI_Operations_OperationRequest_RuntimeContext_"></a>
<a id="Vercel_AI_Operations_OperationRequest_RuntimeContext"></a>

### RuntimeContext


```csharp
public IReadOnlyDictionary<string, object?>? RuntimeContext { get; set; }
```


Values copied onto user callbacks. Telemetry receives the filtered copy.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L461)
