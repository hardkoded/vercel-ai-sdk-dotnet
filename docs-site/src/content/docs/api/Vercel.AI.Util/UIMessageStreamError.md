---
title: "UIMessageStreamError"
description: "Error thrown when a UI message stream reports an error or contains invalid or out-of-sequence chunks. Maps to UIMessageStreamError."
editUrl: false
prev: false
next: false
sidebar:
  label: "UIMessageStreamError"
  hidden: true
---

<a id="Vercel_AI_Util_UIMessageStreamError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class UIMessageStreamError : AiSdkError, ISerializable, IErrorMarkers
```


Error thrown when a UI message stream reports an error or contains invalid or out-of-sequence chunks.
Maps to `UIMessageStreamError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/UIMessageStreamError.cs#L11)

## Constructors

<a id="Vercel_AI_Util_UIMessageStreamError__ctor_"></a>
<a id="Vercel_AI_Util_UIMessageStreamError__ctor_System_String_System_String_System_String_"></a>

### UIMessageStreamError(string, string, string)


```csharp
public UIMessageStreamError(string chunkType, string chunkId, string message)
```


Creates the error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `chunkType` | [string](https://learn.microsoft.com/dotnet/api/system.string) | The type of chunk that caused the error, for example `text-delta`. |
| `chunkId` | [string](https://learn.microsoft.com/dotnet/api/system.string) | The id of the failing chunk (part id or tool call id), or an empty string. |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) | The error message. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/UIMessageStreamError.cs#L20)

## Fields

<a id="Vercel_AI_Util_UIMessageStreamError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_UIMessageStreamError"
```


Marker for [IsInstance(object)](#Vercel_AI_Util_UIMessageStreamError_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/UIMessageStreamError.cs#L14)

## Properties

<a id="Vercel_AI_Util_UIMessageStreamError_ChunkId_"></a>
<a id="Vercel_AI_Util_UIMessageStreamError_ChunkId"></a>

### ChunkId


```csharp
public string ChunkId { get; }
```


The id of the failing chunk, or an empty string when the chunk has no id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/UIMessageStreamError.cs#L32)

<a id="Vercel_AI_Util_UIMessageStreamError_ChunkType_"></a>
<a id="Vercel_AI_Util_UIMessageStreamError_ChunkType"></a>

### ChunkType


```csharp
public string ChunkType { get; }
```


The type of chunk that caused the error.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/UIMessageStreamError.cs#L29)

## Methods

<a id="Vercel_AI_Util_UIMessageStreamError_IsInstance_"></a>
<a id="Vercel_AI_Util_UIMessageStreamError_IsInstance_System_Object_"></a>

### IsInstance(object?)


```csharp
public static bool IsInstance(object? error)
```


Returns whether &lt;code class="paramref"&gt;error&lt;/code&gt; carries this error's marker.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/UIMessageStreamError.cs#L35)
