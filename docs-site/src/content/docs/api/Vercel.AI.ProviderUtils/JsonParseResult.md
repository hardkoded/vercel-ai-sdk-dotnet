---
title: "JsonParseResult"
description: "Success or failure of a JSON parse, including the raw value when parsing succeeded."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsonParseResult"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_JsonParseResult"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class JsonParseResult
```


Success or failure of a JSON parse, including the raw value when parsing succeeded.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L41)

## Properties

<a id="Vercel_AI_ProviderUtils_JsonParseResult_Error_"></a>
<a id="Vercel_AI_ProviderUtils_JsonParseResult_Error"></a>

### Error


```csharp
public Exception? Error { get; }
```


Parse or validation error.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L61)

<a id="Vercel_AI_ProviderUtils_JsonParseResult_RawValue_"></a>
<a id="Vercel_AI_ProviderUtils_JsonParseResult_RawValue"></a>

### RawValue


```csharp
public JsonElement? RawValue { get; }
```


Value before schema projection. Null when the text was not JSON.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L58)

<a id="Vercel_AI_ProviderUtils_JsonParseResult_Success_"></a>
<a id="Vercel_AI_ProviderUtils_JsonParseResult_Success"></a>

### Success


```csharp
public bool Success { get; }
```


True when parsing and schema checks succeeded.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L52)

<a id="Vercel_AI_ProviderUtils_JsonParseResult_Value_"></a>
<a id="Vercel_AI_ProviderUtils_JsonParseResult_Value"></a>

### Value


```csharp
public JsonElement? Value { get; }
```


Projected value.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L55)

## Methods

<a id="Vercel_AI_ProviderUtils_JsonParseResult_Fail_"></a>
<a id="Vercel_AI_ProviderUtils_JsonParseResult_Fail_System_Exception_System_Nullable_System_Text_Json_JsonElement__"></a>

### Fail(Exception, JsonElement?)


```csharp
public static JsonParseResult Fail(Exception error, JsonElement? rawValue)
```


Creates a failure result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `rawValue` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [JsonParseResult](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L70)

<a id="Vercel_AI_ProviderUtils_JsonParseResult_Ok_"></a>
<a id="Vercel_AI_ProviderUtils_JsonParseResult_Ok_System_Text_Json_JsonElement_System_Text_Json_JsonElement_"></a>

### Ok(JsonElement, JsonElement)


```csharp
public static JsonParseResult Ok(JsonElement value, JsonElement rawValue)
```


Creates a success result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `rawValue` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [JsonParseResult](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L64)
