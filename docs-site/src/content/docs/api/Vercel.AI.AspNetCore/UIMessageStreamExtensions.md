---
title: "UIMessageStreamExtensions"
description: "Writes a StreamTextResult as an AI SDK UI message stream."
editUrl: false
prev: false
next: false
sidebar:
  label: "UIMessageStreamExtensions"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_UIMessageStreamExtensions"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public static class UIMessageStreamExtensions
```


Writes a [StreamTextResult](/api/Vercel.AI/StreamTextResult/) as an AI SDK UI message stream.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStream.cs#L18)

## Methods

<a id="Vercel_AI_AspNetCore_UIMessageStreamExtensions_PipeUIMessageStreamToResponseAsync_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamExtensions_PipeUIMessageStreamToResponseAsync_Vercel_AI_StreamTextResult_Vercel_AI_Util_ServerResponse_System_Nullable_System_Int32__System_String_Vercel_AI_Util_HeaderCollection_System_Nullable_System_Int32__System_Func_System_Exception_System_String__System_Nullable_System_Boolean__System_Threading_CancellationToken_System_Boolean_"></a>

### `PipeUIMessageStreamToResponseAsync(StreamTextResult, ServerResponse, int?, string?, HeaderCollection?, int?, Func<Exception, string>?, bool?, CancellationToken, bool)`


```csharp
public static Task PipeUIMessageStreamToResponseAsync(this StreamTextResult result, ServerResponse response, int? status = null, string? statusText = null, HeaderCollection? headers = null, int? keepAliveMs = null, Func<Exception, string>? onError = null, bool? redactToolErrors = null, CancellationToken cancellationToken = default, bool sendSources = false)
```


Pipes the UI message stream to a [ServerResponse](/api/Vercel.AI.Util/ServerResponse/) as Server-Sent Events.
Mirrors upstream `pipeUIMessageStreamToResponse`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `result` | [StreamTextResult](/api/Vercel.AI/StreamTextResult/) | The stream to write. |
| `response` | [ServerResponse](/api/Vercel.AI.Util/ServerResponse/) | The response to write to. |
| `status` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) | The HTTP status code. Defaults to 200. |
| `statusText` | [string](https://learn.microsoft.com/dotnet/api/system.string) | The HTTP status text. |
| `headers` | [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/) | Additional HTTP headers. They take precedence over the protocol headers. |
| `keepAliveMs` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) | Optional idle interval, in milliseconds. Null sends no SSE comments. |
| `onError` | [Func&lt;Exception, string&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) | Optional mapper from the error to the `error` chunk text. Null writes `An error occurred.`. |
| `redactToolErrors` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) | Replaces the error text of failed tool calls with the result of `onError`. Defaults to `true` when `onError` is provided and to `false` otherwise. |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) | Cancels the write. |
| `sendSources` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | Whether source parts are sent to the client. Defaults to `false`. Maps to `sendSources`. |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task) — A task that completes when the stream has been written and the response has ended.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStream.cs#L79)

<a id="Vercel_AI_AspNetCore_UIMessageStreamExtensions_ToUIMessageStreamResult_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamExtensions_ToUIMessageStreamResult_Vercel_AI_StreamTextResult_System_Nullable_System_Int32__System_Func_System_Exception_System_String__System_Nullable_System_Boolean__System_Boolean_"></a>

### `ToUIMessageStreamResult(StreamTextResult, int?, Func<Exception, string>?, bool?, bool)`


```csharp
public static IResult ToUIMessageStreamResult(this StreamTextResult result, int? keepAliveMs = null, Func<Exception, string>? onError = null, bool? redactToolErrors = null, bool sendSources = false)
```


Returns a `text/event-stream` result a JavaScript `useChat` client can consume.
Emitted chunks are start, text, reasoning, tool input/output, tool approval request, source-url, step boundaries, finish, abort, and error.
Finish reasons use the protocol values `stop`, `length`, `content-filter`, `tool-calls`, `error`, and `other`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `result` | [StreamTextResult](/api/Vercel.AI/StreamTextResult/) | The stream to write. |
| `keepAliveMs` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) | Optional idle interval, in milliseconds. Null sends no SSE comments. A positive value up to 2147483647 writes `: stream-open` before the start event and `: keep-alive` while the next part is still pending. |
| `onError` | [Func&lt;Exception, string&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) | Optional mapper from the error to the `error` chunk text. Null writes `An error occurred.`. It also formats failed tool calls when `redactToolErrors` is enabled, which is the default when this callback is provided. Errors of provider-executed tools are not masked. |
| `redactToolErrors` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) | Replaces the error text of failed tool calls (tool execution errors and invalid tool calls) with the result of `onError`. By default, the client receives the same error text as the model. The UI messages are sent back to the model on the next request, so a different text changes the message history that the model's reasoning and answers are based on. Some models reject such a history, for example Anthropic models with preserved thinking. To keep error details away from the model and the client, throw a safe error from the tool. Defaults to `true` when `onError` is provided, so that an existing `onError` keeps handling tool errors, and to `false` otherwise. |
| `sendSources` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | Whether source parts are sent to the client. Defaults to `false`. Maps to `sendSources`. |

**Returns:** [IResult](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.iresult)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStream.cs#L42)
