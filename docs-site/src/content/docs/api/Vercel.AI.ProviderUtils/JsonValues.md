---
title: "JsonValues"
description: "Small JSON helpers shared by providers."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsonValues"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_JsonValues"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class JsonValues
```


Small JSON helpers shared by providers.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonValues.cs#L10)

## Properties

<a id="Vercel_AI_ProviderUtils_JsonValues_Options_"></a>
<a id="Vercel_AI_ProviderUtils_JsonValues_Options"></a>

### Options


```csharp
public static JsonSerializerOptions Options { get; }
```


Serializer options used for provider bodies.

**Value:** [JsonSerializerOptions](https://learn.microsoft.com/dotnet/api/system.text.json.jsonserializeroptions)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonValues.cs#L13)

## Methods

<a id="Vercel_AI_ProviderUtils_JsonValues_EmptyObject_"></a>
<a id="Vercel_AI_ProviderUtils_JsonValues_EmptyObject"></a>

### EmptyObject()


```csharp
public static JsonElement EmptyObject()
```


An empty JSON object.

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonValues.cs#L32)

<a id="Vercel_AI_ProviderUtils_JsonValues_ExtractErrorMessage_"></a>
<a id="Vercel_AI_ProviderUtils_JsonValues_ExtractErrorMessage_System_String_System_Int32_"></a>

### ExtractErrorMessage(string?, int)


```csharp
public static string ExtractErrorMessage(string? body, int statusCode)
```


Best-effort error message from a provider JSON body.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonValues.cs#L66)

<a id="Vercel_AI_ProviderUtils_JsonValues_GenerateId_"></a>
<a id="Vercel_AI_ProviderUtils_JsonValues_GenerateId_System_String_"></a>

### GenerateId(string)


```csharp
public static string GenerateId(string prefix)
```


Creates a new id. Maps to `generateId`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prefix` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonValues.cs#L144)

<a id="Vercel_AI_ProviderUtils_JsonValues_GetInt_"></a>
<a id="Vercel_AI_ProviderUtils_JsonValues_GetInt_System_Text_Json_JsonElement_System_String_"></a>

### GetInt(JsonElement, string)


```csharp
public static int? GetInt(JsonElement element, string name)
```


Reads an integer property, or null.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `element` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonValues.cs#L50)

<a id="Vercel_AI_ProviderUtils_JsonValues_GetString_"></a>
<a id="Vercel_AI_ProviderUtils_JsonValues_GetString_System_Text_Json_JsonElement_System_String_"></a>

### GetString(JsonElement, string)


```csharp
public static string? GetString(JsonElement element, string name)
```


Reads a string property, or null.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `element` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonValues.cs#L39)

<a id="Vercel_AI_ProviderUtils_JsonValues_ParseOrEmpty_"></a>
<a id="Vercel_AI_ProviderUtils_JsonValues_ParseOrEmpty_System_String_"></a>

### ParseOrEmpty(string?)


```csharp
public static JsonElement ParseOrEmpty(string? json)
```


Parses a JSON object or returns an empty object.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `json` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonValues.cs#L20)
