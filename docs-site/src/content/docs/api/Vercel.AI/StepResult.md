---
title: "StepResult"
description: "One language-model step, including any tool results from that step."
editUrl: false
prev: false
next: false
sidebar:
  label: "StepResult"
  hidden: true
---

<a id="Vercel_AI_StepResult"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class StepResult
```


One language-model step, including any tool results from that step.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L223)

## Constructors

<a id="Vercel_AI_StepResult__ctor_"></a>
<a id="Vercel_AI_StepResult__ctor_System_String_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_GeneratedToolCall__System_Collections_Generic_IReadOnlyList_Vercel_AI_ExecutedTool__Vercel_AI_Provider_FinishReason_Vercel_AI_Provider_LanguageModelUsage_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_GeneratedSource__System_Nullable_System_Text_Json_JsonElement__System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_CallWarning__System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_GeneratedFile__System_String_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_GeneratedContent__Vercel_AI_StepPerformance_Vercel_AI_LanguageModelRequestMetadata_Vercel_AI_StepResponse_System_String_"></a>

### `StepResult(string, string?, IReadOnlyList<GeneratedToolCall>, IReadOnlyList<ExecutedTool>, FinishReason, LanguageModelUsage, IReadOnlyList<GeneratedSource>, JsonElement?, IReadOnlyList<CallWarning>?, IReadOnlyList<GeneratedFile>?, string?, string?, IReadOnlyList<GeneratedContent>?, StepPerformance?, LanguageModelRequestMetadata?, StepResponse?, string?)`


```csharp
public StepResult(string text, string? reasoningText, IReadOnlyList<GeneratedToolCall> toolCalls, IReadOnlyList<ExecutedTool> toolResults, FinishReason finishReason, LanguageModelUsage usage, IReadOnlyList<GeneratedSource> sources, JsonElement? providerMetadata = null, IReadOnlyList<CallWarning>? warnings = null, IReadOnlyList<GeneratedFile>? files = null, string? provider = null, string? modelId = null, IReadOnlyList<GeneratedContent>? content = null, StepPerformance? performance = null, LanguageModelRequestMetadata? request = null, StepResponse? response = null, string? rawFinishReason = null)
```


Creates a step result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `reasoningText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolCalls` | [IReadOnlyList&lt;GeneratedToolCall&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `toolResults` | [IReadOnlyList&lt;ExecutedTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `finishReason` | [FinishReason](/api/Vercel.AI.Provider/FinishReason/) |  |
| `usage` | [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/) |  |
| `sources` | [IReadOnlyList&lt;GeneratedSource&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `warnings` | [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `files` | [IReadOnlyList&lt;GeneratedFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `content` | [IReadOnlyList&lt;GeneratedContent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `performance` | [StepPerformance](/api/Vercel.AI/StepPerformance/) |  |
| `request` | [LanguageModelRequestMetadata](/api/Vercel.AI/LanguageModelRequestMetadata/) |  |
| `response` | [StepResponse](/api/Vercel.AI/StepResponse/) |  |
| `rawFinishReason` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L226)

## Properties

<a id="Vercel_AI_StepResult_Content_"></a>
<a id="Vercel_AI_StepResult_Content"></a>

### Content


```csharp
public IReadOnlyList<GeneratedContent> Content { get; }
```


Content the model generated in this step, in order, followed by the results of the client tools the loop ran. A call that failed to
parse stays here as an [Invalid](/api/Vercel.AI.Provider/GeneratedToolCall/#Vercel_AI_Provider_GeneratedToolCall_Invalid) call, followed by its error result.
A [GeneratedText](/api/Vercel.AI.Provider/GeneratedText/) carries its [Citations](/api/Vercel.AI.Provider/GeneratedText/#Vercel_AI_Provider_GeneratedText_Citations), separate from the retrieved [Sources](#Vercel_AI_StepResult_Sources).

**Value:** [IReadOnlyList&lt;GeneratedContent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L275)

<a id="Vercel_AI_StepResult_Files_"></a>
<a id="Vercel_AI_StepResult_Files"></a>

### Files


```csharp
public IReadOnlyList<GeneratedFile> Files { get; }
```


Files generated in this step.

**Value:** [IReadOnlyList&lt;GeneratedFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L305)

<a id="Vercel_AI_StepResult_FinishReason_"></a>
<a id="Vercel_AI_StepResult_FinishReason"></a>

### FinishReason


```csharp
public FinishReason FinishReason { get; }
```


Finish reason.

**Value:** [FinishReason](/api/Vercel.AI.Provider/FinishReason/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L287)

<a id="Vercel_AI_StepResult_ModelId_"></a>
<a id="Vercel_AI_StepResult_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; }
```


Model id of the model that produced this step, when known.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L311)

<a id="Vercel_AI_StepResult_Performance_"></a>
<a id="Vercel_AI_StepResult_Performance"></a>

### Performance


```csharp
public StepPerformance Performance { get; }
```


Performance metrics of this step. Streaming-only metrics are &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt; for `generateText`.

**Value:** [StepPerformance](/api/Vercel.AI/StepPerformance/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L314)

<a id="Vercel_AI_StepResult_Provider_"></a>
<a id="Vercel_AI_StepResult_Provider"></a>

### Provider


```csharp
public string? Provider { get; }
```


Provider id of the model that produced this step, when known.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L308)

<a id="Vercel_AI_StepResult_ProviderMetadata_"></a>
<a id="Vercel_AI_StepResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this step, including cost and native-tool counts when the provider sent them.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L299)

<a id="Vercel_AI_StepResult_RawFinishReason_"></a>
<a id="Vercel_AI_StepResult_RawFinishReason"></a>

### RawFinishReason


```csharp
public string? RawFinishReason { get; }
```


The finish reason exactly as the provider reported it, or &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt; when the provider reported none. Maps to `rawFinishReason`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L290)

<a id="Vercel_AI_StepResult_ReasoningText_"></a>
<a id="Vercel_AI_StepResult_ReasoningText"></a>

### ReasoningText


```csharp
public string? ReasoningText { get; }
```


Reasoning text generated in this step.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L278)

<a id="Vercel_AI_StepResult_Request_"></a>
<a id="Vercel_AI_StepResult_Request"></a>

### Request


```csharp
public LanguageModelRequestMetadata Request { get; }
```


Additional request information for this step. [Body](/api/Vercel.AI/LanguageModelRequestMetadata/#Vercel_AI_LanguageModelRequestMetadata_Body) and [Messages](/api/Vercel.AI/LanguageModelRequestMetadata/#Vercel_AI_LanguageModelRequestMetadata_Messages)
are set only when [Include](/api/Vercel.AI/StreamTextOptions/#Vercel_AI_StreamTextOptions_Include) asks for them. Maps to `request`.

**Value:** [LanguageModelRequestMetadata](/api/Vercel.AI/LanguageModelRequestMetadata/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L320)

<a id="Vercel_AI_StepResult_Response_"></a>
<a id="Vercel_AI_StepResult_Response"></a>

### Response


```csharp
public StepResponse Response { get; }
```


Additional response information for this step, with the messages this step added to the conversation. Maps to `response`.

**Value:** [StepResponse](/api/Vercel.AI/StepResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L323)

<a id="Vercel_AI_StepResult_Sources_"></a>
<a id="Vercel_AI_StepResult_Sources"></a>

### Sources


```csharp
public IReadOnlyList<GeneratedSource> Sources { get; }
```


Sources cited in this step.

**Value:** [IReadOnlyList&lt;GeneratedSource&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L296)

<a id="Vercel_AI_StepResult_Text_"></a>
<a id="Vercel_AI_StepResult_Text"></a>

### Text


```csharp
public string Text { get; }
```


Text generated in this step.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L265)

<a id="Vercel_AI_StepResult_ToolCalls_"></a>
<a id="Vercel_AI_StepResult_ToolCalls"></a>

### ToolCalls


```csharp
public IReadOnlyList<GeneratedToolCall> ToolCalls { get; }
```


Tool calls from this step.

**Value:** [IReadOnlyList&lt;GeneratedToolCall&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L281)

<a id="Vercel_AI_StepResult_ToolResults_"></a>
<a id="Vercel_AI_StepResult_ToolResults"></a>

### ToolResults


```csharp
public IReadOnlyList<ExecutedTool> ToolResults { get; }
```


Tool results from this step.

**Value:** [IReadOnlyList&lt;ExecutedTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L284)

<a id="Vercel_AI_StepResult_Usage_"></a>
<a id="Vercel_AI_StepResult_Usage"></a>

### Usage


```csharp
public LanguageModelUsage Usage { get; }
```


Usage for this step.

**Value:** [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L293)

<a id="Vercel_AI_StepResult_Warnings_"></a>
<a id="Vercel_AI_StepResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<CallWarning> Warnings { get; }
```


Warnings reported for this step.

**Value:** [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L302)
