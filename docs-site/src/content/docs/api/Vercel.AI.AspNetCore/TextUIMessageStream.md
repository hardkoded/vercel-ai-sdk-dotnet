---
title: "TextUIMessageStream"
description: "Turns plain text into the UI message chunks a text-only client expects."
editUrl: false
prev: false
next: false
sidebar:
  label: "TextUIMessageStream"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_TextUIMessageStream"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public static class TextUIMessageStream
```


Turns plain text into the UI message chunks a text-only client expects.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/TextUIMessageStream.cs#L12)

## Methods

<a id="Vercel_AI_AspNetCore_TextUIMessageStream_ProcessAsync_"></a>
<a id="Vercel_AI_AspNetCore_TextUIMessageStream_ProcessAsync_System_Collections_Generic_IAsyncEnumerable_System_Byte____System_Func_System_String_System_Threading_Tasks_Task__System_Threading_CancellationToken_"></a>

### `ProcessAsync(IAsyncEnumerable<byte[]>, Func<string, Task>, CancellationToken)`


```csharp
public static Task ProcessAsync(IAsyncEnumerable<byte[]> chunks, Func<string, Task> onTextPart, CancellationToken cancellationToken = default)
```


Decodes UTF-8 chunks and passes each decoded piece to &lt;code class="paramref"&gt;onTextPart&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `chunks` | [IAsyncEnumerable&lt;byte\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1) | Encoded text pieces. An empty sequence does not call &lt;code class="paramref"&gt;onTextPart&lt;/code&gt;. |
| `onTextPart` | [Func&lt;string, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) | Receives one decoded piece. |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) | Cancels reading &lt;code class="paramref"&gt;chunks&lt;/code&gt;. |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/TextUIMessageStream.cs#L51)

<a id="Vercel_AI_AspNetCore_TextUIMessageStream_TransformAsync_"></a>
<a id="Vercel_AI_AspNetCore_TextUIMessageStream_TransformAsync_System_Collections_Generic_IAsyncEnumerable_System_String__System_Threading_CancellationToken_"></a>

### `TransformAsync(IAsyncEnumerable<string>, CancellationToken)`


```csharp
public static IAsyncEnumerable<JsonObject> TransformAsync(IAsyncEnumerable<string> text, CancellationToken cancellationToken = default)
```


Wraps &lt;code class="paramref"&gt;text&lt;/code&gt; in start, one text block with id `text-1`, and finish chunks.
An empty source still emits the surrounding chunks and no text delta.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [IAsyncEnumerable&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1) | Text pieces, in order. |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) | Cancels reading &lt;code class="paramref"&gt;text&lt;/code&gt;. |

**Returns:** [IAsyncEnumerable&lt;JsonObject&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/TextUIMessageStream.cs#L20)
