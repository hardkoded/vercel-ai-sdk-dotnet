---
title: "OpenAICompatibleSseResponse"
description: "An open SSE response. The caller disposes it."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICompatibleSseResponse"
  hidden: true
---

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleSseResponse"></a>

Namespace: [Vercel.AI.OpenAICompatible](/api/Vercel.AI.OpenAICompatible/)


```csharp
public sealed class OpenAICompatibleSseResponse : IDisposable
```


An open SSE response. The caller disposes it.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L1012)

## Constructors

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleSseResponse__ctor_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleSseResponse__ctor_System_Net_Http_HttpResponseMessage_"></a>

### OpenAICompatibleSseResponse(HttpResponseMessage)


```csharp
public OpenAICompatibleSseResponse(HttpResponseMessage response)
```


Creates a response wrapper.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [HttpResponseMessage](https://learn.microsoft.com/dotnet/api/system.net.http.httpresponsemessage) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L1017)

## Properties

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleSseResponse_Headers_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleSseResponse_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L1038)

## Methods

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleSseResponse_Dispose_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleSseResponse_Dispose"></a>

### Dispose()


```csharp
public void Dispose()
```


Disposes the HTTP response.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L1051)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleSseResponse_ReadAsync_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleSseResponse_ReadAsync_System_Threading_CancellationToken_"></a>

### ReadAsync(CancellationToken)


```csharp
public IAsyncEnumerable<string> ReadAsync(CancellationToken cancellationToken = default)
```


Reads SSE `data` payloads until `[DONE]`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L1041)
