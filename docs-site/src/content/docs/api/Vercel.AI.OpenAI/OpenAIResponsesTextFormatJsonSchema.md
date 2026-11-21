---
title: "OpenAIResponsesTextFormatJsonSchema"
description: "A JSON Schema format (the jsonschema type)."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIResponsesTextFormatJsonSchema"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIResponsesTextFormatJsonSchema"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIResponsesTextFormatJsonSchema : OpenAIResponsesTextFormat
```


A JSON Schema format (the `json_schema` type).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAIResponsesTextFormat](/api/Vercel.AI.OpenAI/OpenAIResponsesTextFormat/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesTextFormat.cs#L94)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIResponsesTextFormatJsonSchema_Description_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesTextFormatJsonSchema_Description"></a>

### Description


```csharp
public string? Description { get; set; }
```


The format description.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesTextFormat.cs#L103)

<a id="Vercel_AI_OpenAI_OpenAIResponsesTextFormatJsonSchema_Name_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesTextFormatJsonSchema_Name"></a>

### Name


```csharp
public string Name { get; set; }
```


The format name: 1 to 64 characters of letters, digits, `_` or `-`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesTextFormat.cs#L97)

<a id="Vercel_AI_OpenAI_OpenAIResponsesTextFormatJsonSchema_Schema_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesTextFormatJsonSchema_Schema"></a>

### Schema


```csharp
public JsonElement Schema { get; set; }
```


The JSON Schema, an object.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesTextFormat.cs#L100)

<a id="Vercel_AI_OpenAI_OpenAIResponsesTextFormatJsonSchema_Strict_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesTextFormatJsonSchema_Strict"></a>

### Strict


```csharp
public bool? Strict { get; set; }
```


Strict mode.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesTextFormat.cs#L106)
