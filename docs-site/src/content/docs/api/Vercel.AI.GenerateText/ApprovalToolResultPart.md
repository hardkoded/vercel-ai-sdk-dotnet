---
title: "ApprovalToolResultPart"
description: "A tool result stored beside an approval response."
editUrl: false
prev: false
next: false
sidebar:
  label: "ApprovalToolResultPart"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ApprovalToolResultPart"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class ApprovalToolResultPart : ApprovalHistoryPart
```


A tool result stored beside an approval response.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ApprovalHistoryPart](/api/Vercel.AI.GenerateText/ApprovalHistoryPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L86)

## Constructors

<a id="Vercel_AI_GenerateText_ApprovalToolResultPart__ctor_"></a>
<a id="Vercel_AI_GenerateText_ApprovalToolResultPart__ctor_System_String_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__System_String_"></a>

### ApprovalToolResultPart(string, string, string, JsonElement?, string?)


```csharp
public ApprovalToolResultPart(string toolCallId, string toolName, string outputType, JsonElement? outputValue = null, string? reason = null)
```


Creates a tool result part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `outputType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `outputValue` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `reason` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L89)

## Properties

<a id="Vercel_AI_GenerateText_ApprovalToolResultPart_OutputType_"></a>
<a id="Vercel_AI_GenerateText_ApprovalToolResultPart_OutputType"></a>

### OutputType


```csharp
public string OutputType { get; }
```


Output type, such as `text` or `execution-denied`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L106)

<a id="Vercel_AI_GenerateText_ApprovalToolResultPart_OutputValue_"></a>
<a id="Vercel_AI_GenerateText_ApprovalToolResultPart_OutputValue"></a>

### OutputValue


```csharp
public JsonElement? OutputValue { get; }
```


Output value for text and JSON results.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L109)

<a id="Vercel_AI_GenerateText_ApprovalToolResultPart_Reason_"></a>
<a id="Vercel_AI_GenerateText_ApprovalToolResultPart_Reason"></a>

### Reason


```csharp
public string? Reason { get; }
```


Denial reason.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L112)

<a id="Vercel_AI_GenerateText_ApprovalToolResultPart_ToolCallId_"></a>
<a id="Vercel_AI_GenerateText_ApprovalToolResultPart_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L100)

<a id="Vercel_AI_GenerateText_ApprovalToolResultPart_ToolName_"></a>
<a id="Vercel_AI_GenerateText_ApprovalToolResultPart_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L103)
