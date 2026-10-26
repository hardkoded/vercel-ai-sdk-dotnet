---
title: "StreamTranslateResult"
description: "Streaming translation result."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamTranslateResult"
  hidden: true
---

<a id="Vercel_AI_Operations_StreamTranslateResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class StreamTranslateResult
```


Streaming translation result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L123)

## Properties

<a id="Vercel_AI_Operations_StreamTranslateResult_DurationInSeconds_"></a>
<a id="Vercel_AI_Operations_StreamTranslateResult_DurationInSeconds"></a>

### DurationInSeconds


```csharp
public Task<double?> DurationInSeconds { get; }
```


Duration. Accessing it claims the stream.

**Value:** [Task&lt;double?&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L163)

<a id="Vercel_AI_Operations_StreamTranslateResult_FullStream_"></a>
<a id="Vercel_AI_Operations_StreamTranslateResult_FullStream"></a>

### FullStream


```csharp
public OperationFullStream<ModelStreamPart> FullStream { get; }
```


Single-consumer part stream.

**Value:** `OperationFullStream&lt;ModelStreamPart&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L193)

<a id="Vercel_AI_Operations_StreamTranslateResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_StreamTranslateResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public Task<JsonElement> ProviderMetadata { get; }
```


Provider metadata. Accessing it claims the stream.

**Value:** [Task&lt;JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L187)

<a id="Vercel_AI_Operations_StreamTranslateResult_Response_"></a>
<a id="Vercel_AI_Operations_StreamTranslateResult_Response"></a>

### Response


```csharp
public Task<ProviderResponse> Response { get; }
```


Response. Accessing it claims the stream.

**Value:** [Task&lt;ProviderResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L181)

<a id="Vercel_AI_Operations_StreamTranslateResult_SourceText_"></a>
<a id="Vercel_AI_Operations_StreamTranslateResult_SourceText"></a>

### SourceText


```csharp
public Task<string> SourceText { get; }
```


Source transcript. Accessing it claims the stream.

**Value:** [Task&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L151)

<a id="Vercel_AI_Operations_StreamTranslateResult_TranslationText_"></a>
<a id="Vercel_AI_Operations_StreamTranslateResult_TranslationText"></a>

### TranslationText


```csharp
public Task<string> TranslationText { get; }
```


Translated text. Accessing it claims the stream.

**Value:** [Task&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L157)

<a id="Vercel_AI_Operations_StreamTranslateResult_Usage_"></a>
<a id="Vercel_AI_Operations_StreamTranslateResult_Usage"></a>

### Usage


```csharp
public Task<OperationUsage?> Usage { get; }
```


Usage. Accessing it claims the stream.

**Value:** [Task&lt;OperationUsage&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L169)

<a id="Vercel_AI_Operations_StreamTranslateResult_Warnings_"></a>
<a id="Vercel_AI_Operations_StreamTranslateResult_Warnings"></a>

### Warnings


```csharp
public Task<IReadOnlyList<OperationWarning>> Warnings { get; }
```


Warnings. Accessing it claims the stream.

**Value:** [Task&lt;IReadOnlyList&lt;OperationWarning&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L175)
