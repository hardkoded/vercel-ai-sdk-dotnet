---
title: "MockStreamResult"
description: "Stream result stored in a scripted list."
editUrl: false
prev: false
next: false
sidebar:
  label: "MockStreamResult"
  hidden: true
---

<a id="Vercel_AI_Testing_MockStreamResult"></a>

Namespace: [Vercel.AI.Testing](/api/Vercel.AI.Testing/)


```csharp
public sealed class MockStreamResult
```


Stream result stored in a scripted list.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L66)

## Constructors

<a id="Vercel_AI_Testing_MockStreamResult__ctor_"></a>
<a id="Vercel_AI_Testing_MockStreamResult__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Testing_MockStreamChunk__"></a>

### `MockStreamResult(IReadOnlyList<MockStreamChunk>)`


```csharp
public MockStreamResult(IReadOnlyList<MockStreamChunk> chunks)
```


Creates a result with explicit chunks.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `chunks` | [IReadOnlyList&lt;MockStreamChunk&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L80)

<a id="Vercel_AI_Testing_MockStreamResult__ctor_System_String_"></a>

### MockStreamResult(string)


```csharp
public MockStreamResult(string text)
```


Creates a result that yields text-start, text-delta, and text-end for &lt;code class="paramref"&gt;text&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L69)

## Properties

<a id="Vercel_AI_Testing_MockStreamResult_Chunks_"></a>
<a id="Vercel_AI_Testing_MockStreamResult_Chunks"></a>

### Chunks


```csharp
public IReadOnlyList<MockStreamChunk> Chunks { get; }
```


Chunks in order.

**Value:** [IReadOnlyList&lt;MockStreamChunk&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Testing/MockModels.cs#L86)
