---
title: "JsonInstructions"
description: "JSON output instructions. Maps to injectJsonInstruction."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsonInstructions"
  hidden: true
---

<a id="Vercel_AI_GenerateText_JsonInstructions"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class JsonInstructions
```


JSON output instructions. Maps to `injectJsonInstruction`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/InjectJsonInstruction.cs#L38)

## Methods

<a id="Vercel_AI_GenerateText_JsonInstructions_InjectJsonInstruction_"></a>
<a id="Vercel_AI_GenerateText_JsonInstructions_InjectJsonInstruction_System_String_System_Nullable_System_Text_Json_JsonElement__Vercel_AI_GenerateText_JsonInstructionText_Vercel_AI_GenerateText_JsonInstructionText_"></a>

### InjectJsonInstruction(string?, JsonElement?, JsonInstructionText, JsonInstructionText)


```csharp
public static string InjectJsonInstruction(string? prompt = null, JsonElement? schema = null, JsonInstructionText schemaPrefix = default, JsonInstructionText schemaSuffix = default)
```


Builds the instruction text appended to a prompt.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `schema` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `schemaPrefix` | [JsonInstructionText](/api/Vercel.AI.GenerateText/JsonInstructionText/) |  |
| `schemaSuffix` | [JsonInstructionText](/api/Vercel.AI.GenerateText/JsonInstructionText/) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/InjectJsonInstruction.cs#L45)
