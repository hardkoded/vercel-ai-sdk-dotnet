---
title: "StartBatchOptions"
description: "Options for CancellationToken). Upstream StartBatchOptions. Starting a batch never retries, so there is no MaxRetries."
editUrl: false
prev: false
next: false
sidebar:
  label: "StartBatchOptions"
  hidden: true
---

<a id="Vercel_AI_Operations_StartBatchOptions"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class StartBatchOptions
```


Options for `CancellationToken)`. Upstream `StartBatchOptions`. Starting a batch never retries,
so there is no `MaxRetries`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L879)

## Properties

<a id="Vercel_AI_Operations_StartBatchOptions_AbortReason_"></a>
<a id="Vercel_AI_Operations_StartBatchOptions_AbortReason"></a>

### AbortReason


```csharp
public Exception? AbortReason { get; set; }
```


Exception thrown when [CancellationToken](#Vercel_AI_Operations_StartBatchOptions_CancellationToken) is already cancelled.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L907)

<a id="Vercel_AI_Operations_StartBatchOptions_CancellationToken_"></a>
<a id="Vercel_AI_Operations_StartBatchOptions_CancellationToken"></a>

### CancellationToken


```csharp
public CancellationToken CancellationToken { get; set; }
```


Cancels the call.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L904)

<a id="Vercel_AI_Operations_StartBatchOptions_Headers_"></a>
<a id="Vercel_AI_Operations_StartBatchOptions_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; set; }
```


Extra HTTP headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L910)

<a id="Vercel_AI_Operations_StartBatchOptions_Output_"></a>
<a id="Vercel_AI_Operations_StartBatchOptions_Output"></a>

### Output


```csharp
public OutputSpec? Output { get; set; }
```


Specification for structured data in the text responses. Its response format is sent with every text request
in the batch; image requests ignore it. Pass the same value to `GetBatchResults` to parse and
validate each result.

**Value:** [OutputSpec](/api/Vercel.AI/OutputSpec/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L895)

<a id="Vercel_AI_Operations_StartBatchOptions_Provider_"></a>
<a id="Vercel_AI_Operations_StartBatchOptions_Provider"></a>

### Provider


```csharp
public ApiOrProvider<IBatchApi, IBatchProvider>? Provider { get; set; }
```


Batch API or a provider that exposes one. Null uses [Default](/api/Vercel.AI.Operations/BatchModels/#Vercel_AI_Operations_BatchModels_Default).

**Value:** `ApiOrProvider&lt;IBatchApi, IBatchProvider&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L882)

<a id="Vercel_AI_Operations_StartBatchOptions_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_StartBatchOptions_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Provider-specific options.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L888)

<a id="Vercel_AI_Operations_StartBatchOptions_Requests_"></a>
<a id="Vercel_AI_Operations_StartBatchOptions_Requests"></a>

### Requests


```csharp
public IReadOnlyList<BatchRequest>? Requests { get; set; }
```


Requests to start.

**Value:** [IReadOnlyList&lt;BatchRequest&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L885)

<a id="Vercel_AI_Operations_StartBatchOptions_TimeoutMs_"></a>
<a id="Vercel_AI_Operations_StartBatchOptions_TimeoutMs"></a>

### TimeoutMs


```csharp
public int? TimeoutMs { get; set; }
```


Total timeout in milliseconds.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L901)

<a id="Vercel_AI_Operations_StartBatchOptions_WebhookUrl_"></a>
<a id="Vercel_AI_Operations_StartBatchOptions_WebhookUrl"></a>

### WebhookUrl


```csharp
public string? WebhookUrl { get; set; }
```


URL the provider notifies when the batch reaches a terminal state.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L898)
