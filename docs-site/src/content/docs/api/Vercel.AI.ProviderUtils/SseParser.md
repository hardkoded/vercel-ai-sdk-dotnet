---
title: "SseParser"
description: "Reads Server-Sent Events data: lines."
editUrl: false
prev: false
next: false
sidebar:
  label: "SseParser"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_SseParser"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class SseParser
```


Reads Server-Sent Events `data:` lines.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/SseParser.cs#L8)

## Methods

<a id="Vercel_AI_ProviderUtils_SseParser_ReadDataAsync_"></a>
<a id="Vercel_AI_ProviderUtils_SseParser_ReadDataAsync_System_IO_Stream_System_Threading_CancellationToken_"></a>

### ReadDataAsync(Stream, CancellationToken)


```csharp
public static IAsyncEnumerable<string> ReadDataAsync(Stream stream, CancellationToken cancellationToken = default)
```


Yields event data. A `data: [DONE]` line ends the stream.
Multi-line data fields are joined with newlines.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stream` | [Stream](https://learn.microsoft.com/dotnet/api/system.io.stream) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/SseParser.cs#L14)
