---
title: "TypeValidationException"
description: "A value did not match a JSON Schema."
editUrl: false
prev: false
next: false
sidebar:
  label: "TypeValidationException"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_TypeValidationException"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class TypeValidationException : Exception, ISerializable
```


A value did not match a JSON Schema.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L30)

## Constructors

<a id="Vercel_AI_ProviderUtils_TypeValidationException__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_TypeValidationException__ctor_System_Nullable_System_Text_Json_JsonElement__System_Exception_Vercel_AI_ProviderUtils_TypeValidationContext_"></a>

### TypeValidationException(JsonElement?, Exception, TypeValidationContext?)


```csharp
public TypeValidationException(JsonElement? value, Exception cause, TypeValidationContext? context)
```


Creates the exception with the location of the failed value. Maps to the `TypeValidationError` constructor.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `context` | [TypeValidationContext](/api/Vercel.AI.ProviderUtils/TypeValidationContext/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L40)

<a id="Vercel_AI_ProviderUtils_TypeValidationException__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### TypeValidationException(string, JsonElement?)


```csharp
public TypeValidationException(string message, JsonElement? value)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `value` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L33)

## Properties

<a id="Vercel_AI_ProviderUtils_TypeValidationException_Context_"></a>
<a id="Vercel_AI_ProviderUtils_TypeValidationException_Context"></a>

### Context


```csharp
public TypeValidationContext? Context { get; }
```


Where the value was found. Maps to `context`.

**Value:** [TypeValidationContext](/api/Vercel.AI.ProviderUtils/TypeValidationContext/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L55)

<a id="Vercel_AI_ProviderUtils_TypeValidationException_Value_"></a>
<a id="Vercel_AI_ProviderUtils_TypeValidationException_Value"></a>

### Value


```csharp
public JsonElement? Value { get; }
```


Value that failed validation.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L52)
