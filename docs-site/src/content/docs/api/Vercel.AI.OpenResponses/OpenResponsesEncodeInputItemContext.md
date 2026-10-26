---
title: "OpenResponsesEncodeInputItemContext"
description: "Input of EncodeInputItem."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenResponsesEncodeInputItemContext"
  hidden: true
---

<a id="Vercel_AI_OpenResponses_OpenResponsesEncodeInputItemContext"></a>

Namespace: [Vercel.AI.OpenResponses](/api/Vercel.AI.OpenResponses/)


```csharp
public sealed class OpenResponsesEncodeInputItemContext
```


Input of [EncodeInputItem](/api/Vercel.AI.OpenResponses/OpenResponsesExtension/#Vercel_AI_OpenResponses_OpenResponsesExtension_EncodeInputItem).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L57)

## Constructors

<a id="Vercel_AI_OpenResponses_OpenResponsesEncodeInputItemContext__ctor_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesEncodeInputItemContext__ctor_Vercel_AI_Provider_AssistantContentPart_Vercel_AI_Provider_LanguageModelTool_"></a>

### OpenResponsesEncodeInputItemContext(AssistantContentPart, LanguageModelTool)


```csharp
public OpenResponsesEncodeInputItemContext(AssistantContentPart part, LanguageModelTool tool)
```


Creates the context.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `part` | [AssistantContentPart](/api/Vercel.AI.Provider/AssistantContentPart/) |  |
| `tool` | [LanguageModelTool](/api/Vercel.AI.Provider/LanguageModelTool/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L60)

## Properties

<a id="Vercel_AI_OpenResponses_OpenResponsesEncodeInputItemContext_Part_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesEncodeInputItemContext_Part"></a>

### Part


```csharp
public AssistantContentPart Part { get; }
```


The history part: an [AssistantToolCallPart](/api/Vercel.AI.Provider/AssistantToolCallPart/) or an [AssistantToolResultPart](/api/Vercel.AI.Provider/AssistantToolResultPart/).

**Value:** [AssistantContentPart](/api/Vercel.AI.Provider/AssistantContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L67)

<a id="Vercel_AI_OpenResponses_OpenResponsesEncodeInputItemContext_Tool_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesEncodeInputItemContext_Tool"></a>

### Tool


```csharp
public LanguageModelTool Tool { get; }
```


The provider-defined tool the part belongs to.

**Value:** [LanguageModelTool](/api/Vercel.AI.Provider/LanguageModelTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L70)
