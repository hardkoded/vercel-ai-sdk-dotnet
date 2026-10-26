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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L27)

## Constructors

<a id="Vercel_AI_ProviderUtils_TypeValidationException__ctor_"></a>
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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L30)

## Properties

<a id="Vercel_AI_ProviderUtils_TypeValidationException_Value_"></a>
<a id="Vercel_AI_ProviderUtils_TypeValidationException_Value"></a>

### Value


```csharp
public JsonElement? Value { get; }
```


Value that failed validation.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L37)
