---
title: "ToUIMessageChunkOptions"
description: "Options of ToUIMessageChunkOptions). Maps to ToUIMessageChunkOptions."
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


Options of `ToUIMessageChunkOptions)`. Maps to `ToUIMessageChunkOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L16)

## Properties

<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_MessageMetadata_"></a>
<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_MessageMetadata"></a>

### MessageMetadata


```csharp
public JsonElement? MessageMetadata { get; init; }
```


Metadata of the response message, sent with the `start` and `finish` chunks. A value, as upstream's `toUIMessageChunk` takes it. Maps to `messageMetadata`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L47)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L38)

<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_RedactToolErrors_"></a>
<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_RedactToolErrors"></a>

### RedactToolErrors


```csharp
public bool? RedactToolErrors { get; init; }
```


Replaces the error text of failed tool calls with the result of [OnError](#Vercel_AI_AspNetCore_ToUIMessageChunkOptions_OnError).
Defaults to `true` when [OnError](#Vercel_AI_AspNetCore_ToUIMessageChunkOptions_OnError) is provided and to `false` otherwise.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L44)

<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_ResponseMessageId_"></a>
<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_ResponseMessageId"></a>

### ResponseMessageId


```csharp
public string? ResponseMessageId { get; init; }
```


Id of the response message sent with the `start` chunk. Maps to `responseMessageId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L50)

<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_SendFinish_"></a>
<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_SendFinish"></a>

### SendFinish


```csharp
public bool SendFinish { get; init; }
```


Whether the `finish` chunk is sent. Defaults to `true`. Maps to `sendFinish`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L31)

<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_SendReasoning_"></a>
<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_SendReasoning"></a>

### SendReasoning


```csharp
public bool SendReasoning { get; init; }
```


Whether reasoning parts are sent. Defaults to `true`. Maps to `sendReasoning`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L22)

<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_SendSources_"></a>
<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_SendSources"></a>

### SendSources


```csharp
public bool SendSources { get; init; }
```


Whether source parts are sent. Defaults to `false`. Maps to `sendSources`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L25)

<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_SendStart_"></a>
<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_SendStart"></a>

### SendStart


```csharp
public bool SendStart { get; init; }
```


Whether the `start` chunk is sent. Defaults to `true`. Maps to `sendStart`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L28)

<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_Tools_"></a>
<a id="Vercel_AI_AspNetCore_ToUIMessageChunkOptions_Tools"></a>

### Tools


```csharp
public IReadOnlyList<Tool>? Tools { get; init; }
```


The tools of the call. A call, input, result or error of a dynamic tool is marked as dynamic. Maps to `tools`.

**Value:** [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ToUIMessageChunk.cs#L19)
