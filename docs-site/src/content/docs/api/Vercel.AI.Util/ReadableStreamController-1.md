---
title: "ReadableStreamController<T>"
description: "Enqueues chunks into a ReadableStream%601."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReadableStreamController<T>"
  hidden: true
---

<a id="Vercel_AI_Util_ReadableStreamController_1"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class ReadableStreamController<T>
```


Enqueues chunks into a `ReadableStream%601`.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L86)

## Methods

<a id="Vercel_AI_Util_ReadableStreamController_1_Close_"></a>
<a id="Vercel_AI_Util_ReadableStreamController_1_Close"></a>

### Close()


```csharp
public void Close()
```


Closes the stream.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L102)

<a id="Vercel_AI_Util_ReadableStreamController_1_Enqueue_"></a>
<a id="Vercel_AI_Util_ReadableStreamController_1_Enqueue__0_"></a>

### Enqueue(T)


```csharp
public void Enqueue(T chunk)
```


Queues &lt;code class="paramref"&gt;chunk&lt;/code&gt; for the next reader.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `chunk` | `T` |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L96)

<a id="Vercel_AI_Util_ReadableStreamController_1_Error_"></a>
<a id="Vercel_AI_Util_ReadableStreamController_1_Error_System_Object_"></a>

### Error(object?)


```csharp
public void Error(object? reason)
```


Fails the stream. &lt;code class="paramref"&gt;reason&lt;/code&gt; may be `null`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `reason` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L108)
