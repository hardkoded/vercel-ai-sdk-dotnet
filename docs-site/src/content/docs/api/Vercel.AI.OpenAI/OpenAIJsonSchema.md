---
title: "OpenAIJsonSchema"
description: "Removes JSON Schema keywords OpenAI does not enforce: string propertyNames and regex lookaround in pattern."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIJsonSchema"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIJsonSchema"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public static class OpenAIJsonSchema
```


Removes JSON Schema keywords OpenAI does not enforce: string `propertyNames`
and regex lookaround in `pattern`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIJsonSchema.cs#L30)

## Fields

<a id="Vercel_AI_OpenAI_OpenAIJsonSchema_LookaroundDetails"></a>

### LookaroundDetails


```csharp
public const string LookaroundDetails = "OpenAI does not support regex lookaround in JSON Schema patterns. The pattern was removed before sending the schema, so OpenAI will not enforce that constraint."
```


The warning OpenAI emits when a lookaround pattern is removed.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIJsonSchema.cs#L37)

<a id="Vercel_AI_OpenAI_OpenAIJsonSchema_PropertyNamesDetails"></a>

### PropertyNamesDetails


```csharp
public const string PropertyNamesDetails = "OpenAI does not support JSON Schema propertyNames. It was removed before sending the schema, so OpenAI will not enforce property-name constraints."
```


The warning OpenAI emits when `propertyNames` is removed.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIJsonSchema.cs#L33)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIJsonSchema_ContainsRegexLookaround_"></a>
<a id="Vercel_AI_OpenAI_OpenAIJsonSchema_ContainsRegexLookaround_System_String_"></a>

### ContainsRegexLookaround(string)


```csharp
public static bool ContainsRegexLookaround(string pattern)
```


True when &lt;code class="paramref"&gt;pattern&lt;/code&gt; contains an unescaped regex lookaround.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `pattern` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIJsonSchema.cs#L116)

<a id="Vercel_AI_OpenAI_OpenAIJsonSchema_Normalize_"></a>
<a id="Vercel_AI_OpenAI_OpenAIJsonSchema_Normalize_System_Text_Json_JsonElement_"></a>

### Normalize(JsonElement)


```csharp
public static OpenAIJsonSchemaResult Normalize(JsonElement schema)
```


Returns a copy of &lt;code class="paramref"&gt;schema&lt;/code&gt; safe to send to OpenAI.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `schema` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [OpenAIJsonSchemaResult](/api/Vercel.AI.OpenAI/OpenAIJsonSchemaResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIJsonSchema.cs#L41)

<a id="Vercel_AI_OpenAI_OpenAIJsonSchema_Normalize_System_Text_Json_Nodes_JsonNode_"></a>

### Normalize(JsonNode)


```csharp
public static OpenAIJsonSchemaResult Normalize(JsonNode schema)
```


Returns a copy of &lt;code class="paramref"&gt;schema&lt;/code&gt; safe to send to OpenAI.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `schema` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |

**Returns:** [OpenAIJsonSchemaResult](/api/Vercel.AI.OpenAI/OpenAIJsonSchemaResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIJsonSchema.cs#L48)
