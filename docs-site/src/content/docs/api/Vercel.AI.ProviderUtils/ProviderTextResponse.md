---
title: "ProviderTextResponse"
description: "A successful JSON response body and its headers."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProviderTextResponse"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ProviderTextResponse"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class ProviderTextResponse
```


A successful JSON response body and its headers.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderHttp.cs#L503)

## Constructors

<a id="Vercel_AI_ProviderUtils_ProviderTextResponse__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderTextResponse__ctor_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `ProviderTextResponse(string, IReadOnlyDictionary<string, string>)`


```csharp
public ProviderTextResponse(string body, IReadOnlyDictionary<string, string> headers)
```


Creates a text response.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderHttp.cs#L506)

## Properties

<a id="Vercel_AI_ProviderUtils_ProviderTextResponse_Body_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderTextResponse_Body"></a>

### Body


```csharp
public string Body { get; }
```


Response body.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderHttp.cs#L513)

<a id="Vercel_AI_ProviderUtils_ProviderTextResponse_Headers_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderTextResponse_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderHttp.cs#L516)
