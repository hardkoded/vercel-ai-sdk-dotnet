---
title: "JsonSchemas"
description: "Builds JSON Schema documents from explicit shapes. This is not a Zod converter."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsonSchemas"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_JsonSchemas"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class JsonSchemas
```


Builds JSON Schema documents from explicit shapes. This is not a Zod converter.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L15)

## Methods

<a id="Vercel_AI_ProviderUtils_JsonSchemas_AddAdditionalProperties_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_AddAdditionalProperties_System_Text_Json_Nodes_JsonObject_"></a>

### AddAdditionalProperties(JsonObject)


```csharp
public static JsonObject AddAdditionalProperties(JsonObject schema)
```


Sets `additionalProperties` to false on object schemas, including objects nested in
combinators, items, and definitions. A schema-valued `additionalProperties` is kept and visited.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `schema` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L381)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_AnyOf_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_AnyOf_System_Text_Json_Nodes_JsonNode___"></a>

### AnyOf(params JsonNode[])


```csharp
public static JsonObject AnyOf(params JsonNode[] schemas)
```


`anyOf` schema.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `schemas` | [JsonNode\[\]](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L209)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_Anything_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_Anything"></a>

### Anything()


```csharp
public static JsonObject Anything()
```


Schema that accepts any value.

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L34)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_Array_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_Array_System_Text_Json_Nodes_JsonNode_System_Nullable_System_Int32__System_Nullable_System_Int32__System_Text_Json_Nodes_JsonNode_"></a>

### Array(JsonNode?, int?, int?, JsonNode?)


```csharp
public static JsonObject Array(JsonNode? items = null, int? minItems = null, int? maxItems = null, JsonNode? additionalItems = null)
```


Array schema. &lt;code class="paramref"&gt;items&lt;/code&gt; may be a schema or a tuple of schemas.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `items` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |
| `minItems` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `maxItems` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `additionalItems` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L117)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_Boolean_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_Boolean"></a>

### Boolean()


```csharp
public static JsonObject Boolean()
```


Boolean schema.

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L105)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_DateTimeString_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_DateTimeString"></a>

### DateTimeString()


```csharp
public static JsonObject DateTimeString()
```


Date-time string.

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L249)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_EmptyObject_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_EmptyObject"></a>

### EmptyObject()


```csharp
public static JsonObject EmptyObject()
```


Empty object schema used when a caller does not supply one.

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L23)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_EndsWithPattern_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_EndsWithPattern_System_String_"></a>

### EndsWithPattern(string)


```csharp
public static string EndsWithPattern(string literal)
```


Pattern that anchors &lt;code class="paramref"&gt;literal&lt;/code&gt; at the end.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `literal` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L286)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_Enum_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_Enum_System_Text_Json_Nodes_JsonNode_System_Text_Json_Nodes_JsonArray_"></a>

### Enum(JsonNode, JsonArray)


```csharp
public static JsonObject Enum(JsonNode type, JsonArray values)
```


Enum schema. &lt;code class="paramref"&gt;type&lt;/code&gt; may be a string or an array of type names.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |
| `values` | [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L187)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_EscapePattern_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_EscapePattern_System_String_"></a>

### EscapePattern(string)


```csharp
public static string EscapePattern(string literal)
```


Escapes a literal for a JSON Schema pattern.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `literal` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L263)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_IncludesPattern_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_IncludesPattern_System_String_"></a>

### IncludesPattern(string)


```csharp
public static string IncludesPattern(string literal)
```


Pattern that contains &lt;code class="paramref"&gt;literal&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `literal` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L292)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_Integer_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_Integer_System_Nullable_System_Double__System_Nullable_System_Double__System_String_"></a>

### Integer(double?, double?, string?)


```csharp
public static JsonObject Integer(double? minimum = null, double? maximum = null, string? format = null)
```


Integer schema.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `minimum` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `maximum` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `format` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L95)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_Literal_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_Literal_System_String_System_Text_Json_Nodes_JsonNode_System_String_"></a>

### Literal(string, JsonNode, string?)


```csharp
public static JsonObject Literal(string type, JsonNode constant, string? description = null)
```


A single literal.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `constant` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L197)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_NotEmpty_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_NotEmpty"></a>

### NotEmpty()


```csharp
public static JsonObject NotEmpty()
```


JSON Schema `not` of an empty schema, used for an explicit undefined alternative.

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L40)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_Null_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_Null"></a>

### Null()


```csharp
public static JsonObject Null()
```


Null schema.

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L111)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_Number_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_Number_System_Nullable_System_Double__System_Nullable_System_Double__System_Nullable_System_Double__System_Nullable_System_Double__System_Nullable_System_Double__"></a>

### Number(double?, double?, double?, double?, double?)


```csharp
public static JsonObject Number(double? minimum = null, double? maximum = null, double? exclusiveMinimum = null, double? exclusiveMaximum = null, double? multipleOf = null)
```


Number schema.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `minimum` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `maximum` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `exclusiveMinimum` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `exclusiveMaximum` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `multipleOf` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L78)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_Object_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_Object_System_Collections_Generic_IReadOnlyList_System_Collections_Generic_KeyValuePair_System_String_System_Text_Json_Nodes_JsonNode___System_Collections_Generic_IReadOnlyList_System_String__System_Text_Json_Nodes_JsonNode_System_Nullable_System_Boolean__System_Text_Json_Nodes_JsonNode_"></a>

### `Object(IReadOnlyList<KeyValuePair<string, JsonNode>>?, IReadOnlyList<string>?, JsonNode?, bool?, JsonNode?)`


```csharp
public static JsonObject Object(IReadOnlyList<KeyValuePair<string, JsonNode>>? properties = null, IReadOnlyList<string>? required = null, JsonNode? additionalProperties = null, bool? additionalPropertiesFlag = null, JsonNode? defaultValue = null)
```


Object schema. Pass &lt;code class="paramref"&gt;additionalProperties&lt;/code&gt; for a schema, or
&lt;code class="paramref"&gt;additionalPropertiesFlag&lt;/code&gt; for true or false. Omit both to leave the keyword unset.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `properties` | [IReadOnlyList&lt;KeyValuePair&lt;string, JsonNode&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `required` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `additionalProperties` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |
| `additionalPropertiesFlag` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `defaultValue` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L139)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_PrimitiveUnion_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_PrimitiveUnion_System_String___"></a>

### PrimitiveUnion(params string[])


```csharp
public static JsonObject PrimitiveUnion(params string[] types)
```


Union of primitive type names.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `types` | [string\[\]](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L221)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_Record_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_Record_System_Text_Json_Nodes_JsonNode_System_Text_Json_Nodes_JsonNode_"></a>

### Record(JsonNode, JsonNode?)


```csharp
public static JsonObject Record(JsonNode additionalProperties, JsonNode? propertyNames = null)
```


Object whose values match &lt;code class="paramref"&gt;additionalProperties&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `additionalProperties` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |
| `propertyNames` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L233)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_StartsWithPattern_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_StartsWithPattern_System_String_"></a>

### StartsWithPattern(string)


```csharp
public static string StartsWithPattern(string literal)
```


Pattern that anchors &lt;code class="paramref"&gt;literal&lt;/code&gt; at the start.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `literal` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L280)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_String_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_String_System_Nullable_System_Int32__System_Nullable_System_Int32__System_String_System_String_System_String_System_Text_Json_Nodes_JsonArray_System_Text_Json_Nodes_JsonArray_"></a>

### String(int?, int?, string?, string?, string?, JsonArray?, JsonArray?)


```csharp
public static JsonObject String(int? minLength = null, int? maxLength = null, string? format = null, string? pattern = null, string? contentEncoding = null, JsonArray? allOf = null, JsonArray? anyOf = null)
```


String schema.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `minLength` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `maxLength` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `format` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `pattern` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `contentEncoding` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `allOf` | [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray) |  |
| `anyOf` | [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L49)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_Stringify_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_Stringify_System_Text_Json_Nodes_JsonNode_"></a>

### Stringify(JsonNode?)


```csharp
public static string Stringify(JsonNode? node)
```


Serializes a schema the way `JSON.stringify` does: no extra whitespace, insertion order.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `node` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L298)

<a id="Vercel_AI_ProviderUtils_JsonSchemas_WithDefault_"></a>
<a id="Vercel_AI_ProviderUtils_JsonSchemas_WithDefault_System_Text_Json_Nodes_JsonObject_System_Text_Json_Nodes_JsonNode_"></a>

### WithDefault(JsonObject, JsonNode)


```csharp
public static JsonObject WithDefault(JsonObject schema, JsonNode defaultValue)
```


Copies &lt;code class="paramref"&gt;schema&lt;/code&gt; and sets `default`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `schema` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `defaultValue` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L255)
