---
title: "GoogleToolCallAssistantPart"
description: "An assistant function or server tool call."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleToolCallAssistantPart"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleToolCallAssistantPart"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleToolCallAssistantPart : GoogleAssistantPart
```


An assistant function or server tool call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GoogleAssistantPart](/api/Vercel.AI.Google/GoogleAssistantPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L183)

## Constructors

<a id="Vercel_AI_Google_GoogleToolCallAssistantPart__ctor_"></a>
<a id="Vercel_AI_Google_GoogleToolCallAssistantPart__ctor_System_String_System_String_"></a>

### GoogleToolCallAssistantPart(string, string)


```csharp
public GoogleToolCallAssistantPart(string toolName, string argumentsJson)
```


Creates a tool call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `argumentsJson` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L186)

## Properties

<a id="Vercel_AI_Google_GoogleToolCallAssistantPart_ArgumentsJson_"></a>
<a id="Vercel_AI_Google_GoogleToolCallAssistantPart_ArgumentsJson"></a>

### ArgumentsJson


```csharp
public string ArgumentsJson { get; }
```


JSON arguments.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L196)

<a id="Vercel_AI_Google_GoogleToolCallAssistantPart_ToolCallId_"></a>
<a id="Vercel_AI_Google_GoogleToolCallAssistantPart_ToolCallId"></a>

### ToolCallId


```csharp
public string? ToolCallId { get; set; }
```


Client tool-call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L199)

<a id="Vercel_AI_Google_GoogleToolCallAssistantPart_ToolName_"></a>
<a id="Vercel_AI_Google_GoogleToolCallAssistantPart_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L193)
