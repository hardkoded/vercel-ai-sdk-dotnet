---
title: "GoogleJsonSchema"
description: "Rewrites JSON Schema so Gemini responseJsonSchema accepts it."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleJsonSchema"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleJsonSchema"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleJsonSchema
```


Rewrites JSON Schema so Gemini `responseJsonSchema` accepts it.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonSchema.cs#L11)

## Methods

<a id="Vercel_AI_Google_GoogleJsonSchema_Sanitize_"></a>
<a id="Vercel_AI_Google_GoogleJsonSchema_Sanitize_System_Text_Json_JsonElement_"></a>

### Sanitize(JsonElement)


```csharp
public static JsonNode Sanitize(JsonElement schema)
```


Sanitizes a schema element.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `schema` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonSchema.cs#L48)

<a id="Vercel_AI_Google_GoogleJsonSchema_Sanitize_System_Text_Json_Nodes_JsonNode_"></a>

### Sanitize(JsonNode)


```csharp
public static JsonNode Sanitize(JsonNode schema)
```


Replaces each `const` with a one-value `enum` and leaves every other keyword in place.
The input document is not modified.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `schema` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |

**Returns:** [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJsonSchema.cs#L17)
