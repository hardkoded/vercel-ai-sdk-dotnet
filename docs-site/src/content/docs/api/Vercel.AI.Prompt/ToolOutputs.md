---
title: "ToolOutputs"
description: "Builds the model-facing tool result. Maps to createToolModelOutput."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolOutputs"
  hidden: true
---

<a id="Vercel_AI_Prompt_ToolOutputs"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public static class ToolOutputs
```


Builds the model-facing tool result. Maps to `createToolModelOutput`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L140)

## Methods

<a id="Vercel_AI_Prompt_ToolOutputs_CreateToolModelOutput_"></a>
<a id="Vercel_AI_Prompt_ToolOutputs_CreateToolModelOutput_System_String_System_Nullable_System_Text_Json_JsonElement__System_Object_System_Boolean_System_Func_Vercel_AI_Prompt_ToolModelOutputCall_Vercel_AI_Prompt_ToolModelOutput__System_String_"></a>

### `CreateToolModelOutput(string, JsonElement?, object?, bool, Func<ToolModelOutputCall, ToolModelOutput>?, string)`


```csharp
public static ToolModelOutput CreateToolModelOutput(string toolCallId, JsonElement? input, object? output, bool hasOutput, Func<ToolModelOutputCall, ToolModelOutput>? toModelOutput, string errorMode)
```


Converts a tool output. &lt;code class="paramref"&gt;errorMode&lt;/code&gt; is `text`, `json`, or `none`.
Set &lt;code class="paramref"&gt;hasOutput&lt;/code&gt; to false when the output is undefined.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `input` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `output` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `hasOutput` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `toModelOutput` | [Func&lt;ToolModelOutputCall, ToolModelOutput&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `errorMode` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ToolModelOutput](/api/Vercel.AI.Prompt/ToolModelOutput/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L151)
