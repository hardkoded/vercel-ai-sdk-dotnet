---
title: "LanguageModelStreamResponse"
description: "Response info for a stream call. Maps to the response field of doStream."
editUrl: false
prev: false
next: false
sidebar:
  label: "LanguageModelStreamResponse"
  hidden: true
---

<a id="Vercel_AI_Provider_LanguageModelStreamResponse"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class LanguageModelStreamResponse
```


Response info for a stream call. Maps to the `response` field of `doStream`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2186)

## Constructors

<a id="Vercel_AI_Provider_LanguageModelStreamResponse__ctor_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamResponse__ctor_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `LanguageModelStreamResponse(IReadOnlyDictionary<string, string>?)`


```csharp
public LanguageModelStreamResponse(IReadOnlyDictionary<string, string>? headers = null)
```


Creates response info.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2189)

## Properties

<a id="Vercel_AI_Provider_LanguageModelStreamResponse_Headers_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamResponse_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


HTTP response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2195)
