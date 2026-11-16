---
title: "ToUIMessageChunkOptions"
description: "Options of ToUIMessageChunkOptions) and CancellationToken). Maps to ToUIMessageChunkOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToUIMessageChunkOptions"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public sealed class ToUIMessageChunkOptions
```


Options of `ToUIMessageChunkOptions)` and `CancellationToken)`. Maps to `ToUIMessageChunkOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L15)

## Properties

<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_OnEnd_"></a>
<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_OnEnd"></a>

### OnEnd


```csharp
public Func<UIMessageStreamOnEndEvent, Task>? OnEnd { get; init; }
```


Called when the stream ends, whether it completed, failed, was aborted, or the consumer cancelled it. Maps to `onEnd`.

**Value:** [Func&lt;UIMessageStreamOnEndEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L54)

<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_OnError_"></a>
<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_OnError"></a>

### OnError


```csharp
public Func<Exception, string>? OnError { get; init; }
```


Formats stream errors. Also formats invalid tool calls and application-executed tool errors when [RedactToolErrors](#Vercel_AI_AspNetCore_ToUIMessageChunkOptions_RedactToolErrors) is enabled.
Provider-executed tool execution errors bypass this callback so their original error data survives model-message round trips.
Null writes `An error occurred.`. The argument is the error. Maps to `onError`.

**Value:** [Func&lt;Exception, string&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L31)

<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_OnStepEnd_"></a>
<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_OnStepEnd"></a>

### OnStepEnd


```csharp
public Func<UIMessageStreamOnStepEndEvent, Task>? OnStepEnd { get; init; }
```


Called when each step ends during multi-step agent runs. Maps to `onStepEnd`.

**Value:** [Func&lt;UIMessageStreamOnStepEndEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L49)

<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_OriginalMessages_"></a>
<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_OriginalMessages"></a>

### OriginalMessages


```csharp
public IReadOnlyList<UIMessage>? OriginalMessages { get; init; }
```


The messages of the conversation. When the last one is an assistant message, the response continues it and reuses its id.
Maps to `originalMessages`.

**Value:** [IReadOnlyList&lt;UIMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L46)

<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_RedactToolErrors_"></a>
<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_RedactToolErrors"></a>

### RedactToolErrors


```csharp
public bool? RedactToolErrors { get; init; }
```


Replaces the error text of failed tool calls with the result of [OnError](#Vercel_AI_AspNetCore_ToUIMessageChunkOptions_OnError).
Defaults to `true` when [OnError](#Vercel_AI_AspNetCore_ToUIMessageChunkOptions_OnError) is provided and to `false` otherwise.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L37)

<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_ResponseMessageId_"></a>
<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_ResponseMessageId"></a>

### ResponseMessageId


```csharp
public string? ResponseMessageId { get; init; }
```


Id of the response message sent with the `start` chunk. Maps to `responseMessageId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L40)

<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_SendFinish_"></a>
<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_SendFinish"></a>

### SendFinish


```csharp
public bool SendFinish { get; init; }
```


Whether the `finish` chunk is sent. Defaults to `true`. Maps to `sendFinish`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L24)

<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_SendReasoning_"></a>
<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_SendReasoning"></a>

### SendReasoning


```csharp
public bool SendReasoning { get; init; }
```


Whether reasoning parts are sent. Defaults to `true`. Maps to `sendReasoning`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L18)

<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_SendStart_"></a>
<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_SendStart"></a>

### SendStart


```csharp
public bool SendStart { get; init; }
```


Whether the `start` chunk is sent. Defaults to `true`. Maps to `sendStart`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L21)
