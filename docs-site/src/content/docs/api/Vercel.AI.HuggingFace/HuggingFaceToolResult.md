---
title: "HuggingFaceToolResult"
description: "A tool result returned in the response output."
editUrl: false
prev: false
next: false
sidebar:
  label: "HuggingFaceToolResult"
  hidden: true
---

<a id="Vercel_AI_HuggingFace_HuggingFaceToolResult"></a>

Namespace: [Vercel.AI.HuggingFace](/api/Vercel.AI.HuggingFace/)


```csharp
public sealed class HuggingFaceToolResult : GeneratedContent
```


A tool result returned in the response output.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceContent.cs#L47)

## Constructors

<a id="Vercel_AI_HuggingFace_HuggingFaceToolResult__ctor_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceToolResult__ctor_System_String_System_String_System_Text_Json_JsonElement_"></a>

### HuggingFaceToolResult(string, string, JsonElement)


```csharp
public HuggingFaceToolResult(string toolCallId, string toolName, JsonElement result)
```


Creates a tool result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `result` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceContent.cs#L50)

## Properties

<a id="Vercel_AI_HuggingFace_HuggingFaceToolResult_Result_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceToolResult_Result"></a>

### Result


```csharp
public JsonElement Result { get; }
```


Result value.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceContent.cs#L65)

<a id="Vercel_AI_HuggingFace_HuggingFaceToolResult_ToolCallId_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceToolResult_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Matching tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceContent.cs#L59)

<a id="Vercel_AI_HuggingFace_HuggingFaceToolResult_ToolName_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceToolResult_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceContent.cs#L62)
