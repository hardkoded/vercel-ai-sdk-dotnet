---
title: "StitchableStream<T>"
description: "A stream that reads inner streams one at a time."
editUrl: false
prev: false
next: false
sidebar:
  label: "StitchableStream<T>"
  hidden: true
---

<a id="Vercel_AI_Util_StitchableStream_1"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class StitchableStream<T>
```


A stream that reads inner streams one at a time.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/StitchableStream.cs#L8)

## Constructors

<a id="Vercel_AI_Util_StitchableStream_1__ctor_"></a>
<a id="Vercel_AI_Util_StitchableStream_1__ctor"></a>

### StitchableStream()


```csharp
public StitchableStream()
```


Creates an empty stitchable stream.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/StitchableStream.cs#L18)

## Properties

<a id="Vercel_AI_Util_StitchableStream_1_Stream_"></a>
<a id="Vercel_AI_Util_StitchableStream_1_Stream"></a>

### Stream


```csharp
public ReadableStream<T> Stream { get; }
```


Outer stream.

**Value:** `ReadableStream&lt;T&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/StitchableStream.cs#L24)

## Methods

<a id="Vercel_AI_Util_StitchableStream_1_AddStream_"></a>
<a id="Vercel_AI_Util_StitchableStream_1_AddStream_Vercel_AI_Util_ReadableStream__0__System_Action_System_Object__System_Action_"></a>

### `AddStream(ReadableStream<T>?, Action<object>?, Action?)`


```csharp
public void AddStream(ReadableStream<T>? inner, Action<object>? onError = null, Action? onCancel = null)
```


Queues &lt;code class="paramref"&gt;inner&lt;/code&gt; to be read after the streams already added.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `inner` | `ReadableStream&lt;T&gt;` |  |
| `onError` | [Action&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |
| `onCancel` | [Action](https://learn.microsoft.com/dotnet/api/system.action) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/StitchableStream.cs#L30)

<a id="Vercel_AI_Util_StitchableStream_1_Close_"></a>
<a id="Vercel_AI_Util_StitchableStream_1_Close"></a>

### Close()


```csharp
public void Close()
```


Lets current inner streams finish, then closes the outer stream.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/StitchableStream.cs#L62)

<a id="Vercel_AI_Util_StitchableStream_1_Terminate_"></a>
<a id="Vercel_AI_Util_StitchableStream_1_Terminate"></a>

### Terminate()


```csharp
public void Terminate()
```


Cancels every inner stream and closes the outer stream.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/StitchableStream.cs#L81)
