---
title: "ToolModelOutputCall"
description: "Arguments passed to toModelOutput."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolModelOutputCall"
  hidden: true
---

<a id="Vercel_AI_Prompt_ToolModelOutputCall"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class ToolModelOutputCall
```


Arguments passed to `toModelOutput`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L19)

## Constructors

<a id="Vercel_AI_Prompt_ToolModelOutputCall__ctor_"></a>
<a id="Vercel_AI_Prompt_ToolModelOutputCall__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__System_Object_"></a>

### ToolModelOutputCall(string, JsonElement?, object?)


```csharp
public ToolModelOutputCall(string toolCallId, JsonElement? input, object? output)
```


Creates the call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `input` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `output` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L22)

## Properties

<a id="Vercel_AI_Prompt_ToolModelOutputCall_Input_"></a>
<a id="Vercel_AI_Prompt_ToolModelOutputCall_Input"></a>

### Input


```csharp
public JsonElement? Input { get; }
```


Tool input.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L33)

<a id="Vercel_AI_Prompt_ToolModelOutputCall_Output_"></a>
<a id="Vercel_AI_Prompt_ToolModelOutputCall_Output"></a>

### Output


```csharp
public object? Output { get; }
```


Tool output.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L36)

<a id="Vercel_AI_Prompt_ToolModelOutputCall_ToolCallId_"></a>
<a id="Vercel_AI_Prompt_ToolModelOutputCall_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L30)
