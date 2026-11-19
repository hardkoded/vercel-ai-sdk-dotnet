---
title: "CreateAgentUIStreamResponseOptions<TCallOptions>"
description: "Options of CancellationToken). Maps to the options of createAgentUIStreamResponse with a CALLOPTIONS type."
editUrl: false
prev: false
next: false
sidebar:
  label: "CreateAgentUIStreamResponseOptions<TCallOptions>"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_1"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public class CreateAgentUIStreamResponseOptions<TCallOptions> : CreateAgentUIStreamOptions<TCallOptions>
```


Options of `CancellationToken)`. Maps to the options of `createAgentUIStreamResponse` with a `CALL_OPTIONS` type.

**Type parameters**

| Name | Description |
| --- | --- |
| `TCallOptions` | The type of the call options of the agent. Maps to `CALL_OPTIONS`. |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [CreateAgentUIStreamOptionsBase](/api/Vercel.AI.AspNetCore/CreateAgentUIStreamOptionsBase/) → `CreateAgentUIStreamOptions&lt;TCallOptions&gt;`

**Derived:** [PipeAgentUIStreamToResponseOptions&lt;TCallOptions&gt;](/api/Vercel.AI.AspNetCore/PipeAgentUIStreamToResponseOptions-1/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStreamResponse.cs#L40)

## Properties

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_1_ConsumeSseStream_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_1_ConsumeSseStream"></a>

### ConsumeSseStream


```csharp
public Func<ReadableStream<string>, Task?>? ConsumeSseStream { get; set; }
```


Consumes a copy of the SSE stream independently. Maps to `consumeSseStream`.

**Value:** [Func&lt;ReadableStream&lt;string&gt;, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStreamResponse.cs#L56)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_1_Headers_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_1_Headers"></a>

### Headers


```csharp
public HeaderCollection? Headers { get; set; }
```


Additional headers for the response. Maps to `headers`.

**Value:** [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStreamResponse.cs#L44)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_1_KeepAliveMs_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_1_KeepAliveMs"></a>

### KeepAliveMs


```csharp
public int? KeepAliveMs { get; set; }
```


Optional interval in milliseconds for sending SSE keep-alive comments. Maps to `keepAliveMs`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStreamResponse.cs#L53)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_1_Status_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_1_Status"></a>

### Status


```csharp
public int? Status { get; set; }
```


The status code for the response. Maps to `status`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStreamResponse.cs#L47)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_1_StatusText_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_1_StatusText"></a>

### StatusText


```csharp
public string? StatusText { get; set; }
```


The status text for the response. Maps to `statusText`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStreamResponse.cs#L50)
