---
title: "UIMessageStreamOptions"
description: "Options of CancellationToken). Maps to UIMessageStreamOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "UIMessageStreamOptions"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public sealed class UIMessageStreamOptions
```


Options of `CancellationToken)`. Maps to `UIMessageStreamOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageStream.cs#L17)

## Properties

<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_GenerateMessageId_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_GenerateMessageId"></a>

### GenerateMessageId


```csharp
public Func<string>? GenerateMessageId { get; init; }
```


Generates the id of the response message, unless the last original message is an assistant message, whose id is reused.
Without it, the `start` chunk has no `messageId`. Maps to `generateMessageId`.

**Value:** [Func&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageStream.cs#L63)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_MessageMetadata_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_MessageMetadata"></a>

### MessageMetadata


```csharp
public Func<TextStreamPart, JsonElement?>? MessageMetadata { get; init; }
```


Called for each stream part. Its value is sent with the `start` and `finish` chunks, and with a separate `message-metadata` chunk for any other part.
Maps to the function form of `messageMetadata`.

**Value:** [Func&lt;TextStreamPart, JsonElement?&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageStream.cs#L51)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_OnEnd_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_OnEnd"></a>

### OnEnd


```csharp
public Func<UIMessageStreamOnEndEvent, Task>? OnEnd { get; init; }
```


Called when the stream ends, whether it completed, failed, was aborted, or the consumer cancelled it. Maps to `onEnd`.

**Value:** [Func&lt;UIMessageStreamOnEndEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageStream.cs#L75)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_OnError_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_OnError"></a>

### OnError


```csharp
public Func<Exception, string>? OnError { get; init; }
```


Formats stream errors. Also formats invalid tool calls and application-executed tool errors when [RedactToolErrors](#Vercel_AI_AspNetCore_UIMessageStreamOptions_RedactToolErrors) is enabled.
Provider-executed tool execution errors bypass this callback so their original error data survives model-message round trips.
Null writes `An error occurred.`. Maps to `onError`.

**Value:** [Func&lt;Exception, string&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageStream.cs#L39)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_OnFinish_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_OnFinish"></a>

### OnFinish


```csharp
[Obsolete("Use OnEnd instead.")]
public Func<UIMessageStreamOnEndEvent, Task>? OnFinish { get; init; }
```


Called when the stream ends. Maps to the deprecated `onFinish`. Use [OnEnd](#Vercel_AI_AspNetCore_UIMessageStreamOptions_OnEnd) instead.

**Value:** [Func&lt;UIMessageStreamOnEndEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageStream.cs#L78)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_OnStepEnd_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_OnStepEnd"></a>

### OnStepEnd


```csharp
public Func<UIMessageStreamOnStepEndEvent, Task>? OnStepEnd { get; init; }
```


Called when each step ends during multi-step agent runs. Maps to `onStepEnd`.

**Value:** [Func&lt;UIMessageStreamOnStepEndEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageStream.cs#L66)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_OnStepFinish_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_OnStepFinish"></a>

### OnStepFinish


```csharp
[Obsolete("Use OnStepEnd instead.")]
public Func<UIMessageStreamOnStepEndEvent, Task>? OnStepFinish { get; init; }
```


Called when each step ends during multi-step agent runs. Maps to the deprecated `onStepFinish`. Use [OnStepEnd](#Vercel_AI_AspNetCore_UIMessageStreamOptions_OnStepEnd) instead.

**Value:** [Func&lt;UIMessageStreamOnStepEndEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageStream.cs#L69)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_OriginalMessages_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_OriginalMessages"></a>

### OriginalMessages


```csharp
public IReadOnlyList<UIMessage>? OriginalMessages { get; init; }
```


The messages of the conversation. When the last one is an assistant message, the response continues it and reuses its id.
Maps to `originalMessages`.

**Value:** [IReadOnlyList&lt;UIMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageStream.cs#L57)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_RedactToolErrors_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_RedactToolErrors"></a>

### RedactToolErrors


```csharp
public bool? RedactToolErrors { get; init; }
```


Replaces the error text of failed tool calls with the result of [OnError](#Vercel_AI_AspNetCore_UIMessageStreamOptions_OnError).
Defaults to `true` when [OnError](#Vercel_AI_AspNetCore_UIMessageStreamOptions_OnError) is provided and to `false` otherwise. Maps to `redactToolErrors`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageStream.cs#L45)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_SendFinish_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_SendFinish"></a>

### SendFinish


```csharp
public bool SendFinish { get; init; }
```


Whether the `finish` chunk is sent. Defaults to `true`. Maps to `sendFinish`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageStream.cs#L32)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_SendReasoning_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_SendReasoning"></a>

### SendReasoning


```csharp
public bool SendReasoning { get; init; }
```


Whether reasoning parts are sent. Defaults to `true`. Maps to `sendReasoning`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageStream.cs#L23)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_SendSources_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_SendSources"></a>

### SendSources


```csharp
public bool SendSources { get; init; }
```


Whether source parts are sent. Defaults to `false`. Maps to `sendSources`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageStream.cs#L26)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_SendStart_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_SendStart"></a>

### SendStart


```csharp
public bool SendStart { get; init; }
```


Whether the `start` chunk is sent. Defaults to `true`. Maps to `sendStart`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageStream.cs#L29)

<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_Tools_"></a>
<a id="Vercel_AI_AspNetCore_UIMessageStreamOptions_Tools"></a>

### Tools


```csharp
public IReadOnlyList<Tool>? Tools { get; init; }
```


The tools of the call. A call, input, result or error of a dynamic tool is marked as dynamic. Maps to the `tools` argument of `toUIMessageStream`.

**Value:** [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageStream.cs#L20)
