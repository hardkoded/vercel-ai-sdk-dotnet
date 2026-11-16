---
title: "UIMessageStreamResponse"
description: "Response whose body is a UI message stream encoded as Server-Sent Events."
editUrl: false
prev: false
next: false
sidebar:
  label: "UIMessageStreamResponse"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_UIMessageStreamResponse"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public sealed class UIMessageStreamResponse
```


Response whose body is a UI message stream encoded as Server-Sent Events.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateUIMessageStreamResponse.cs#L38)

## Constructors

<a id="Vercel_AI_AspNetCore_UIMessageStreamResponse__ctor_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamResponse__ctor_System_Int32_System_String_Vercel_AI_Util_HeaderCollection_Vercel_AI_Util_ReadableStream_System_Byte____"></a>

### `UIMessageStreamResponse(int, string?, HeaderCollection, ReadableStream<byte[]>)`


```csharp
public UIMessageStreamResponse(int status, string? statusText, HeaderCollection headers, ReadableStream<byte[]> body)
```


Creates the response.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `status` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `statusText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/) |  |
| `body` | `ReadableStream&lt;byte\[\]&gt;` |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateUIMessageStreamResponse.cs#L41)

## Properties

<a id="Vercel_AI_AspNetCore_UIMessageStreamResponse_Body_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamResponse_Body"></a>

### Body


```csharp
public ReadableStream<byte[]> Body { get; }
```


UTF-8 encoded SSE body chunks.

**Value:** `ReadableStream&lt;byte\[\]&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateUIMessageStreamResponse.cs#L59)

<a id="Vercel_AI_AspNetCore_UIMessageStreamResponse_Headers_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamResponse_Headers"></a>

### Headers


```csharp
public HeaderCollection Headers { get; }
```


Response headers, including the protocol headers the caller did not set.

**Value:** [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateUIMessageStreamResponse.cs#L56)

<a id="Vercel_AI_AspNetCore_UIMessageStreamResponse_Status_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamResponse_Status"></a>

### Status


```csharp
public int Status { get; }
```


HTTP status. Defaults to 200.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateUIMessageStreamResponse.cs#L50)

<a id="Vercel_AI_AspNetCore_UIMessageStreamResponse_StatusText_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamResponse_StatusText"></a>

### StatusText


```csharp
public string? StatusText { get; }
```


HTTP status text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateUIMessageStreamResponse.cs#L53)
