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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/PipeUIMessageStreamToResponse.cs#L10)

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

<a id="Vercel_AI_AspNetCore_UIMessageStreamResponses_PipeUIMessageStreamToResponse_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamResponses_PipeUIMessageStreamToResponse_Vercel_AI_Util_ServerResponse_Vercel_AI_Util_ReadableStream_System_Text_Json_Nodes_JsonObject__Vercel_AI_AspNetCore_UIMessageStreamResponseInit_System_Threading_CancellationToken_"></a>

### `PipeUIMessageStreamToResponse(ServerResponse, ReadableStream<JsonObject>, UIMessageStreamResponseInit?, CancellationToken)`


```csharp
public static Task PipeUIMessageStreamToResponse(ServerResponse response, ReadableStream<JsonObject> stream, UIMessageStreamResponseInit? init = null, CancellationToken cancellationToken = default)
```


Pipes a UI message stream, encoded as Server-Sent Events, to a [ServerResponse](/api/Vercel.AI.Util/ServerResponse/). Maps to `pipeUIMessageStreamToResponse`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [ServerResponse](/api/Vercel.AI.Util/ServerResponse/) | The response to write to. |
| `stream` | `ReadableStream&lt;JsonObject&gt;` | The UI message chunk stream to send. |
| `init` | [UIMessageStreamResponseInit](/api/Vercel.AI.AspNetCore/UIMessageStreamResponseInit/) | Status, headers, keep-alive interval, and the optional SSE stream consumer. |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) | Cancels the write. |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/PipeUIMessageStreamToResponse.cs#L19)
