---
title: "TencentModelCatalog"
description: "Known Tencent model ids and the per-model policy. Upstream tencent-model-catalog.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "TencentModelCatalog"
  hidden: true
---

<a id="Vercel_AI_Tencent_TencentModelCatalog"></a>

Namespace: [Vercel.AI.Tencent](/api/Vercel.AI.Tencent/)


```csharp
public static class TencentModelCatalog
```


Known Tencent model ids and the per-model policy. Upstream `tencent-model-catalog.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L49)

## Fields

<a id="Vercel_AI_Tencent_TencentModelCatalog_Entries"></a>

### Entries


```csharp
public static readonly IReadOnlyDictionary<string, TencentModelEntry> Entries
```


Entries of every known model id. Upstream `tencentModelCatalog`; renamed because C# does not allow a member named like its class.

**Value:** [IReadOnlyDictionary&lt;string, TencentModelEntry&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L79)

<a id="Vercel_AI_Tencent_TencentModelCatalog_TencentLanguageModelIds"></a>

### TencentLanguageModelIds


```csharp
public static readonly IReadOnlyList<string> TencentLanguageModelIds
```


TokenHub language model ids.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L52)

<a id="Vercel_AI_Tencent_TencentModelCatalog_TencentVideoModelIds"></a>

### TencentVideoModelIds


```csharp
public static readonly IReadOnlyList<string> TencentVideoModelIds
```


Video model ids.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L59)

<a id="Vercel_AI_Tencent_TencentModelCatalog_TokenHubOnlyOptions"></a>

### TokenHubOnlyOptions


```csharp
public static readonly IReadOnlyList<string> TokenHubOnlyOptions
```


Options that only TokenHub Kling models read.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L67)

<a id="Vercel_AI_Tencent_TencentModelCatalog_VodOnlyOptions"></a>

### VodOnlyOptions


```csharp
public static readonly IReadOnlyList<string> VodOnlyOptions
```


Options that only VOD models read.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L70)

## Methods

<a id="Vercel_AI_Tencent_TencentModelCatalog_GetTencentLanguageModelEntry_"></a>
<a id="Vercel_AI_Tencent_TencentModelCatalog_GetTencentLanguageModelEntry_System_String_"></a>

### GetTencentLanguageModelEntry(string)


```csharp
public static TencentLanguageModelEntry GetTencentLanguageModelEntry(string modelId)
```


Returns the catalog entry of a language model. Upstream `getTencentLanguageModelEntry`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [TencentLanguageModelEntry](/api/Vercel.AI.Tencent/TencentLanguageModelEntry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L128)

<a id="Vercel_AI_Tencent_TencentModelCatalog_GetTencentVideoModelEntry_"></a>
<a id="Vercel_AI_Tencent_TencentModelCatalog_GetTencentVideoModelEntry_System_String_"></a>

### GetTencentVideoModelEntry(string)


```csharp
public static TencentVideoModelEntry GetTencentVideoModelEntry(string modelId)
```


Returns the catalog entry of a video model. Upstream `getTencentVideoModelEntry`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [TencentVideoModelEntry](/api/Vercel.AI.Tencent/TencentVideoModelEntry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L140)

<a id="Vercel_AI_Tencent_TencentModelCatalog_GetTencentVideoRequestConfig_"></a>
<a id="Vercel_AI_Tencent_TencentModelCatalog_GetTencentVideoRequestConfig_System_String_"></a>

### GetTencentVideoRequestConfig(string)


```csharp
public static TencentVideoRequestConfig GetTencentVideoRequestConfig(string modelId)
```


Request-shaping policy for a video model. Upstream `getTencentVideoModelEntry(modelId).requestConfig.video`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [TencentVideoRequestConfig](/api/Vercel.AI.Tencent/TencentVideoRequestConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L201)

<a id="Vercel_AI_Tencent_TencentModelCatalog_ParseTencentVideoModelId_"></a>
<a id="Vercel_AI_Tencent_TencentModelCatalog_ParseTencentVideoModelId_System_String_"></a>

### ParseTencentVideoModelId(string)


```csharp
public static ParsedTencentVideoModelId? ParseTencentVideoModelId(string modelId)
```


Parses a video model id. Returns null for an id that is neither a TokenHub Kling nor a VOD id.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ParsedTencentVideoModelId](/api/Vercel.AI.Tencent/ParsedTencentVideoModelId/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L84)
