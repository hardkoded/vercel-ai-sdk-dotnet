---
title: "RerankResponseInfo"
description: "Response metadata for a rerank call."
editUrl: false
prev: false
next: false
sidebar:
  label: "RerankResponseInfo"
  hidden: true
---

<a id="Vercel_AI_Operations_RerankResponseInfo"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class RerankResponseInfo
```


Response metadata for a rerank call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L136)

## Constructors

<a id="Vercel_AI_Operations_RerankResponseInfo__ctor_"></a>
<a id="Vercel_AI_Operations_RerankResponseInfo__ctor_System_String_System_DateTime_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Nullable_System_Text_Json_JsonElement__"></a>

### `RerankResponseInfo(string?, DateTime, string, IReadOnlyDictionary<string, string>?, JsonElement?)`


```csharp
public RerankResponseInfo(string? id, DateTime timestamp, string modelId, IReadOnlyDictionary<string, string>? headers, JsonElement? body)
```


Creates response metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `timestamp` | [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `body` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L139)

## Properties

<a id="Vercel_AI_Operations_RerankResponseInfo_Body_"></a>
<a id="Vercel_AI_Operations_RerankResponseInfo_Body"></a>

### Body


```csharp
public JsonElement? Body { get; }
```


Response body.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L161)

<a id="Vercel_AI_Operations_RerankResponseInfo_Headers_"></a>
<a id="Vercel_AI_Operations_RerankResponseInfo_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L158)

<a id="Vercel_AI_Operations_RerankResponseInfo_Id_"></a>
<a id="Vercel_AI_Operations_RerankResponseInfo_Id"></a>

### Id


```csharp
public string? Id { get; }
```


Provider response id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L149)

<a id="Vercel_AI_Operations_RerankResponseInfo_ModelId_"></a>
<a id="Vercel_AI_Operations_RerankResponseInfo_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L155)

<a id="Vercel_AI_Operations_RerankResponseInfo_Timestamp_"></a>
<a id="Vercel_AI_Operations_RerankResponseInfo_Timestamp"></a>

### Timestamp


```csharp
public DateTime Timestamp { get; }
```


Response timestamp.

**Value:** [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L152)
