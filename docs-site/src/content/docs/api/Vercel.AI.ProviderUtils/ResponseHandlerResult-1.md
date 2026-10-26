---
title: "ResponseHandlerResult<T>"
description: "A handled response value with the response headers."
editUrl: false
prev: false
next: false
sidebar:
  label: "ResponseHandlerResult<T>"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ResponseHandlerResult_1"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class ResponseHandlerResult<T>
```


A handled response value with the response headers.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ResponseHandlers.cs#L14)

## Constructors

<a id="Vercel_AI_ProviderUtils_ResponseHandlerResult_1__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_ResponseHandlerResult_1__ctor__0_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `ResponseHandlerResult(T, IReadOnlyDictionary<string, string>)`


```csharp
public ResponseHandlerResult(T value, IReadOnlyDictionary<string, string> responseHeaders)
```


Creates a result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | `T` |  |
| `responseHeaders` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ResponseHandlers.cs#L17)

## Properties

<a id="Vercel_AI_ProviderUtils_ResponseHandlerResult_1_ResponseHeaders_"></a>
<a id="Vercel_AI_ProviderUtils_ResponseHandlerResult_1_ResponseHeaders"></a>

### ResponseHeaders


```csharp
public IReadOnlyDictionary<string, string> ResponseHeaders { get; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ResponseHandlers.cs#L27)

<a id="Vercel_AI_ProviderUtils_ResponseHandlerResult_1_Value_"></a>
<a id="Vercel_AI_ProviderUtils_ResponseHandlerResult_1_Value"></a>

### Value


```csharp
public T Value { get; }
```


Handled value.

**Value:** `T`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ResponseHandlers.cs#L24)
