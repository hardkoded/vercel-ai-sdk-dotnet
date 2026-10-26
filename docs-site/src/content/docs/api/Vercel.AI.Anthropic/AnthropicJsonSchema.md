---
title: "AnthropicJsonSchema"
description: "Relaxes a JSON Schema so it can be sent as Anthropic outputconfig.format.schema. The caller's original schema is not modified."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicJsonSchema"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicJsonSchema"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public static class AnthropicJsonSchema
```


Relaxes a JSON Schema so it can be sent as Anthropic `output_config.format.schema`.
The caller's original schema is not modified.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicJsonSchema.cs#L16)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicJsonSchema_Sanitize_"></a>
<a id="Vercel_AI_Anthropic_AnthropicJsonSchema_Sanitize_System_Text_Json_JsonElement_"></a>

### Sanitize(JsonElement)


```csharp
public static JsonNode Sanitize(JsonElement schema)
```


Returns a schema Anthropic's constrained decoder accepts.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `schema` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicJsonSchema.cs#L31)

<a id="Vercel_AI_Anthropic_AnthropicJsonSchema_Sanitize_System_Text_Json_Nodes_JsonNode_"></a>

### Sanitize(JsonNode)


```csharp
public static JsonNode Sanitize(JsonNode schema)
```


Returns a schema Anthropic's constrained decoder accepts.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `schema` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |

**Returns:** [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicJsonSchema.cs#L37)
