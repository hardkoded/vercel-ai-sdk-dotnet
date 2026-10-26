---
title: "ToolApprovalRequest"
description: "A tool call the caller may approve before it runs."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolApprovalRequest"
  hidden: true
---

<a id="Vercel_AI_ToolApprovalRequest"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ToolApprovalRequest
```


A tool call the caller may approve before it runs.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L150)

## Constructors

<a id="Vercel_AI_ToolApprovalRequest__ctor_"></a>
<a id="Vercel_AI_ToolApprovalRequest__ctor_System_String_System_String_System_String_"></a>

### ToolApprovalRequest(string, string, string)


```csharp
public ToolApprovalRequest(string toolCallId, string toolName, string argumentsJson)
```


Creates an approval request.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `argumentsJson` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L153)

## Properties

<a id="Vercel_AI_ToolApprovalRequest_ArgumentsJson_"></a>
<a id="Vercel_AI_ToolApprovalRequest_ArgumentsJson"></a>

### ArgumentsJson


```csharp
public string ArgumentsJson { get; }
```


JSON arguments.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L167)

<a id="Vercel_AI_ToolApprovalRequest_ToolCallId_"></a>
<a id="Vercel_AI_ToolApprovalRequest_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L161)

<a id="Vercel_AI_ToolApprovalRequest_ToolName_"></a>
<a id="Vercel_AI_ToolApprovalRequest_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L164)
