---
title: "GenerateTextResult"
description: "Result of generateText."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateTextResult"
  hidden: true
---

<a id="Vercel_AI_GenerateTextResult"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class GenerateTextResult
```


Result of `generateText`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L890)

## Constructors

<a id="Vercel_AI_GenerateTextResult__ctor_"></a>
<a id="Vercel_AI_GenerateTextResult__ctor_System_String_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_StepResult__Vercel_AI_Provider_FinishReason_Vercel_AI_Provider_LanguageModelUsage_System_Nullable_System_Text_Json_JsonElement__System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_GeneratedSource__System_Nullable_System_Text_Json_JsonElement__"></a>

### `GenerateTextResult(string, string?, IReadOnlyList<StepResult>, FinishReason, LanguageModelUsage, JsonElement?, IReadOnlyList<GeneratedSource>, JsonElement?)`


```csharp
public GenerateTextResult(string text, string? reasoningText, IReadOnlyList<StepResult> steps, FinishReason finishReason, LanguageModelUsage usage, JsonElement? output, IReadOnlyList<GeneratedSource> sources, JsonElement? providerMetadata = null)
```


Creates a result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `reasoningText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `steps` | [IReadOnlyList&lt;StepResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `finishReason` | [FinishReason](/api/Vercel.AI.Provider/FinishReason/) |  |
| `usage` | [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/) |  |
| `output` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `sources` | [IReadOnlyList&lt;GeneratedSource&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L893)

## Properties

<a id="Vercel_AI_GenerateTextResult_Content_"></a>
<a id="Vercel_AI_GenerateTextResult_Content"></a>

### Content


```csharp
public IReadOnlyList<GeneratedContent> Content { get; }
```


Content from the last step. See [Content](/api/Vercel.AI/StepResult/#Vercel_AI_StepResult_Content).

**Value:** [IReadOnlyList&lt;GeneratedContent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L954)

<a id="Vercel_AI_GenerateTextResult_Files_"></a>
<a id="Vercel_AI_GenerateTextResult_Files"></a>

### Files


```csharp
public IReadOnlyList<GeneratedFile> Files { get; }
```


Files from every step.

**Value:** [IReadOnlyList&lt;GeneratedFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L990)

<a id="Vercel_AI_GenerateTextResult_FinalStep_"></a>
<a id="Vercel_AI_GenerateTextResult_FinalStep"></a>

### FinalStep


```csharp
public StepResult FinalStep { get; }
```


The last completed step.

**Value:** [StepResult](/api/Vercel.AI/StepResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L934)

<a id="Vercel_AI_GenerateTextResult_FinishReason_"></a>
<a id="Vercel_AI_GenerateTextResult_FinishReason"></a>

### FinishReason


```csharp
public FinishReason FinishReason { get; }
```


Finish reason of the last step.

**Value:** [FinishReason](/api/Vercel.AI.Provider/FinishReason/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L960)

<a id="Vercel_AI_GenerateTextResult_Output_"></a>
<a id="Vercel_AI_GenerateTextResult_Output"></a>

### Output


```csharp
public JsonElement? Output { get; }
```


Parsed structured output, when `String)` was used.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L966)

<a id="Vercel_AI_GenerateTextResult_ProviderMetadata_"></a>
<a id="Vercel_AI_GenerateTextResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata from the last step.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L978)

<a id="Vercel_AI_GenerateTextResult_ReasoningText_"></a>
<a id="Vercel_AI_GenerateTextResult_ReasoningText"></a>

### ReasoningText


```csharp
public string? ReasoningText { get; }
```


Reasoning from the last step.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L951)

<a id="Vercel_AI_GenerateTextResult_Request_"></a>
<a id="Vercel_AI_GenerateTextResult_Request"></a>

### Request


```csharp
public LanguageModelRequestMetadata Request { get; }
```


Additional request information of the last step. Maps to `request`. [GenerateTextOptions](/api/Vercel.AI/GenerateTextOptions/) has no include option yet, so `Body` and `Messages` are &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt;.

**Value:** [LanguageModelRequestMetadata](/api/Vercel.AI/LanguageModelRequestMetadata/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L981)

<a id="Vercel_AI_GenerateTextResult_Response_"></a>
<a id="Vercel_AI_GenerateTextResult_Response"></a>

### Response


```csharp
public StepResponse Response { get; }
```


Additional response information of the last step. Maps to `response`.

**Value:** [StepResponse](/api/Vercel.AI/StepResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L984)

<a id="Vercel_AI_GenerateTextResult_ResponseMessages_"></a>
<a id="Vercel_AI_GenerateTextResult_ResponseMessages"></a>

### ResponseMessages


```csharp
public IReadOnlyList<ModelMessage> ResponseMessages { get; init; }
```


Messages this call added to the conversation: the assistant message of each step, with any tool approval request, and the
tool results the client ran. Pass them back with the caller's approval responses to continue the conversation.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L996)

<a id="Vercel_AI_GenerateTextResult_Sources_"></a>
<a id="Vercel_AI_GenerateTextResult_Sources"></a>

### Sources


```csharp
public IReadOnlyList<GeneratedSource> Sources { get; }
```


Sources from every step.

**Value:** [IReadOnlyList&lt;GeneratedSource&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L975)

<a id="Vercel_AI_GenerateTextResult_Steps_"></a>
<a id="Vercel_AI_GenerateTextResult_Steps"></a>

### Steps


```csharp
public IReadOnlyList<StepResult> Steps { get; }
```


Every step.

**Value:** [IReadOnlyList&lt;StepResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L957)

<a id="Vercel_AI_GenerateTextResult_Text_"></a>
<a id="Vercel_AI_GenerateTextResult_Text"></a>

### Text


```csharp
public string Text { get; }
```


Text from the last step.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L948)

<a id="Vercel_AI_GenerateTextResult_ToolCalls_"></a>
<a id="Vercel_AI_GenerateTextResult_ToolCalls"></a>

### ToolCalls


```csharp
public IReadOnlyList<GeneratedToolCall> ToolCalls { get; }
```


Tool calls from every step.

**Value:** [IReadOnlyList&lt;GeneratedToolCall&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L969)

<a id="Vercel_AI_GenerateTextResult_ToolResults_"></a>
<a id="Vercel_AI_GenerateTextResult_ToolResults"></a>

### ToolResults


```csharp
public IReadOnlyList<ExecutedTool> ToolResults { get; }
```


Tool results from every step.

**Value:** [IReadOnlyList&lt;ExecutedTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L972)

<a id="Vercel_AI_GenerateTextResult_Usage_"></a>
<a id="Vercel_AI_GenerateTextResult_Usage"></a>

### Usage


```csharp
public LanguageModelUsage Usage { get; }
```


Usage summed across steps.

**Value:** [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L963)

<a id="Vercel_AI_GenerateTextResult_Warnings_"></a>
<a id="Vercel_AI_GenerateTextResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<CallWarning> Warnings { get; }
```


Warnings from every step.

**Value:** [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L987)
