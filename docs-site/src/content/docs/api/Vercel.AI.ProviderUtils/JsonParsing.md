---
title: "JsonParsing"
description: "Parses JSON text, optionally checking it against a JSON Schema document."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsonParsing"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_JsonParsing"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class JsonParsing
```


Parses JSON text, optionally checking it against a JSON Schema document.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L149)

## Methods

<a id="Vercel_AI_ProviderUtils_JsonParsing_IsParsable_"></a>
<a id="Vercel_AI_ProviderUtils_JsonParsing_IsParsable_System_String_"></a>

### IsParsable(string)


```csharp
public static bool IsParsable(string text)
```


True when [Parse(string)](/api/Vercel.AI.ProviderUtils/SecureJson/#Vercel_AI_ProviderUtils_SecureJson_Parse_System_String_) accepts &lt;code class="paramref"&gt;text&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L152)

<a id="Vercel_AI_ProviderUtils_JsonParsing_Parse_"></a>
<a id="Vercel_AI_ProviderUtils_JsonParsing_Parse_System_String_System_Text_Json_Nodes_JsonNode_"></a>

### Parse(string, JsonNode?)


```csharp
public static JsonElement Parse(string text, JsonNode? schema = null)
```


Parses &lt;code class="paramref"&gt;text&lt;/code&gt;. Schema failures throw [TypeValidationException](/api/Vercel.AI.ProviderUtils/TypeValidationException/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `schema` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L166)

<a id="Vercel_AI_ProviderUtils_JsonParsing_SafeParse_"></a>
<a id="Vercel_AI_ProviderUtils_JsonParsing_SafeParse_System_String_System_Text_Json_Nodes_JsonNode_"></a>

### SafeParse(string, JsonNode?)


```csharp
public static JsonParseResult SafeParse(string text, JsonNode? schema = null)
```


Parses &lt;code class="paramref"&gt;text&lt;/code&gt; without throwing for invalid JSON or schema mismatches.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `schema` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |

**Returns:** [JsonParseResult](/api/Vercel.AI.ProviderUtils/JsonParseResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L182)

<a id="Vercel_AI_ProviderUtils_JsonParsing_SafeValidateTypes_"></a>
<a id="Vercel_AI_ProviderUtils_JsonParsing_SafeValidateTypes_System_Text_Json_JsonElement_System_Text_Json_Nodes_JsonNode_"></a>

### SafeValidateTypes(JsonElement, JsonNode)


```csharp
public static JsonParseResult SafeValidateTypes(JsonElement value, JsonNode schema)
```


Validates &lt;code class="paramref"&gt;value&lt;/code&gt; without throwing. Maps to `safeValidateTypes`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `schema` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |

**Returns:** [JsonParseResult](/api/Vercel.AI.ProviderUtils/JsonParseResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L211)

<a id="Vercel_AI_ProviderUtils_JsonParsing_ValidateTypes_"></a>
<a id="Vercel_AI_ProviderUtils_JsonParsing_ValidateTypes_System_Text_Json_JsonElement_System_Text_Json_Nodes_JsonNode_"></a>

### ValidateTypes(JsonElement, JsonNode)


```csharp
public static JsonElement ValidateTypes(JsonElement value, JsonNode schema)
```


Returns &lt;code class="paramref"&gt;value&lt;/code&gt; projected through &lt;code class="paramref"&gt;schema&lt;/code&gt;. Maps to `validateTypes`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `schema` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

**Exceptions**

| Type | Condition |
| --- | --- |
| [TypeValidationException](/api/Vercel.AI.ProviderUtils/TypeValidationException/) | The value does not match the schema. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L200)
