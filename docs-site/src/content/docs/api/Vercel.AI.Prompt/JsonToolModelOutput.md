---
title: "JsonToolModelOutput"
description: "JSON tool output."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsonToolModelOutput"
  hidden: true
---

<a id="Vercel_AI_Prompt_JsonToolModelOutput"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class JsonToolModelOutput : ToolModelOutput
```


JSON tool output.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ToolModelOutput](/api/Vercel.AI.Prompt/ToolModelOutput/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L67)

## Constructors

<a id="Vercel_AI_Prompt_JsonToolModelOutput__ctor_"></a>
<a id="Vercel_AI_Prompt_JsonToolModelOutput__ctor_System_Text_Json_JsonElement_"></a>

### JsonToolModelOutput(JsonElement)


```csharp
public JsonToolModelOutput(JsonElement value)
```


Creates JSON output.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L70)

## Properties

<a id="Vercel_AI_Prompt_JsonToolModelOutput_Value_"></a>
<a id="Vercel_AI_Prompt_JsonToolModelOutput_Value"></a>

### Value


```csharp
public JsonElement Value { get; }
```


JSON value.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L77)
