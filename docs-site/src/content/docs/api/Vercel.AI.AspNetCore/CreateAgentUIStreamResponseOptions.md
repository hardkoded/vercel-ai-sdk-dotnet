---
title: "CreateAgentUIStreamResponseOptions"
description: "Options of CancellationToken). Maps to the options of createAgentUIStreamResponse."
editUrl: false
prev: false
next: false
sidebar:
  label: "CreateAgentUIStreamResponseOptions"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public class CreateAgentUIStreamResponseOptions : CreateAgentUIStreamOptions
```


Options of `CancellationToken)`. Maps to the options of `createAgentUIStreamResponse`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [CreateAgentUIStreamOptions](/api/Vercel.AI.AspNetCore/CreateAgentUIStreamOptions/)

**Derived:** [PipeAgentUIStreamToResponseOptions](/api/Vercel.AI.AspNetCore/PipeAgentUIStreamToResponseOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStreamResponse.cs#L10)

## Properties

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_ConsumeSseStream_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_ConsumeSseStream"></a>

### ConsumeSseStream


```csharp
public Func<ReadableStream<string>, Task?>? ConsumeSseStream { get; set; }
```


Consumes a copy of the SSE stream independently. Maps to `consumeSseStream`.

**Value:** [Func&lt;ReadableStream&lt;string&gt;, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStreamResponse.cs#L25)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_Headers_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_Headers"></a>

### Headers


```csharp
public HeaderCollection? Headers { get; set; }
```


Additional headers for the response. Maps to `headers`.

**Value:** [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStreamResponse.cs#L13)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_KeepAliveMs_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_KeepAliveMs"></a>

### KeepAliveMs


```csharp
public int? KeepAliveMs { get; set; }
```


Optional interval in milliseconds for sending SSE keep-alive comments. Maps to `keepAliveMs`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStreamResponse.cs#L22)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_Status_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_Status"></a>

### Status


```csharp
public int? Status { get; set; }
```


The status code for the response. Maps to `status`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStreamResponse.cs#L16)

<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_StatusText_"></a>
<a id="Vercel_AI_AspNetCore_CreateAgentUIStreamResponseOptions_StatusText"></a>

### StatusText


```csharp
public string? StatusText { get; set; }
```


The status text for the response. Maps to `statusText`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateAgentUIStreamResponse.cs#L19)
