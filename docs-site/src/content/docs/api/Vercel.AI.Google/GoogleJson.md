---
title: "GoogleJson"
description: "JSON settings shared by Gemini request builders."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleJson"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleJson"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleJson
```


JSON settings shared by Gemini request builders.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJson.cs#L13)

## Properties

<a id="Vercel_AI_Google_GoogleJson_Metadata_"></a>
<a id="Vercel_AI_Google_GoogleJson_Metadata"></a>

### Metadata


```csharp
public static JsonSerializerOptions Metadata { get; }
```


Serializes metadata and keeps explicit nulls.

**Value:** [JsonSerializerOptions](https://learn.microsoft.com/dotnet/api/system.text.json.jsonserializeroptions)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJson.cs#L25)

<a id="Vercel_AI_Google_GoogleJson_Options_"></a>
<a id="Vercel_AI_Google_GoogleJson_Options"></a>

### Options


```csharp
public static JsonSerializerOptions Options { get; }
```


Serializes like `JSON.stringify`: no null properties, and non-ASCII characters stay unescaped.

**Value:** [JsonSerializerOptions](https://learn.microsoft.com/dotnet/api/system.text.json.jsonserializeroptions)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJson.cs#L18)

## Methods

<a id="Vercel_AI_Google_GoogleJson_Clone_"></a>
<a id="Vercel_AI_Google_GoogleJson_Clone_System_Nullable_System_Text_Json_JsonElement__"></a>

### Clone(JsonElement?)


```csharp
public static JsonNode? Clone(JsonElement? element)
```


Clones a JSON element into a node.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `element` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJson.cs#L93)

<a id="Vercel_AI_Google_GoogleJson_ParseObject_"></a>
<a id="Vercel_AI_Google_GoogleJson_ParseObject_System_String_"></a>

### ParseObject(string?)


```csharp
public static JsonObject ParseObject(string? json)
```


Parses a JSON object, or returns an empty object.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `json` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJson.cs#L37)

<a id="Vercel_AI_Google_GoogleJson_Set_"></a>
<a id="Vercel_AI_Google_GoogleJson_Set_System_Text_Json_Nodes_JsonObject_System_String_System_Nullable_System_Boolean__"></a>

### Set(JsonObject, string, bool?)


```csharp
public static void Set(JsonObject target, string name, bool? value)
```


Assigns a boolean when it is present.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `target` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `value` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJson.cs#L84)

<a id="Vercel_AI_Google_GoogleJson_Set_System_Text_Json_Nodes_JsonObject_System_String_System_Nullable_System_Double__"></a>

### Set(JsonObject, string, double?)


```csharp
public static void Set(JsonObject target, string name, double? value)
```


Assigns a number when it is present.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `target` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `value` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJson.cs#L66)

<a id="Vercel_AI_Google_GoogleJson_Set_System_Text_Json_Nodes_JsonObject_System_String_System_Nullable_System_Int32__"></a>

### Set(JsonObject, string, int?)


```csharp
public static void Set(JsonObject target, string name, int? value)
```


Assigns an integer when it is present.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `target` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `value` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJson.cs#L75)

<a id="Vercel_AI_Google_GoogleJson_Set_System_Text_Json_Nodes_JsonObject_System_String_System_String_"></a>

### Set(JsonObject, string, string?)


```csharp
public static void Set(JsonObject target, string name, string? value)
```


Assigns a string when it is non-null.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `target` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `value` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJson.cs#L57)

<a id="Vercel_AI_Google_GoogleJson_Set_System_Text_Json_Nodes_JsonObject_System_String_System_Text_Json_Nodes_JsonNode_"></a>

### Set(JsonObject, string, JsonNode?)


```csharp
public static void Set(JsonObject target, string name, JsonNode? value)
```


Assigns a property when the value is present.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `target` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `value` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJson.cs#L48)

<a id="Vercel_AI_Google_GoogleJson_String_"></a>
<a id="Vercel_AI_Google_GoogleJson_String_System_Text_Json_JsonElement_System_String_"></a>

### String(JsonElement, string)


```csharp
public static string? String(JsonElement element, string name)
```


Reads a string property.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `element` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJson.cs#L104)

<a id="Vercel_AI_Google_GoogleJson_TryObject_"></a>
<a id="Vercel_AI_Google_GoogleJson_TryObject_System_Text_Json_JsonElement_System_String_System_Text_Json_JsonElement__"></a>

### TryObject(JsonElement, string, out JsonElement)


```csharp
public static bool TryObject(JsonElement element, string name, out JsonElement value)
```


Reads an object property.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `element` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJson.cs#L115)

<a id="Vercel_AI_Google_GoogleJson_Write_"></a>
<a id="Vercel_AI_Google_GoogleJson_Write_System_Text_Json_Nodes_JsonNode_"></a>

### Write(JsonNode)


```csharp
public static string Write(JsonNode node)
```


Writes a node with [Options](#Vercel_AI_Google_GoogleJson_Options).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `node` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleJson.cs#L31)
