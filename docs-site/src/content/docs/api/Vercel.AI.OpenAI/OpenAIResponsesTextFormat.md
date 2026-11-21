---
title: "OpenAIResponsesTextFormat"
description: "Text format accepted by textFormat and textFormatUpdate: OpenAIResponsesTextFormatText or OpenAIResponsesTextFormatJsonSchema. Maps to OpenAIResponsesTextFor..."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIResponsesTextFormat"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIResponsesTextFormat"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public abstract class OpenAIResponsesTextFormat
```


Text format accepted by `textFormat` and `textFormatUpdate`: [OpenAIResponsesTextFormatText](/api/Vercel.AI.OpenAI/OpenAIResponsesTextFormatText/) or
[OpenAIResponsesTextFormatJsonSchema](/api/Vercel.AI.OpenAI/OpenAIResponsesTextFormatJsonSchema/). Maps to `OpenAIResponsesTextFormat`
(`openai-responses-text-format.ts`).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [OpenAIResponsesTextFormatJsonSchema](/api/Vercel.AI.OpenAI/OpenAIResponsesTextFormatJsonSchema/), [OpenAIResponsesTextFormatText](/api/Vercel.AI.OpenAI/OpenAIResponsesTextFormatText/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesTextFormat.cs#L17)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIResponsesTextFormat_ToJsonElement_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesTextFormat_ToJsonElement"></a>

### ToJsonElement()


```csharp
public JsonElement ToJsonElement()
```


Returns the format as the JSON value of `textFormat` or `textFormatUpdate`.

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesTextFormat.cs#L32)
