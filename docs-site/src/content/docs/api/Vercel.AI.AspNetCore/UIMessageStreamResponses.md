---
title: "UIMessageStreamResponses"
description: "Builds UI message stream responses. Maps to createUIMessageStreamResponse."
editUrl: false
prev: false
next: false
sidebar:
  label: "UIMessageStreamResponses"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_UIMessageStreamResponses"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public static class UIMessageStreamResponses
```


Builds UI message stream responses. Maps to `createUIMessageStreamResponse`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateUIMessageStreamResponse.cs#L68)

## Methods

<a id="Vercel_AI_AspNetCore_UIMessageStreamResponses_CreateUIMessageStreamResponse_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamResponses_CreateUIMessageStreamResponse_Vercel_AI_Util_ReadableStream_System_Text_Json_Nodes_JsonObject__Vercel_AI_AspNetCore_UIMessageStreamResponseInit_"></a>

### `CreateUIMessageStreamResponse(ReadableStream<JsonObject>, UIMessageStreamResponseInit?)`


```csharp
public static UIMessageStreamResponse CreateUIMessageStreamResponse(ReadableStream<JsonObject> stream, UIMessageStreamResponseInit? init = null)
```


Creates a response from a UI message stream. The stream is transformed to Server-Sent Events (SSE) format.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stream` | `ReadableStream&lt;JsonObject&gt;` | The UI message chunk stream to send. |
| `init` | [UIMessageStreamResponseInit](/api/Vercel.AI.AspNetCore/UIMessageStreamResponseInit/) | Status, headers, keep-alive interval, and the optional SSE stream consumer. |

**Returns:** [UIMessageStreamResponse](/api/Vercel.AI.AspNetCore/UIMessageStreamResponse/) — A response with the UI message stream as the body.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/CreateUIMessageStreamResponse.cs#L76)
