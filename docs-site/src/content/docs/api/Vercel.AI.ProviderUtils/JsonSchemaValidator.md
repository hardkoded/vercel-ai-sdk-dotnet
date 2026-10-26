---
title: "JsonSchemaValidator"
description: "Applies a JSON Schema document. Unknown object properties are dropped."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsonSchemaValidator"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_JsonSchemaValidator"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class JsonSchemaValidator
```


Applies a JSON Schema document. Unknown object properties are dropped.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L223)

## Methods

<a id="Vercel_AI_ProviderUtils_JsonSchemaValidator_TryApply_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemaValidator_TryApply_System_Text_Json_JsonElement_System_Text_Json_Nodes_JsonNode_System_Text_Json_JsonElement__System_String__"></a>

### TryApply(JsonElement, JsonNode, out JsonElement, out string?)


```csharp
public static bool TryApply(JsonElement value, JsonNode schema, out JsonElement projected, out string? error)
```


Projects &lt;code class="paramref"&gt;value&lt;/code&gt; through &lt;code class="paramref"&gt;schema&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `schema` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |
| `projected` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `error` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L226)
