---
title: "StreamTextTransformOptions"
description: "The options a StreamTextTransform receives. Maps to the argument of StreamTextTransform."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamTextTransformOptions"
  hidden: true
---

<a id="Vercel_AI_StreamTextTransformOptions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class StreamTextTransformOptions
```


The options a [StreamTextTransform](/api/Vercel.AI/StreamTextTransform/) receives. Maps to the argument of `StreamTextTransform`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2101)

## Constructors

<a id="Vercel_AI_StreamTextTransformOptions__ctor_"></a>
<a id="Vercel_AI_StreamTextTransformOptions__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Tool__System_Action_"></a>

### `StreamTextTransformOptions(IReadOnlyList<Tool>?, Action?)`


```csharp
public StreamTextTransformOptions(IReadOnlyList<Tool>? tools = null, Action? stopStream = null)
```


Creates the options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tools` | [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `stopStream` | [Action](https://learn.microsoft.com/dotnet/api/system.action) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2104)

## Properties

<a id="Vercel_AI_StreamTextTransformOptions_StopStream_"></a>
<a id="Vercel_AI_StreamTextTransformOptions_StopStream"></a>

### StopStream


```csharp
public Action StopStream { get; }
```


Stops the source stream. No further part reaches the transform after it is called. Maps to `stopStream`.

**Value:** [Action](https://learn.microsoft.com/dotnet/api/system.action)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2114)

<a id="Vercel_AI_StreamTextTransformOptions_Tools_"></a>
<a id="Vercel_AI_StreamTextTransformOptions_Tools"></a>

### Tools


```csharp
public IReadOnlyList<Tool> Tools { get; }
```


The tools that are accessible to and can be called by the model. Maps to `tools`.

**Value:** [IReadOnlyList&lt;Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2111)
