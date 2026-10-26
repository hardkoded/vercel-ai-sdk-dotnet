---
title: "CohereRerankResponse"
description: "Cohere rerank HTTP response."
editUrl: false
prev: false
next: false
sidebar:
  label: "CohereRerankResponse"
  hidden: true
---

<a id="Vercel_AI_Cohere_CohereRerankResponse"></a>

Namespace: [Vercel.AI.Cohere](/api/Vercel.AI.Cohere/)


```csharp
public sealed class CohereRerankResponse
```


Cohere rerank HTTP response.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L345)

## Constructors

<a id="Vercel_AI_Cohere_CohereRerankResponse__ctor_"></a>
<a id="Vercel_AI_Cohere_CohereRerankResponse__ctor_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Text_Json_JsonElement_"></a>

### `CohereRerankResponse(string?, IReadOnlyDictionary<string, string>, JsonElement)`


```csharp
public CohereRerankResponse(string? id, IReadOnlyDictionary<string, string> headers, JsonElement body)
```


Creates response metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `body` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L348)

## Properties

<a id="Vercel_AI_Cohere_CohereRerankResponse_Body_"></a>
<a id="Vercel_AI_Cohere_CohereRerankResponse_Body"></a>

### Body


```csharp
public JsonElement Body { get; }
```


Raw response body.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L362)

<a id="Vercel_AI_Cohere_CohereRerankResponse_Headers_"></a>
<a id="Vercel_AI_Cohere_CohereRerankResponse_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L359)

<a id="Vercel_AI_Cohere_CohereRerankResponse_Id_"></a>
<a id="Vercel_AI_Cohere_CohereRerankResponse_Id"></a>

### Id


```csharp
public string? Id { get; }
```


Response id, when the body includes one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L356)
