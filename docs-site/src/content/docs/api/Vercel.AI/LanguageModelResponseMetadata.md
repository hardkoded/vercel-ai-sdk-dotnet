---
title: "LanguageModelResponseMetadata"
description: "Metadata for a language model response, as the finish-step part carries it. Maps to Omit<LanguageModelResponseMetadata, 'messages' | 'body'>."
editUrl: false
prev: false
next: false
sidebar:
  label: "LanguageModelResponseMetadata"
  hidden: true
---

<a id="Vercel_AI_LanguageModelResponseMetadata"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public class LanguageModelResponseMetadata
```


Metadata for a language model response, as the `finish-step` part carries it.
Maps to `Omit<LanguageModelResponseMetadata, 'messages' | 'body'>`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [StepResponse](/api/Vercel.AI/StepResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1039)

## Constructors

<a id="Vercel_AI_LanguageModelResponseMetadata__ctor_"></a>
<a id="Vercel_AI_LanguageModelResponseMetadata__ctor_System_String_System_DateTimeOffset_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `LanguageModelResponseMetadata(string, DateTimeOffset, string, IReadOnlyDictionary<string, string>?)`


```csharp
public LanguageModelResponseMetadata(string id, DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string>? headers = null)
```


Creates response metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `timestamp` | [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1042)

## Properties

<a id="Vercel_AI_LanguageModelResponseMetadata_Headers_"></a>
<a id="Vercel_AI_LanguageModelResponseMetadata_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; }
```


Response headers. &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt; for providers that do not use HTTP.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1060)

<a id="Vercel_AI_LanguageModelResponseMetadata_Id_"></a>
<a id="Vercel_AI_LanguageModelResponseMetadata_Id"></a>

### Id


```csharp
public string Id { get; }
```


ID for the generated response.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1051)

<a id="Vercel_AI_LanguageModelResponseMetadata_ModelId_"></a>
<a id="Vercel_AI_LanguageModelResponseMetadata_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


The ID of the response model that was used to generate the response.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1057)

<a id="Vercel_AI_LanguageModelResponseMetadata_Timestamp_"></a>
<a id="Vercel_AI_LanguageModelResponseMetadata_Timestamp"></a>

### Timestamp


```csharp
public DateTimeOffset Timestamp { get; }
```


Timestamp for the start of the generated response.

**Value:** [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1054)
