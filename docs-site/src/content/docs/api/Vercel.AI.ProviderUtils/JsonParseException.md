---
title: "JsonParseException"
description: "JSON text could not be parsed."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsonParseException"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_JsonParseException"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class JsonParseException : Exception, ISerializable
```


JSON text could not be parsed.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L13)

## Constructors

<a id="Vercel_AI_ProviderUtils_JsonParseException__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_JsonParseException__ctor_System_String_System_Exception_"></a>

### JsonParseException(string, Exception)


```csharp
public JsonParseException(string text, Exception innerException)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `innerException` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L16)

## Properties

<a id="Vercel_AI_ProviderUtils_JsonParseException_Text_"></a>
<a id="Vercel_AI_ProviderUtils_JsonParseException_Text"></a>

### Text


```csharp
public string Text { get; }
```


Original text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L23)
