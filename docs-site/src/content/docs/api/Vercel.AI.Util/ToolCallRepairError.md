---
title: "ToolCallRepairError"
description: "The repair of a tool call failed. Maps to ToolCallRepairError."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolCallRepairError"
  hidden: true
---

<a id="Vercel_AI_Util_ToolCallRepairError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class ToolCallRepairError : AiSdkError, ISerializable, IErrorMarkers
```


The repair of a tool call failed. Maps to `ToolCallRepairError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ToolCallRepairError.cs#L10)

## Constructors

<a id="Vercel_AI_Util_ToolCallRepairError__ctor_"></a>
<a id="Vercel_AI_Util_ToolCallRepairError__ctor_System_Exception_Vercel_AI_Util_AiSdkError_System_String_"></a>

### ToolCallRepairError(Exception, AiSdkError, string?)


```csharp
public ToolCallRepairError(Exception cause, AiSdkError originalError, string? message = null)
```


Creates the error. &lt;code class="paramref"&gt;cause&lt;/code&gt; is the failure of the repair function and
&lt;code class="paramref"&gt;originalError&lt;/code&gt; the [NoSuchToolError](/api/Vercel.AI.Util/NoSuchToolError/) or [InvalidToolInputError](/api/Vercel.AI.Util/InvalidToolInputError/) that started the repair.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `originalError` | [AiSdkError](/api/Vercel.AI.Util/AiSdkError/) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ToolCallRepairError.cs#L19)

## Fields

<a id="Vercel_AI_Util_ToolCallRepairError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_ToolCallRepairError"
```


Marker for [IsInstance(object)](#Vercel_AI_Util_ToolCallRepairError_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ToolCallRepairError.cs#L13)

## Properties

<a id="Vercel_AI_Util_ToolCallRepairError_OriginalError_"></a>
<a id="Vercel_AI_Util_ToolCallRepairError_OriginalError"></a>

### OriginalError


```csharp
public AiSdkError OriginalError { get; }
```


The error that started the repair.

**Value:** [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ToolCallRepairError.cs#L27)

## Methods

<a id="Vercel_AI_Util_ToolCallRepairError_IsInstance_"></a>
<a id="Vercel_AI_Util_ToolCallRepairError_IsInstance_System_Object_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ToolCallRepairError.cs#L30)
