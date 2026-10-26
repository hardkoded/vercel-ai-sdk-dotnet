---
title: "OpenAIChatPromptMessage"
description: "One prompt message converted into an OpenAI chat message."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIChatPromptMessage"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIChatPromptMessage"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIChatPromptMessage
```


One prompt message converted into an OpenAI chat message.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L14)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIChatPromptMessage__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptMessage__ctor_System_String_"></a>

### OpenAIChatPromptMessage(string)


```csharp
public OpenAIChatPromptMessage(string role)
```


Creates a message.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `role` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L17)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIChatPromptMessage_Parts_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptMessage_Parts"></a>

### Parts


```csharp
public IReadOnlyList<OpenAIChatPromptPart>? Parts { get; set; }
```


User or assistant parts.

**Value:** [IReadOnlyList&lt;OpenAIChatPromptPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L29)

<a id="Vercel_AI_OpenAI_OpenAIChatPromptMessage_ProviderOptions_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptMessage_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Provider options for this message. The `openai` object is read.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L47)

<a id="Vercel_AI_OpenAI_OpenAIChatPromptMessage_Role_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptMessage_Role"></a>

### Role


```csharp
public string Role { get; }
```


`system`, `user`, `assistant`, or `tool`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L23)

<a id="Vercel_AI_OpenAI_OpenAIChatPromptMessage_Text_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptMessage_Text"></a>

### Text


```csharp
public string? Text { get; set; }
```


System text, or assistant text when there are no parts.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L26)

<a id="Vercel_AI_OpenAI_OpenAIChatPromptMessage_ToolCallId_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptMessage_ToolCallId"></a>

### ToolCallId


```csharp
public string? ToolCallId { get; set; }
```


Tool result id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L35)

<a id="Vercel_AI_OpenAI_OpenAIChatPromptMessage_ToolCalls_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptMessage_ToolCalls"></a>

### ToolCalls


```csharp
public IReadOnlyList<GeneratedToolCall>? ToolCalls { get; set; }
```


Assistant tool calls.

**Value:** [IReadOnlyList&lt;GeneratedToolCall&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L32)

<a id="Vercel_AI_OpenAI_OpenAIChatPromptMessage_ToolOutput_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptMessage_ToolOutput"></a>

### ToolOutput


```csharp
public string? ToolOutput { get; set; }
```


Tool result text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L38)

<a id="Vercel_AI_OpenAI_OpenAIChatPromptMessage_ToolOutputType_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptMessage_ToolOutputType"></a>

### ToolOutputType


```csharp
public string? ToolOutputType { get; set; }
```


Tool result type: `text`, `json`, `error-text`, `error-json`, or `execution-denied`.
Error types are sent as `{"error": value}`. When it is null, [ToolOutput](#Vercel_AI_OpenAI_OpenAIChatPromptMessage_ToolOutput) is sent as is.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L44)
