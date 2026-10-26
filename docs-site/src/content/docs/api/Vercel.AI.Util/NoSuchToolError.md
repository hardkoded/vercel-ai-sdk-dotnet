---
title: "NoSuchToolError"
description: "The model called a tool that is not available. Maps to NoSuchToolError."
editUrl: false
prev: false
next: false
sidebar:
  label: "NoSuchToolError"
  hidden: true
---

<a id="Vercel_AI_Util_NoSuchToolError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class NoSuchToolError : AiSdkError, ISerializable, IErrorMarkers
```


The model called a tool that is not available. Maps to `NoSuchToolError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/NoSuchToolError.cs#L8)

## Constructors

<a id="Vercel_AI_Util_NoSuchToolError__ctor_"></a>
<a id="Vercel_AI_Util_NoSuchToolError__ctor_System_String_System_Collections_Generic_IReadOnlyList_System_String__System_String_"></a>

### `NoSuchToolError(string, IReadOnlyList<string>?, string?)`


```csharp
public NoSuchToolError(string toolName, IReadOnlyList<string>? availableTools = null, string? message = null)
```


Creates the error. &lt;code class="paramref"&gt;message&lt;/code&gt; defaults to the upstream format.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `availableTools` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/NoSuchToolError.cs#L14)

## Fields

<a id="Vercel_AI_Util_NoSuchToolError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_NoSuchToolError"
```


Marker for [IsInstance(object)](#Vercel_AI_Util_NoSuchToolError_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/NoSuchToolError.cs#L11)

## Properties

<a id="Vercel_AI_Util_NoSuchToolError_AvailableTools_"></a>
<a id="Vercel_AI_Util_NoSuchToolError_AvailableTools"></a>

### AvailableTools


```csharp
public IReadOnlyList<string>? AvailableTools { get; }
```


Names of the available tools, or null when no tools were set.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/NoSuchToolError.cs#L29)

<a id="Vercel_AI_Util_NoSuchToolError_ToolName_"></a>
<a id="Vercel_AI_Util_NoSuchToolError_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Name of the tool that was called.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/NoSuchToolError.cs#L26)

## Methods

<a id="Vercel_AI_Util_NoSuchToolError_IsInstance_"></a>
<a id="Vercel_AI_Util_NoSuchToolError_IsInstance_System_Object_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/NoSuchToolError.cs#L32)
