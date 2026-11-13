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

<a id="Vercel_AI_AspNetCore_UIMessageStreamExtensions_ToUIMessageStreamResult_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamExtensions_ToUIMessageStreamResult_Vercel_AI_StreamTextResult_System_Nullable_System_Int32__System_Func_System_String_System_String__System_Nullable_System_Boolean__"></a>

### `ToUIMessageStreamResult(StreamTextResult, int?, Func<string, string>?, bool?)`


```csharp
public static IResult ToUIMessageStreamResult(this StreamTextResult result, int? keepAliveMs = null, Func<string, string>? onError = null, bool? redactToolErrors = null)
```


Returns a `text/event-stream` result a JavaScript `useChat` client can consume.
Emitted chunks are start, text, reasoning, tool input/output, tool approval request, source-url, step boundaries, finish, abort, and error.
Finish reasons use the protocol values `stop`, `length`, `content-filter`, `tool-calls`, `error`, and `other`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `result` | [StreamTextResult](/api/Vercel.AI/StreamTextResult/) | The stream to write. |
| `keepAliveMs` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) | Optional idle interval, in milliseconds. Null sends no SSE comments. A positive value up to 2147483647 writes `: stream-open` before the start event and `: keep-alive` while the next part is still pending. |
| `onError` | [Func&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) | Optional mapper for `error` chunk text. Null writes `An error occurred.`. It also formats failed tool calls when `redactToolErrors` is enabled, which is the default when this callback is provided. Errors of provider-executed tools are not masked. |
| `redactToolErrors` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) | Replaces the error text of failed tool calls (tool execution errors and invalid tool calls) with the result of `onError`. By default, the client receives the same error text as the model. The UI messages are sent back to the model on the next request, so a different text changes the message history that the model's reasoning and answers are based on. Some models reject such a history, for example Anthropic models with preserved thinking. To keep error details away from the model and the client, throw a safe error from the tool. Defaults to `true` when `onError` is provided, so that an existing `onError` keeps handling tool errors, and to `false` otherwise. |

**Returns:** [IResult](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.iresult)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/UIMessageStream.cs#L41)
