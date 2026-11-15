---
title: "InvalidToolApprovalSignatureError"
description: "The signature of a tool approval request is missing or does not match. Maps to InvalidToolApprovalSignatureError."
editUrl: false
prev: false
next: false
sidebar:
  label: "InvalidToolApprovalSignatureError"
  hidden: true
---

<a id="Vercel_AI_Util_InvalidToolApprovalSignatureError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class InvalidToolApprovalSignatureError : AiSdkError, ISerializable, IErrorMarkers
```


The signature of a tool approval request is missing or does not match. Maps to `InvalidToolApprovalSignatureError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/InvalidToolApprovalSignatureError.cs#L8)

## Constructors

<a id="Vercel_AI_Util_InvalidToolApprovalSignatureError__ctor_"></a>
<a id="Vercel_AI_Util_InvalidToolApprovalSignatureError__ctor_System_String_System_String_System_String_"></a>

### InvalidToolApprovalSignatureError(string, string, string)


```csharp
public InvalidToolApprovalSignatureError(string approvalId, string toolCallId, string reason)
```


Creates the error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `approvalId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `reason` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/InvalidToolApprovalSignatureError.cs#L14)

## Fields

<a id="Vercel_AI_Util_InvalidToolApprovalSignatureError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_InvalidToolApprovalSignatureError"
```


Marker for [IsInstance(object)](#Vercel_AI_Util_InvalidToolApprovalSignatureError_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/InvalidToolApprovalSignatureError.cs#L11)

## Properties

<a id="Vercel_AI_Util_InvalidToolApprovalSignatureError_ApprovalId_"></a>
<a id="Vercel_AI_Util_InvalidToolApprovalSignatureError_ApprovalId"></a>

### ApprovalId


```csharp
public string ApprovalId { get; }
```


Id of the approval whose signature failed.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/InvalidToolApprovalSignatureError.cs#L25)

<a id="Vercel_AI_Util_InvalidToolApprovalSignatureError_ToolCallId_"></a>
<a id="Vercel_AI_Util_InvalidToolApprovalSignatureError_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Id of the tool call of the approval.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/InvalidToolApprovalSignatureError.cs#L28)

## Methods

<a id="Vercel_AI_Util_InvalidToolApprovalSignatureError_IsInstance_"></a>
<a id="Vercel_AI_Util_InvalidToolApprovalSignatureError_IsInstance_System_Object_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/InvalidToolApprovalSignatureError.cs#L31)
