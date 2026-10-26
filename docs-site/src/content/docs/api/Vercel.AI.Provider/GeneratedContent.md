---
title: "GeneratedContent"
description: "Content the model generated."
editUrl: false
prev: false
next: false
sidebar:
  label: "GeneratedContent"
  hidden: true
---

<a id="Vercel_AI_Provider_GeneratedContent"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public abstract class GeneratedContent
```


Content the model generated.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [AnthropicCustomContent](/api/Vercel.AI.Anthropic/AnthropicCustomContent/), [AnthropicReasoningContent](/api/Vercel.AI.Anthropic/AnthropicReasoningContent/), [AnthropicToolResultContent](/api/Vercel.AI.Anthropic/AnthropicToolResultContent/), [GeneratedFileUrl](/api/Vercel.AI.GenerateText/GeneratedFileUrl/), [GoogleToolResult](/api/Vercel.AI.Google/GoogleToolResult/), [HuggingFaceToolResult](/api/Vercel.AI.HuggingFace/HuggingFaceToolResult/), [GeneratedCustom](/api/Vercel.AI.Provider/GeneratedCustom/), [GeneratedDocumentSource](/api/Vercel.AI.Provider/GeneratedDocumentSource/), [GeneratedFile](/api/Vercel.AI.Provider/GeneratedFile/), [GeneratedReasoning](/api/Vercel.AI.Provider/GeneratedReasoning/), [GeneratedSource](/api/Vercel.AI.Provider/GeneratedSource/), [GeneratedText](/api/Vercel.AI.Provider/GeneratedText/), [GeneratedToolApprovalRequest](/api/Vercel.AI.Provider/GeneratedToolApprovalRequest/), [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/), [GeneratedToolResult](/api/Vercel.AI.Provider/GeneratedToolResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L169)

## Constructors

<a id="Vercel_AI_Provider_GeneratedContent__ctor_"></a>
<a id="Vercel_AI_Provider_GeneratedContent__ctor_System_String_"></a>

### GeneratedContent(string)


```csharp
protected GeneratedContent(string type)
```


Creates content of the given type.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L172)

## Properties

<a id="Vercel_AI_Provider_GeneratedContent_Type_"></a>
<a id="Vercel_AI_Provider_GeneratedContent_Type"></a>

### Type


```csharp
public string Type { get; }
```


Content type name from the V4 specification.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L178)
