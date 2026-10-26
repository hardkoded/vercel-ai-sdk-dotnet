---
title: "IteratorResult<T>"
description: "Result of one async-iterator step."
editUrl: false
prev: false
next: false
sidebar:
  label: "IteratorResult<T>"
  hidden: true
---

<a id="Vercel_AI_Util_IteratorResult_1"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public struct IteratorResult<T>
```


Result of one async-iterator step.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L61)

## Properties

<a id="Vercel_AI_Util_IteratorResult_1_Done_"></a>
<a id="Vercel_AI_Util_IteratorResult_1_Done"></a>

### Done


```csharp
public bool Done { readonly get; set; }
```


Whether iteration is finished.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L64)

<a id="Vercel_AI_Util_IteratorResult_1_HasValue_"></a>
<a id="Vercel_AI_Util_IteratorResult_1_HasValue"></a>

### HasValue


```csharp
public bool HasValue { readonly get; set; }
```


Whether `Value` is present.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L67)

<a id="Vercel_AI_Util_IteratorResult_1_Value_"></a>
<a id="Vercel_AI_Util_IteratorResult_1_Value"></a>

### Value


```csharp
public T Value { readonly get; set; }
```


Yielded value.

**Value:** `T`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L70)

## Methods

<a id="Vercel_AI_Util_IteratorResult_1_Complete_"></a>
<a id="Vercel_AI_Util_IteratorResult_1_Complete"></a>

### Complete()


```csharp
public static IteratorResult<T> Complete()
```


A finished step.

**Returns:** [IteratorResult&lt;T&gt;](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L73)

<a id="Vercel_AI_Util_IteratorResult_1_Of_"></a>
<a id="Vercel_AI_Util_IteratorResult_1_Of__0_"></a>

### Of(T)


```csharp
public static IteratorResult<T> Of(T value)
```


A value step.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | `T` |  |

**Returns:** [IteratorResult&lt;T&gt;](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L79)
