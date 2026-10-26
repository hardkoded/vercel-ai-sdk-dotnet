---
title: "EmbeddingResponseInfo"
description: "Raw HTTP response of an embedding call. Maps to response of doEmbed."
editUrl: false
prev: false
next: false
sidebar:
  label: "EmbeddingResponseInfo"
  hidden: true
---

<a id="Vercel_AI_Provider_EmbeddingResponseInfo"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class EmbeddingResponseInfo
```


Raw HTTP response of an embedding call. Maps to `response` of `doEmbed`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L12)

## Constructors

<a id="Vercel_AI_Provider_EmbeddingResponseInfo__ctor_"></a>
<a id="Vercel_AI_Provider_EmbeddingResponseInfo__ctor_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_String_"></a>

### `EmbeddingResponseInfo(IReadOnlyDictionary<string, string>?, string?)`


```csharp
public EmbeddingResponseInfo(IReadOnlyDictionary<string, string>? headers, string? body)
```


Creates response info.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L15)

## Properties

<a id="Vercel_AI_Provider_EmbeddingResponseInfo_Body_"></a>
<a id="Vercel_AI_Provider_EmbeddingResponseInfo_Body"></a>

### Body


```csharp
public string? Body { get; }
```


Raw HTTP response body.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L25)

<a id="Vercel_AI_Provider_EmbeddingResponseInfo_Headers_"></a>
<a id="Vercel_AI_Provider_EmbeddingResponseInfo_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


HTTP response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L22)
