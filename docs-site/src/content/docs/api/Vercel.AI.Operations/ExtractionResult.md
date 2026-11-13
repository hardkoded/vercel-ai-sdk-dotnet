---
title: "ExtractionResult"
description: "Extraction result. Maps to ExtractionResult."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExtractionResult"
  hidden: true
---

<a id="Vercel_AI_Operations_ExtractionResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ExtractionResult
```


Extraction result. Maps to `ExtractionResult`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L11)

## Constructors

<a id="Vercel_AI_Operations_ExtractionResult__ctor_"></a>
<a id="Vercel_AI_Operations_ExtractionResult__ctor_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_ExtractionAnswer__Vercel_AI_Operations_OperationUsage_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_ProviderResponse_"></a>

### `ExtractionResult(IReadOnlyDictionary<string, ExtractionAnswer>, OperationUsage, IReadOnlyList<OperationWarning>, JsonElement?, ProviderResponse)`


```csharp
public ExtractionResult(IReadOnlyDictionary<string, ExtractionAnswer> answers, OperationUsage usage, IReadOnlyList<OperationWarning> warnings, JsonElement? providerMetadata, ProviderResponse response)
```


Creates a result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `answers` | [IReadOnlyDictionary&lt;string, ExtractionAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `usage` | [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L14)

## Properties

<a id="Vercel_AI_Operations_ExtractionResult_Answers_"></a>
<a id="Vercel_AI_Operations_ExtractionResult_Answers"></a>

### Answers


```csharp
public IReadOnlyDictionary<string, ExtractionAnswer> Answers { get; }
```


One answer per field, under the field ids. A value is null when the state has nothing for the field.

**Value:** [IReadOnlyDictionary&lt;string, ExtractionAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L24)

<a id="Vercel_AI_Operations_ExtractionResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_ExtractionResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L33)

<a id="Vercel_AI_Operations_ExtractionResult_Response_"></a>
<a id="Vercel_AI_Operations_ExtractionResult_Response"></a>

### Response


```csharp
public ProviderResponse Response { get; }
```


Response. Timestamp and model id are filled when the provider omitted them.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L36)

<a id="Vercel_AI_Operations_ExtractionResult_Usage_"></a>
<a id="Vercel_AI_Operations_ExtractionResult_Usage"></a>

### Usage


```csharp
public OperationUsage Usage { get; }
```


Usage. [TotalTokens](/api/Vercel.AI.Operations/OperationUsage/#Vercel_AI_Operations_OperationUsage_TotalTokens) is input plus output when both are set.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L27)

<a id="Vercel_AI_Operations_ExtractionResult_Warnings_"></a>
<a id="Vercel_AI_Operations_ExtractionResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Extract.cs#L30)
