---
title: "InvalidToolInputError"
description: "The input of a tool call does not match the tool's input schema. Maps to InvalidToolInputError."
editUrl: false
prev: false
next: false
sidebar:
  label: "InvalidToolInputError"
  hidden: true
---

<a id="Vercel_AI_Util_InvalidToolInputError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class InvalidToolInputError : AiSdkError, ISerializable, IErrorMarkers
```


The input of a tool call does not match the tool's input schema. Maps to `InvalidToolInputError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/InvalidToolInputError.cs#L8)

## Constructors

<a id="Vercel_AI_Util_InvalidToolInputError__ctor_"></a>
<a id="Vercel_AI_Util_InvalidToolInputError__ctor_System_String_System_String_System_Exception_System_String_"></a>

### InvalidToolInputError(string, string, Exception?, string?)


```csharp
public InvalidToolInputError(string toolName, string toolInput, Exception? cause = null, string? message = null)
```


Creates the error. &lt;code class="paramref"&gt;message&lt;/code&gt; defaults to the upstream format.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolInput` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/InvalidToolInputError.cs#L14)

## Fields

<a id="Vercel_AI_Util_InvalidToolInputError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_InvalidToolInputError"
```


Marker for [IsInstance(object)](#Vercel_AI_Util_InvalidToolInputError_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/InvalidToolInputError.cs#L11)

## Properties

<a id="Vercel_AI_Util_InvalidToolInputError_ToolInput_"></a>
<a id="Vercel_AI_Util_InvalidToolInputError_ToolInput"></a>

### ToolInput


```csharp
public string ToolInput { get; }
```


Raw input of the call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/InvalidToolInputError.cs#L26)

<a id="Vercel_AI_Util_InvalidToolInputError_ToolName_"></a>
<a id="Vercel_AI_Util_InvalidToolInputError_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Name of the tool that was called.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/InvalidToolInputError.cs#L23)

## Methods

<a id="Vercel_AI_Util_InvalidToolInputError_IsInstance_"></a>
<a id="Vercel_AI_Util_InvalidToolInputError_IsInstance_System_Object_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/InvalidToolInputError.cs#L29)
