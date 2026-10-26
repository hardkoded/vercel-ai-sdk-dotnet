---
title: "JsonStreams.JsonBody"
description: "A parsed JSON body and the headers that came with it."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsonStreams.JsonBody"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_JsonStreams_JsonBody"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class JsonStreams.JsonBody
```


A parsed JSON body and the headers that came with it.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L602)

## Constructors

<a id="Vercel_AI_ProviderUtils_JsonStreams_JsonBody__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_JsonStreams_JsonBody__ctor_System_Text_Json_JsonElement_System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `JsonBody(JsonElement, JsonElement, IReadOnlyDictionary<string, string>)`


```csharp
public JsonBody(JsonElement value, JsonElement rawValue, IReadOnlyDictionary<string, string> headers)
```


Creates a body.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `rawValue` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L605)

## Properties

<a id="Vercel_AI_ProviderUtils_JsonStreams_JsonBody_Headers_"></a>
<a id="Vercel_AI_ProviderUtils_JsonStreams_JsonBody_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L619)

<a id="Vercel_AI_ProviderUtils_JsonStreams_JsonBody_RawValue_"></a>
<a id="Vercel_AI_ProviderUtils_JsonStreams_JsonBody_RawValue"></a>

### RawValue


```csharp
public JsonElement RawValue { get; }
```


Parsed JSON before projection.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L616)

<a id="Vercel_AI_ProviderUtils_JsonStreams_JsonBody_Value_"></a>
<a id="Vercel_AI_ProviderUtils_JsonStreams_JsonBody_Value"></a>

### Value


```csharp
public JsonElement Value { get; }
```


Value after the schema projection.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L613)
