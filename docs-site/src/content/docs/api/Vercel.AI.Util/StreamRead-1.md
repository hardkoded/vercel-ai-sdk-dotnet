---
title: "StreamRead<T>"
description: "One read from a ReadableStream%601."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamRead<T>"
  hidden: true
---

<a id="Vercel_AI_Util_StreamRead_1"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public struct StreamRead<T>
```


One read from a `ReadableStream%601`.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L8)

## Properties

<a id="Vercel_AI_Util_StreamRead_1_Done_"></a>
<a id="Vercel_AI_Util_StreamRead_1_Done"></a>

### Done


```csharp
public bool Done { readonly get; set; }
```


The stream is exhausted.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L11)

<a id="Vercel_AI_Util_StreamRead_1_HasValue_"></a>
<a id="Vercel_AI_Util_StreamRead_1_HasValue"></a>

### HasValue


```csharp
public bool HasValue { readonly get; set; }
```


Whether `Value` was produced.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L14)

<a id="Vercel_AI_Util_StreamRead_1_Reason_"></a>
<a id="Vercel_AI_Util_StreamRead_1_Reason"></a>

### Reason


```csharp
public object? Reason { readonly get; set; }
```


Failure reason. `null` is the JavaScript `undefined` error.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L23)

<a id="Vercel_AI_Util_StreamRead_1_Rejected_"></a>
<a id="Vercel_AI_Util_StreamRead_1_Rejected"></a>

### Rejected


```csharp
public bool Rejected { readonly get; set; }
```


The stream failed.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L20)

<a id="Vercel_AI_Util_StreamRead_1_Value_"></a>
<a id="Vercel_AI_Util_StreamRead_1_Value"></a>

### Value


```csharp
public T Value { readonly get; set; }
```


Chunk, when `HasValue` is true.

**Value:** `T`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L17)

## Methods

<a id="Vercel_AI_Util_StreamRead_1_Chunk_"></a>
<a id="Vercel_AI_Util_StreamRead_1_Chunk__0_"></a>

### Chunk(T)


```csharp
public static StreamRead<T> Chunk(T value)
```


A successful chunk.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | `T` |  |

**Returns:** [StreamRead&lt;T&gt;](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L26)

<a id="Vercel_AI_Util_StreamRead_1_Complete_"></a>
<a id="Vercel_AI_Util_StreamRead_1_Complete"></a>

### Complete()


```csharp
public static StreamRead<T> Complete()
```


A completed read.

**Returns:** [StreamRead&lt;T&gt;](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L32)

<a id="Vercel_AI_Util_StreamRead_1_Fail_"></a>
<a id="Vercel_AI_Util_StreamRead_1_Fail_System_Object_"></a>

### Fail(object?)


```csharp
public static StreamRead<T> Fail(object? reason)
```


A failed read.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `reason` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [StreamRead&lt;T&gt;](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L38)
