---
title: "ProviderResponse"
description: "Headers and body returned by one provider call."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProviderResponse"
  hidden: true
---

<a id="Vercel_AI_Operations_ProviderResponse"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ProviderResponse
```


Headers and body returned by one provider call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L366)

## Constructors

<a id="Vercel_AI_Operations_ProviderResponse__ctor_"></a>
<a id="Vercel_AI_Operations_ProviderResponse__ctor_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Nullable_System_Text_Json_JsonElement__System_String_System_Nullable_System_DateTime__System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### `ProviderResponse(IReadOnlyDictionary<string, string>?, JsonElement?, string?, DateTime?, string?, JsonElement?)`


```csharp
public ProviderResponse(IReadOnlyDictionary<string, string>? headers = null, JsonElement? body = null, string? id = null, DateTime? timestamp = null, string? modelId = null, JsonElement? providerMetadata = null)
```


Creates response metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `body` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `timestamp` | [DateTime?](https://learn.microsoft.com/dotnet/api/system.datetime) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L369)

## Properties

<a id="Vercel_AI_Operations_ProviderResponse_Body_"></a>
<a id="Vercel_AI_Operations_ProviderResponse_Body"></a>

### Body


```csharp
public JsonElement? Body { get; }
```


Response body.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L383)

<a id="Vercel_AI_Operations_ProviderResponse_Headers_"></a>
<a id="Vercel_AI_Operations_ProviderResponse_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L380)

<a id="Vercel_AI_Operations_ProviderResponse_Id_"></a>
<a id="Vercel_AI_Operations_ProviderResponse_Id"></a>

### Id


```csharp
public string? Id { get; }
```


Provider response id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L386)

<a id="Vercel_AI_Operations_ProviderResponse_ModelId_"></a>
<a id="Vercel_AI_Operations_ProviderResponse_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; }
```


Model id reported by the provider.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L392)

<a id="Vercel_AI_Operations_ProviderResponse_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_ProviderResponse_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata stored on the response, when the call includes it there.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L395)

<a id="Vercel_AI_Operations_ProviderResponse_Timestamp_"></a>
<a id="Vercel_AI_Operations_ProviderResponse_Timestamp"></a>

### Timestamp


```csharp
public DateTime? Timestamp { get; }
```


Provider timestamp.

**Value:** [DateTime?](https://learn.microsoft.com/dotnet/api/system.datetime)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L389)
