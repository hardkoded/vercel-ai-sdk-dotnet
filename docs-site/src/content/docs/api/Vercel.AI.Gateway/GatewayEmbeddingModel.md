---
title: "GatewayEmbeddingModel"
description: "Gateway embedding model."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayEmbeddingModel"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayEmbeddingModel"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayEmbeddingModel : IEmbeddingModel
```


Gateway embedding model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L332)

## Constructors

<a id="Vercel_AI_Gateway_GatewayEmbeddingModel__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayEmbeddingModel__ctor_Vercel_AI_Gateway_GatewayProvider_System_String_"></a>

### GatewayEmbeddingModel(GatewayProvider, string)


```csharp
public GatewayEmbeddingModel(GatewayProvider provider, string modelId)
```


Creates a Gateway embedding model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GatewayProvider](/api/Vercel.AI.Gateway/GatewayProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L337)

## Properties

<a id="Vercel_AI_Gateway_GatewayEmbeddingModel_ModelId_"></a>
<a id="Vercel_AI_Gateway_GatewayEmbeddingModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L350)

<a id="Vercel_AI_Gateway_GatewayEmbeddingModel_Provider_"></a>
<a id="Vercel_AI_Gateway_GatewayEmbeddingModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L347)

<a id="Vercel_AI_Gateway_GatewayEmbeddingModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Gateway_GatewayEmbeddingModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L344)

## Methods

<a id="Vercel_AI_Gateway_GatewayEmbeddingModel_DoEmbedAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayEmbeddingModel_DoEmbedAsync_System_Collections_Generic_IReadOnlyList_System_String__System_Nullable_System_Text_Json_JsonElement__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_System_Nullable_System_Int32__"></a>

### `DoEmbedAsync(IReadOnlyList<string>, JsonElement?, IReadOnlyDictionary<string, string?>?, CancellationToken, int?)`


```csharp
public Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, JsonElement? providerOptions, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken, int? dimensions = null)
```


Embeds values. Call headers are merged onto the Gateway V4 headers.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `values` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerOptions` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `dimensions` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

**Returns:** [Task&lt;EmbeddingResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L371)

<a id="Vercel_AI_Gateway_GatewayEmbeddingModel_DoEmbedAsync_System_Collections_Generic_IReadOnlyList_System_String__System_Nullable_System_Text_Json_JsonElement__System_Threading_CancellationToken_"></a>

### `DoEmbedAsync(IReadOnlyList<string>, JsonElement?, CancellationToken)`


```csharp
public Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, JsonElement? providerOptions, CancellationToken cancellationToken)
```


Embeds values and sends &lt;code class="paramref"&gt;providerOptions&lt;/code&gt; when it is a JSON object.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `values` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerOptions` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;EmbeddingResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L365)

<a id="Vercel_AI_Gateway_GatewayEmbeddingModel_DoEmbedAsync_System_Collections_Generic_IReadOnlyList_System_String__System_Threading_CancellationToken_"></a>

### `DoEmbedAsync(IReadOnlyList<string>, CancellationToken)`


```csharp
public Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, CancellationToken cancellationToken)
```


Embeds values without provider options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `values` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;EmbeddingResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L353)
