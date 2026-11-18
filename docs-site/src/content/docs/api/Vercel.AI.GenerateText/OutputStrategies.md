---
title: "OutputStrategies"
description: "Array output schema wrapping. Maps to the array branch of getOutputStrategy."
editUrl: false
prev: false
next: false
sidebar:
  label: "OutputStrategies"
  hidden: true
---

<a id="Vercel_AI_GenerateText_OutputStrategies"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class OutputStrategies
```


Array output schema wrapping. Maps to the array branch of `getOutputStrategy`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ArrayOutputStrategy.cs#L11)

## Methods

<a id="Vercel_AI_GenerateText_OutputStrategies_WrapArrayOutputSchema_"></a>
<a id="Vercel_AI_GenerateText_OutputStrategies_WrapArrayOutputSchema_System_Text_Json_JsonElement_"></a>

### WrapArrayOutputSchema(JsonElement)


```csharp
public static JsonElement WrapArrayOutputSchema(JsonElement elementSchema)
```


Wraps an element schema in `{ elements: array }` and lifts root `definitions` or `$defs`
so root-relative references stay valid.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `elementSchema` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ArrayOutputStrategy.cs#L17)
