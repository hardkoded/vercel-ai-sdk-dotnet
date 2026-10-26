---
title: "StepFinishPart"
description: "A step finished."
editUrl: false
prev: false
next: false
sidebar:
  label: "StepFinishPart"
  hidden: true
---

<a id="Vercel_AI_StepFinishPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class StepFinishPart : TextStreamPart
```


A step finished.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1330)

## Constructors

<a id="Vercel_AI_StepFinishPart__ctor_"></a>
<a id="Vercel_AI_StepFinishPart__ctor_Vercel_AI_StepResult_Vercel_AI_LanguageModelResponseMetadata_System_String_"></a>

### StepFinishPart(StepResult, LanguageModelResponseMetadata, string?)


```csharp
public StepFinishPart(StepResult step, LanguageModelResponseMetadata response, string? rawFinishReason = null)
```


Creates a step-finish part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `step` | [StepResult](/api/Vercel.AI/StepResult/) |  |
| `response` | [LanguageModelResponseMetadata](/api/Vercel.AI/LanguageModelResponseMetadata/) |  |
| `rawFinishReason` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1333)

## Properties

<a id="Vercel_AI_StepFinishPart_Performance_"></a>
<a id="Vercel_AI_StepFinishPart_Performance"></a>

### Performance


```csharp
public StepPerformance Performance { get; }
```


Performance metrics of the step. Maps to `performance`.

**Value:** [StepPerformance](/api/Vercel.AI/StepPerformance/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1354)

<a id="Vercel_AI_StepFinishPart_ProviderMetadata_"></a>
<a id="Vercel_AI_StepFinishPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata of the step. Maps to `providerMetadata`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1351)

<a id="Vercel_AI_StepFinishPart_RawFinishReason_"></a>
<a id="Vercel_AI_StepFinishPart_RawFinishReason"></a>

### RawFinishReason


```csharp
public string? RawFinishReason { get; }
```


The finish reason exactly as the provider reported it, or &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt; when the provider reported none. Maps to `rawFinishReason`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1348)

<a id="Vercel_AI_StepFinishPart_Response_"></a>
<a id="Vercel_AI_StepFinishPart_Response"></a>

### Response


```csharp
public LanguageModelResponseMetadata Response { get; }
```


Response metadata of the step: id, model id, timestamp, and headers. Maps to `response`.

**Value:** [LanguageModelResponseMetadata](/api/Vercel.AI/LanguageModelResponseMetadata/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1345)

<a id="Vercel_AI_StepFinishPart_Step_"></a>
<a id="Vercel_AI_StepFinishPart_Step"></a>

### Step


```csharp
public StepResult Step { get; }
```


Completed step.

**Value:** [StepResult](/api/Vercel.AI/StepResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1342)
