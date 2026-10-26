---
title: "TextStreamResponse"
description: "HTTP response whose body is a UTF-8 text stream."
editUrl: false
prev: false
next: false
sidebar:
  label: "TextStreamResponse"
  hidden: true
---

<a id="Vercel_AI_Util_TextStreamResponse"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class TextStreamResponse
```


HTTP response whose body is a UTF-8 text stream.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/TextStream.cs#L35)

## Constructors

<a id="Vercel_AI_Util_TextStreamResponse__ctor_"></a>
<a id="Vercel_AI_Util_TextStreamResponse__ctor_System_Int32_System_String_Vercel_AI_Util_HeaderCollection_Vercel_AI_Util_ReadableStream_System_Byte____"></a>

### `TextStreamResponse(int, string?, HeaderCollection, ReadableStream<byte[]>)`


```csharp
public TextStreamResponse(int status, string? statusText, HeaderCollection headers, ReadableStream<byte[]> body)
```


Creates the response.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `status` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `statusText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/) |  |
| `body` | `ReadableStream&lt;byte\[\]&gt;` |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/TextStream.cs#L38)

## Properties

<a id="Vercel_AI_Util_TextStreamResponse_Body_"></a>
<a id="Vercel_AI_Util_TextStreamResponse_Body"></a>

### Body


```csharp
public ReadableStream<byte[]> Body { get; }
```


UTF-8 encoded body chunks.

**Value:** `ReadableStream&lt;byte\[\]&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/TextStream.cs#L56)

<a id="Vercel_AI_Util_TextStreamResponse_Headers_"></a>
<a id="Vercel_AI_Util_TextStreamResponse_Headers"></a>

### Headers


```csharp
public HeaderCollection Headers { get; }
```


Response headers, including the default content type when the caller did not set one.

**Value:** [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/TextStream.cs#L53)

<a id="Vercel_AI_Util_TextStreamResponse_Status_"></a>
<a id="Vercel_AI_Util_TextStreamResponse_Status"></a>

### Status


```csharp
public int Status { get; }
```


HTTP status. Defaults to 200.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/TextStream.cs#L47)

<a id="Vercel_AI_Util_TextStreamResponse_StatusText_"></a>
<a id="Vercel_AI_Util_TextStreamResponse_StatusText"></a>

### StatusText


```csharp
public string? StatusText { get; }
```


HTTP status text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/TextStream.cs#L50)
