---
title: "LanguageModelStreamPart"
description: "One part of a language-model stream."
editUrl: false
prev: false
next: false
sidebar:
  label: "LanguageModelStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_LanguageModelStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public abstract class LanguageModelStreamPart
```


One part of a language-model stream.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [FileStreamPart](/api/Vercel.AI.Google/FileStreamPart/), [CustomStreamPart](/api/Vercel.AI.Provider/CustomStreamPart/), [DocumentSourceStreamPart](/api/Vercel.AI.Provider/DocumentSourceStreamPart/), [ErrorStreamPart](/api/Vercel.AI.Provider/ErrorStreamPart/), [FinishStreamPart](/api/Vercel.AI.Provider/FinishStreamPart/), [RawStreamPart](/api/Vercel.AI.Provider/RawStreamPart/), [ReasoningDeltaStreamPart](/api/Vercel.AI.Provider/ReasoningDeltaStreamPart/), [ReasoningEndStreamPart](/api/Vercel.AI.Provider/ReasoningEndStreamPart/), [ReasoningStartStreamPart](/api/Vercel.AI.Provider/ReasoningStartStreamPart/), [ResponseMetadataStreamPart](/api/Vercel.AI.Provider/ResponseMetadataStreamPart/), [SourceStreamPart](/api/Vercel.AI.Provider/SourceStreamPart/), [StreamStartStreamPart](/api/Vercel.AI.Provider/StreamStartStreamPart/), [TextDeltaStreamPart](/api/Vercel.AI.Provider/TextDeltaStreamPart/), [TextEndStreamPart](/api/Vercel.AI.Provider/TextEndStreamPart/), [TextStartStreamPart](/api/Vercel.AI.Provider/TextStartStreamPart/), [ToolApprovalRequestStreamPart](/api/Vercel.AI.Provider/ToolApprovalRequestStreamPart/), [ToolCallStreamPart](/api/Vercel.AI.Provider/ToolCallStreamPart/), [ToolInputDeltaStreamPart](/api/Vercel.AI.Provider/ToolInputDeltaStreamPart/), [ToolInputEndStreamPart](/api/Vercel.AI.Provider/ToolInputEndStreamPart/), [ToolInputStartStreamPart](/api/Vercel.AI.Provider/ToolInputStartStreamPart/), [ToolResultStreamPart](/api/Vercel.AI.Provider/ToolResultStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1641)

## Constructors

<a id="Vercel_AI_Provider_LanguageModelStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamPart__ctor_System_String_"></a>

### LanguageModelStreamPart(string)


```csharp
protected LanguageModelStreamPart(string type)
```


Creates a stream part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1644)

## Properties

<a id="Vercel_AI_Provider_LanguageModelStreamPart_Chunk_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamPart_Chunk"></a>

### Chunk


```csharp
public JsonElement? Chunk { get; }
```


The provider chunk this part was parsed from, with every field the typed part does not model.
It is set only by providers that pass chunks through unchanged, such as the Gateway.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1656)

<a id="Vercel_AI_Provider_LanguageModelStreamPart_Type_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamPart_Type"></a>

### Type


```csharp
public string Type { get; }
```


Part type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1650)
