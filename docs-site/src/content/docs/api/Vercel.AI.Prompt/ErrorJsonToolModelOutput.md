---
title: "ErrorJsonToolModelOutput"
description: "Error JSON tool output."
editUrl: false
prev: false
next: false
sidebar:
  label: "ErrorJsonToolModelOutput"
  hidden: true
---

<a id="Vercel_AI_Prompt_ErrorJsonToolModelOutput"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class ErrorJsonToolModelOutput : ToolModelOutput
```


Error JSON tool output.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ToolModelOutput](/api/Vercel.AI.Prompt/ToolModelOutput/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L95)

## Constructors

<a id="Vercel_AI_Prompt_ErrorJsonToolModelOutput__ctor_"></a>
<a id="Vercel_AI_Prompt_ErrorJsonToolModelOutput__ctor_System_Text_Json_JsonElement_"></a>

### ErrorJsonToolModelOutput(JsonElement)


```csharp
public ErrorJsonToolModelOutput(JsonElement value)
```


Creates error JSON.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L98)

## Properties

<a id="Vercel_AI_Prompt_ErrorJsonToolModelOutput_Value_"></a>
<a id="Vercel_AI_Prompt_ErrorJsonToolModelOutput_Value"></a>

### Value


```csharp
public JsonElement Value { get; }
```


Error value.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L105)
