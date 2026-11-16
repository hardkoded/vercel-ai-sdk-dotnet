---
title: "UIMessageStreamResponseInit"
description: "Options for a UI message stream response. Maps to UIMessageStreamResponseInit."
editUrl: false
prev: false
next: false
sidebar:
  label: "UIMessageStreamResponseInit"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_UIMessageStreamResponseInit"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public sealed class UIMessageStreamResponseInit
```


Options for a UI message stream response. Maps to `UIMessageStreamResponseInit`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateUIMessageStreamResponse.cs#L12)

## Properties

<a id="Vercel_AI_AspNetCore_UIMessageStreamResponseInit_ConsumeSseStream_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamResponseInit_ConsumeSseStream"></a>

### ConsumeSseStream


```csharp
public Func<ReadableStream<string>, Task?>? ConsumeSseStream { get; set; }
```


Optional callback to consume a copy of the SSE stream independently.
This is useful for logging, debugging, or processing the stream in parallel.
The callback receives a tee'd copy of the stream and does not block the response.

**Value:** [Func&lt;ReadableStream&lt;string&gt;, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateUIMessageStreamResponse.cs#L34)

<a id="Vercel_AI_AspNetCore_UIMessageStreamResponseInit_Headers_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamResponseInit_Headers"></a>

### Headers


```csharp
public HeaderCollection? Headers { get; set; }
```


Additional HTTP headers. They take precedence over the protocol headers.

**Value:** [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateUIMessageStreamResponse.cs#L21)

<a id="Vercel_AI_AspNetCore_UIMessageStreamResponseInit_KeepAliveMs_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamResponseInit_KeepAliveMs"></a>

### KeepAliveMs


```csharp
public int? KeepAliveMs { get; set; }
```


Optional interval in milliseconds for sending SSE keep-alive comments.
When set, an opening comment is sent immediately and additional comments are sent after the stream has been idle for the configured interval.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateUIMessageStreamResponse.cs#L27)

<a id="Vercel_AI_AspNetCore_UIMessageStreamResponseInit_Status_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamResponseInit_Status"></a>

### Status


```csharp
public int? Status { get; set; }
```


The HTTP status code. Defaults to 200.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateUIMessageStreamResponse.cs#L15)

<a id="Vercel_AI_AspNetCore_UIMessageStreamResponseInit_StatusText_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamResponseInit_StatusText"></a>

### StatusText


```csharp
public string? StatusText { get; set; }
```


The HTTP status text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateUIMessageStreamResponse.cs#L18)
