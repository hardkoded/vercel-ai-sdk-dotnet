---
title: "ReadUIMessageStreamOptions"
description: "Options of CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReadUIMessageStreamOptions"
  hidden: true
---

<a id="Vercel_AI_AspNetCore_ReadUIMessageStreamOptions"></a>

Namespace: [Vercel.AI.AspNetCore](/api/Vercel.AI.AspNetCore/)


```csharp
public sealed class ReadUIMessageStreamOptions
```


Options of `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ReadUIMessageStream.cs#L12)

## Constructors

<a id="Vercel_AI_AspNetCore_ReadUIMessageStreamOptions__ctor_"></a>
<a id="Vercel_AI_AspNetCore_ReadUIMessageStreamOptions__ctor_System_Collections_Generic_IAsyncEnumerable_System_Text_Json_Nodes_JsonObject__"></a>

### `ReadUIMessageStreamOptions(IAsyncEnumerable<JsonObject>)`


```csharp
public ReadUIMessageStreamOptions(IAsyncEnumerable<JsonObject> stream)
```


Creates options that read &lt;code class="paramref"&gt;stream&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stream` | [IAsyncEnumerable&lt;JsonObject&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ReadUIMessageStream.cs#L15)

## Properties

<a id="Vercel_AI_AspNetCore_ReadUIMessageStreamOptions_Message_"></a>
<a id="Vercel_AI_AspNetCore_ReadUIMessageStreamOptions_Message"></a>

### Message


```csharp
public UIMessage? Message { get; init; }
```


The last assistant message to use as a starting point when the conversation is resumed. Otherwise null. Maps to `message`.

**Value:** [UIMessage](/api/Vercel.AI.Ui/UIMessage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ReadUIMessageStream.cs#L24)

<a id="Vercel_AI_AspNetCore_ReadUIMessageStreamOptions_OnError_"></a>
<a id="Vercel_AI_AspNetCore_ReadUIMessageStreamOptions_OnError"></a>

### OnError


```csharp
public Action<Exception>? OnError { get; init; }
```


Called when an error occurs. Maps to `onError`.

**Value:** [Action&lt;Exception&gt;](https://learn.microsoft.com/dotnet/api/system.action-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ReadUIMessageStream.cs#L27)

<a id="Vercel_AI_AspNetCore_ReadUIMessageStreamOptions_Stream_"></a>
<a id="Vercel_AI_AspNetCore_ReadUIMessageStreamOptions_Stream"></a>

### Stream


```csharp
public IAsyncEnumerable<JsonObject> Stream { get; }
```


The stream of UI message chunks to read. Maps to `stream`.

**Value:** [IAsyncEnumerable&lt;JsonObject&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ReadUIMessageStream.cs#L21)

<a id="Vercel_AI_AspNetCore_ReadUIMessageStreamOptions_TerminateOnError_"></a>
<a id="Vercel_AI_AspNetCore_ReadUIMessageStreamOptions_TerminateOnError"></a>

### TerminateOnError


```csharp
public bool TerminateOnError { get; init; }
```


Whether to terminate the stream if an error occurs. Maps to `terminateOnError`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AspNetCore/ReadUIMessageStream.cs#L30)
