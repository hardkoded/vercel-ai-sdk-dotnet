---
title: "OperationJson"
description: "JSON helpers used while normalizing provider options."
editUrl: false
prev: false
next: false
sidebar:
  label: "OperationJson"
  hidden: true
---

<a id="Vercel_AI_Operations_OperationJson"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class OperationJson
```


JSON helpers used while normalizing provider options.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L743)

## Methods

<a id="Vercel_AI_Operations_OperationJson_Get_"></a>
<a id="Vercel_AI_Operations_OperationJson_Get_System_Nullable_System_Text_Json_JsonElement__System_String_"></a>

### Get(JsonElement?, string)


```csharp
public static JsonElement? Get(JsonElement? element, string name)
```


Reads a property or returns null.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `element` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L761)

<a id="Vercel_AI_Operations_OperationJson_Parse_"></a>
<a id="Vercel_AI_Operations_OperationJson_Parse_System_String_"></a>

### Parse(string)


```csharp
public static JsonElement Parse(string json)
```


Parses a JSON string.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `json` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L746)

<a id="Vercel_AI_Operations_OperationJson_Serialize_"></a>
<a id="Vercel_AI_Operations_OperationJson_Serialize__1___0_"></a>

### `Serialize<T>(T)`


```csharp
public static JsonElement Serialize<T>(T value)
```


Serializes a value.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | `T` |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L755)

<a id="Vercel_AI_Operations_OperationJson_Utf8Length_"></a>
<a id="Vercel_AI_Operations_OperationJson_Utf8Length_System_String_"></a>

### Utf8Length(string)


```csharp
public static int Utf8Length(string value)
```


UTF-8 size of a string.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L772)
