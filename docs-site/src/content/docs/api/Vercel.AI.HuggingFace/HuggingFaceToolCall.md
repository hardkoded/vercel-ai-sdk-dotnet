---
title: "HuggingFaceToolCall"
description: "A tool call that Hugging Face ran itself, such as an MCP call."
editUrl: false
prev: false
next: false
sidebar:
  label: "HuggingFaceToolCall"
  hidden: true
---

<a id="Vercel_AI_HuggingFace_HuggingFaceToolCall"></a>

Namespace: [Vercel.AI.HuggingFace](/api/Vercel.AI.HuggingFace/)


```csharp
public sealed class HuggingFaceToolCall : GeneratedToolCall
```


A tool call that Hugging Face ran itself, such as an MCP call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/) → [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceContent.cs#L35)

## Constructors

<a id="Vercel_AI_HuggingFace_HuggingFaceToolCall__ctor_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceToolCall__ctor_System_String_System_String_System_String_System_Boolean_"></a>

### HuggingFaceToolCall(string, string, string, bool)


```csharp
public HuggingFaceToolCall(string toolCallId, string toolName, string argumentsJson, bool providerExecuted)
```


Creates a tool call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `argumentsJson` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerExecuted` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceContent.cs#L38)
