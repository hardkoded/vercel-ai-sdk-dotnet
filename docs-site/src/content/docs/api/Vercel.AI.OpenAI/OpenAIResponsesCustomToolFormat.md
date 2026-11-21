---
title: "OpenAIResponsesCustomToolFormat"
description: "The input format of a custom tool: free text, or a regex / lark grammar."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIResponsesCustomToolFormat"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIResponsesCustomToolFormat"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIResponsesCustomToolFormat
```


The input format of a custom tool: free text, or a `regex` / `lark` grammar.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesAdditionalTool.cs#L90)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIResponsesCustomToolFormat_Grammar_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesCustomToolFormat_Grammar_System_String_System_String_"></a>

### Grammar(string, string)


```csharp
public static OpenAIResponsesCustomToolFormat Grammar(string syntax, string definition)
```


A grammar (`type: 'grammar'`). &lt;code class="paramref"&gt;syntax&lt;/code&gt; is `regex` or `lark`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `syntax` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `definition` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OpenAIResponsesCustomToolFormat](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesAdditionalTool.cs#L96)

<a id="Vercel_AI_OpenAI_OpenAIResponsesCustomToolFormat_Text_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesCustomToolFormat_Text"></a>

### Text()


```csharp
public static OpenAIResponsesCustomToolFormat Text()
```


Free-text input (`type: 'text'`).

**Returns:** [OpenAIResponsesCustomToolFormat](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesAdditionalTool.cs#L93)
