---
title: "FinishStreamPart"
description: "The stream finished."
editUrl: false
prev: false
next: false
sidebar:
  label: "FinishStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_FinishStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class FinishStreamPart : LanguageModelStreamPart
```


The stream finished.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1936)

## Constructors

<a id="Vercel_AI_Provider_FinishStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_FinishStreamPart__ctor_Vercel_AI_Provider_FinishReason_Vercel_AI_Provider_LanguageModelUsage_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### FinishStreamPart(FinishReason, LanguageModelUsage, string?, JsonElement?)


```csharp
public FinishStreamPart(FinishReason finishReason, LanguageModelUsage usage, string? rawFinishReason = null, JsonElement? providerMetadata = null)
```


Creates a finish part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `finishReason` | [FinishReason](/api/Vercel.AI.Provider/FinishReason/) |  |
| `usage` | [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/) |  |
| `rawFinishReason` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1939)

## Properties

<a id="Vercel_AI_Provider_FinishStreamPart_FinishReason_"></a>
<a id="Vercel_AI_Provider_FinishStreamPart_FinishReason"></a>

### FinishReason


```csharp
public FinishReason FinishReason { get; }
```


Finish reason.

**Value:** [FinishReason](/api/Vercel.AI.Provider/FinishReason/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1953)

<a id="Vercel_AI_Provider_FinishStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_FinishStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata reported with the finish event.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1962)

<a id="Vercel_AI_Provider_FinishStreamPart_RawFinishReason_"></a>
<a id="Vercel_AI_Provider_FinishStreamPart_RawFinishReason"></a>

### RawFinishReason


```csharp
public string? RawFinishReason { get; }
```


Provider finish reason.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1959)

<a id="Vercel_AI_Provider_FinishStreamPart_Usage_"></a>
<a id="Vercel_AI_Provider_FinishStreamPart_Usage"></a>

### Usage


```csharp
public LanguageModelUsage Usage { get; }
```


Usage reported with the finish part.

**Value:** [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1956)
