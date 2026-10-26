---
title: "LanguageModelStreamResult"
description: "What CancellationToken) returns: the stream plus the request and response info. Maps to the { stream, request, response } object returned by doStream. Enumer..."
editUrl: false
prev: false
next: false
sidebar:
  label: "LanguageModelStreamResult"
  hidden: true
---

<a id="Vercel_AI_Provider_LanguageModelStreamResult"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class LanguageModelStreamResult : IAsyncEnumerable<LanguageModelStreamPart>, IAsyncDisposable, IDisposable
```


What `CancellationToken)` returns: the stream plus the request and response info.
Maps to the `{ stream, request, response }` object returned by `doStream`.
Enumerating the result enumerates [Stream](#Vercel_AI_Provider_LanguageModelStreamResult_Stream).
A provider opens the HTTP response in `DoStreamAsync`. Dispose the result to release that response when you stop without enumerating the stream, or while you enumerate it.
Disposing is safe to repeat, and it does nothing after the stream has ended.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IAsyncEnumerable&lt;LanguageModelStreamPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1), [IAsyncDisposable](https://learn.microsoft.com/dotnet/api/system.iasyncdisposable), [IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2205)

## Constructors

<a id="Vercel_AI_Provider_LanguageModelStreamResult__ctor_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamResult__ctor_System_Collections_Generic_IAsyncEnumerable_Vercel_AI_Provider_LanguageModelStreamPart__Vercel_AI_Provider_LanguageModelStreamRequest_Vercel_AI_Provider_LanguageModelStreamResponse_System_IDisposable_"></a>

### `LanguageModelStreamResult(IAsyncEnumerable<LanguageModelStreamPart>, LanguageModelStreamRequest?, LanguageModelStreamResponse?, IDisposable?)`


```csharp
public LanguageModelStreamResult(IAsyncEnumerable<LanguageModelStreamPart> stream, LanguageModelStreamRequest? request = null, LanguageModelStreamResponse? response = null, IDisposable? resource = null)
```


Creates a stream result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stream` | [IAsyncEnumerable&lt;LanguageModelStreamPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1) | The stream parts. |
| `request` | [LanguageModelStreamRequest](/api/Vercel.AI.Provider/LanguageModelStreamRequest/) | Request info. |
| `response` | [LanguageModelStreamResponse](/api/Vercel.AI.Provider/LanguageModelStreamResponse/) | Response info. |
| `resource` | [IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable) | A resource the provider opened for the stream, such as the HTTP response. The result disposes it once. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2214)

## Properties

<a id="Vercel_AI_Provider_LanguageModelStreamResult_Request_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamResult_Request"></a>

### Request


```csharp
public LanguageModelStreamRequest? Request { get; }
```


Request info, when the model reports it.

**Value:** [LanguageModelStreamRequest](/api/Vercel.AI.Provider/LanguageModelStreamRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2230)

<a id="Vercel_AI_Provider_LanguageModelStreamResult_Response_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamResult_Response"></a>

### Response


```csharp
public LanguageModelStreamResponse? Response { get; }
```


Response info, when the model reports it.

**Value:** [LanguageModelStreamResponse](/api/Vercel.AI.Provider/LanguageModelStreamResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2233)

<a id="Vercel_AI_Provider_LanguageModelStreamResult_Stream_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamResult_Stream"></a>

### Stream


```csharp
public IAsyncEnumerable<LanguageModelStreamPart> Stream { get; }
```


The stream parts.

**Value:** [IAsyncEnumerable&lt;LanguageModelStreamPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2227)

## Methods

<a id="Vercel_AI_Provider_LanguageModelStreamResult_ConfigureAwait_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamResult_ConfigureAwait_System_Boolean_"></a>

### ConfigureAwait(bool)


```csharp
public ConfiguredCancelableAsyncEnumerable<LanguageModelStreamPart> ConfigureAwait(bool continueOnCapturedContext)
```


Configures how awaits on the stream parts run. This instance method keeps `await foreach (var part in result.ConfigureAwait(false))`
unambiguous, because the result is both an async enumerable and an async disposable.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `continueOnCapturedContext` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [ConfiguredCancelableAsyncEnumerable&lt;LanguageModelStreamPart&gt;](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.configuredcancelableasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2245)

<a id="Vercel_AI_Provider_LanguageModelStreamResult_Dispose_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamResult_Dispose"></a>

### Dispose()


```csharp
public void Dispose()
```


Releases the resource the provider opened for this stream. Safe to call more than once.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2251)

<a id="Vercel_AI_Provider_LanguageModelStreamResult_DisposeAsync_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamResult_DisposeAsync"></a>

### DisposeAsync()


```csharp
public ValueTask DisposeAsync()
```


Releases the resource the provider opened for this stream. Safe to call more than once.

**Returns:** [ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2257)

<a id="Vercel_AI_Provider_LanguageModelStreamResult_GetAsyncEnumerator_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamResult_GetAsyncEnumerator_System_Threading_CancellationToken_"></a>

### GetAsyncEnumerator(CancellationToken)


```csharp
public IAsyncEnumerator<LanguageModelStreamPart> GetAsyncEnumerator(CancellationToken cancellationToken = default)
```


Returns an enumerator that iterates asynchronously through the collection.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) | A [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) that may be used to cancel the asynchronous iteration. |

**Returns:** [IAsyncEnumerator&lt;LanguageModelStreamPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerator-1) — An enumerator that can be used to iterate asynchronously through the collection.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2236)
