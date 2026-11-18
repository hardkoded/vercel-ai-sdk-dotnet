---
title: "MissingToolResultsError"
description: "A tool call has no tool result before the next user or system message. Maps to MissingToolResultsError."
editUrl: false
prev: false
next: false
sidebar:
  label: "MissingToolResultsError"
  hidden: true
---

<a id="Vercel_AI_Util_MissingToolResultsError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class MissingToolResultsError : AiSdkError, ISerializable, IErrorMarkers
```


A tool call has no tool result before the next user or system message. Maps to `MissingToolResultsError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/MissingToolResultsError.cs#L8)

## Constructors

<a id="Vercel_AI_Util_MissingToolResultsError__ctor_"></a>
<a id="Vercel_AI_Util_MissingToolResultsError__ctor_System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `MissingToolResultsError(IReadOnlyList<string>)`


```csharp
public MissingToolResultsError(IReadOnlyList<string> toolCallIds)
```


Creates the error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallIds` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/MissingToolResultsError.cs#L14)

## Fields

<a id="Vercel_AI_Util_MissingToolResultsError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_MissingToolResultsError"
```


Marker for [IsInstance(object)](#Vercel_AI_Util_MissingToolResultsError_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/MissingToolResultsError.cs#L11)

## Properties

<a id="Vercel_AI_Util_MissingToolResultsError_ToolCallIds_"></a>
<a id="Vercel_AI_Util_MissingToolResultsError_ToolCallIds"></a>

### ToolCallIds


```csharp
public IReadOnlyList<string> ToolCallIds { get; }
```


Ids of the tool calls without a result.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/MissingToolResultsError.cs#L31)

## Methods

<a id="Vercel_AI_Util_MissingToolResultsError_IsInstance_"></a>
<a id="Vercel_AI_Util_MissingToolResultsError_IsInstance_System_Object_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/MissingToolResultsError.cs#L34)
