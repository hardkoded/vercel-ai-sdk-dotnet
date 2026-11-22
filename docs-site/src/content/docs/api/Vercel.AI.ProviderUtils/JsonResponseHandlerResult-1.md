---
title: "JsonResponseHandlerResult<T>"
description: "A handled JSON value, the parsed body before validation, and the response headers."
editUrl: false
prev: false
next: false
sidebar:
  label: "JsonResponseHandlerResult<T>"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_JsonResponseHandlerResult_1"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class JsonResponseHandlerResult<T>
```


A handled JSON value, the parsed body before validation, and the response headers.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ResponseHandlers.cs#L31)

## Constructors

<a id="Vercel_AI_ProviderUtils_JsonResponseHandlerResult_1__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_JsonResponseHandlerResult_1__ctor__0_System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `JsonResponseHandlerResult(T, JsonElement, IReadOnlyDictionary<string, string>)`


```csharp
public JsonResponseHandlerResult(T value, JsonElement rawValue, IReadOnlyDictionary<string, string> responseHeaders)
```


Creates a result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | `T` |  |
| `rawValue` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `responseHeaders` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ResponseHandlers.cs#L34)

## Properties

<a id="Vercel_AI_ProviderUtils_JsonResponseHandlerResult_1_RawValue_"></a>
<a id="Vercel_AI_ProviderUtils_JsonResponseHandlerResult_1_RawValue"></a>

### RawValue


```csharp
public JsonElement RawValue { get; }
```


The parsed body. It holds a placeholder string in place of each decoded binary field.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ResponseHandlers.cs#L49)

<a id="Vercel_AI_ProviderUtils_JsonResponseHandlerResult_1_ResponseHeaders_"></a>
<a id="Vercel_AI_ProviderUtils_JsonResponseHandlerResult_1_ResponseHeaders"></a>

### ResponseHeaders


```csharp
public IReadOnlyDictionary<string, string> ResponseHeaders { get; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ResponseHandlers.cs#L52)

<a id="Vercel_AI_ProviderUtils_JsonResponseHandlerResult_1_Value_"></a>
<a id="Vercel_AI_ProviderUtils_JsonResponseHandlerResult_1_Value"></a>

### Value


```csharp
public T Value { get; }
```


The validated value. It holds the decoded [byte](https://learn.microsoft.com/dotnet/api/system.byte) arrays at the binary fields.

**Value:** `T`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ResponseHandlers.cs#L46)
