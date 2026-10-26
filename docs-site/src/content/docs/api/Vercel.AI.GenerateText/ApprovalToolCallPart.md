---
title: "ApprovalToolCallPart"
description: "A tool call stored in assistant history."
editUrl: false
prev: false
next: false
sidebar:
  label: "ApprovalToolCallPart"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ApprovalToolCallPart"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class ApprovalToolCallPart : ApprovalHistoryPart
```


A tool call stored in assistant history.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ApprovalHistoryPart](/api/Vercel.AI.GenerateText/ApprovalHistoryPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L24)

## Constructors

<a id="Vercel_AI_GenerateText_ApprovalToolCallPart__ctor_"></a>
<a id="Vercel_AI_GenerateText_ApprovalToolCallPart__ctor_System_String_System_String_System_Text_Json_JsonElement_"></a>

### ApprovalToolCallPart(string, string, JsonElement)


```csharp
public ApprovalToolCallPart(string toolCallId, string toolName, JsonElement input)
```


Creates a tool call part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `input` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L27)

## Properties

<a id="Vercel_AI_GenerateText_ApprovalToolCallPart_Input_"></a>
<a id="Vercel_AI_GenerateText_ApprovalToolCallPart_Input"></a>

### Input


```csharp
public JsonElement Input { get; }
```


Tool input.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L42)

<a id="Vercel_AI_GenerateText_ApprovalToolCallPart_ToolCallId_"></a>
<a id="Vercel_AI_GenerateText_ApprovalToolCallPart_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L36)

<a id="Vercel_AI_GenerateText_ApprovalToolCallPart_ToolName_"></a>
<a id="Vercel_AI_GenerateText_ApprovalToolCallPart_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L39)
