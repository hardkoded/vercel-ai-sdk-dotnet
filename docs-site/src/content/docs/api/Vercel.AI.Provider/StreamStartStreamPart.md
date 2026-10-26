---
title: "StreamStartStreamPart"
description: "Marks the start of a stream and carries call warnings."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamStartStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_StreamStartStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class StreamStartStreamPart : LanguageModelStreamPart
```


Marks the start of a stream and carries call warnings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1994)

## Constructors

<a id="Vercel_AI_Provider_StreamStartStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_StreamStartStreamPart__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_CallWarning__"></a>

### `StreamStartStreamPart(IReadOnlyList<CallWarning>?)`


```csharp
public StreamStartStreamPart(IReadOnlyList<CallWarning>? warnings)
```


Creates a stream-start part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `warnings` | [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1997)

## Properties

<a id="Vercel_AI_Provider_StreamStartStreamPart_Warnings_"></a>
<a id="Vercel_AI_Provider_StreamStartStreamPart_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<CallWarning> Warnings { get; }
```


Warnings discovered while preparing the request.

**Value:** [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2004)
