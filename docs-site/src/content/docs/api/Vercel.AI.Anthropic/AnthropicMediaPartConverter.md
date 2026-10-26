---
title: "AnthropicMediaPartConverter"
description: "Converts one file part for a provider that speaks the Messages protocol. Receives the part in the SDK message JSON shape. Returns the image or video block wi..."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicMediaPartConverter"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicMediaPartConverter"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public delegate JsonObject? AnthropicMediaPartConverter(JsonElement part)
```


Converts one file part for a provider that speaks the Messages protocol. Receives the part in the SDK message JSON shape.
Returns the image or video block without `cache_control`, or &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt; to use the default conversion.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `part` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelSettings.cs#L14)
