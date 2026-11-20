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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L643)

## Constructors

<a id="Vercel_AI_ProviderUtils_JsonStreams_JsonBody__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_JsonStreams_JsonBody__ctor_System_Text_Json_JsonElement_System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_Byte____"></a>

### `JsonBody(JsonElement, JsonElement, IReadOnlyDictionary<string, string>, IReadOnlyDictionary<string, byte[]>?)`


```csharp
public JsonBody(JsonElement value, JsonElement rawValue, IReadOnlyDictionary<string, string> headers, IReadOnlyDictionary<string, byte[]>? binaries = null)
```


Creates a body.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `rawValue` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `binaries` | [IReadOnlyDictionary&lt;string, byte\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L646)

## Properties

<a id="Vercel_AI_ProviderUtils_JsonStreams_JsonBody_Binaries_"></a>
<a id="Vercel_AI_ProviderUtils_JsonStreams_JsonBody_Binaries"></a>

### Binaries


```csharp
public IReadOnlyDictionary<string, byte[]> Binaries { get; }
```


The bytes extracted at the paths of `experimentalBinaryFields`, by the placeholder string that stands in for them in
[Value](#Vercel_AI_ProviderUtils_JsonStreams_JsonBody_Value) and [RawValue](#Vercel_AI_ProviderUtils_JsonStreams_JsonBody_RawValue). Empty when no path was given or nothing was extracted.

**Value:** [IReadOnlyDictionary&lt;string, byte\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L672)

<a id="Vercel_AI_ProviderUtils_JsonStreams_JsonBody_Headers_"></a>
<a id="Vercel_AI_ProviderUtils_JsonStreams_JsonBody_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L666)

<a id="Vercel_AI_ProviderUtils_JsonStreams_JsonBody_RawValue_"></a>
<a id="Vercel_AI_ProviderUtils_JsonStreams_JsonBody_RawValue"></a>

### RawValue


```csharp
public JsonElement RawValue { get; }
```


Parsed JSON before projection.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L663)

<a id="Vercel_AI_ProviderUtils_JsonStreams_JsonBody_Value_"></a>
<a id="Vercel_AI_ProviderUtils_JsonStreams_JsonBody_Value"></a>

### Value


```csharp
public JsonElement Value { get; }
```


Value after the schema projection.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonSchemas.cs#L660)
