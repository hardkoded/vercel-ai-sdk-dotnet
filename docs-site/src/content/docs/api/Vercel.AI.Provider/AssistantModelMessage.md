---
title: "AssistantModelMessage"
description: "An assistant message."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssistantModelMessage"
  hidden: true
---

<a id="Vercel_AI_Provider_AssistantModelMessage"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class AssistantModelMessage : ModelMessage
```


An assistant message.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ModelMessage](/api/Vercel.AI.Provider/ModelMessage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L915)

## Constructors

<a id="Vercel_AI_Provider_AssistantModelMessage__ctor_"></a>
<a id="Vercel_AI_Provider_AssistantModelMessage__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_AssistantContentPart__"></a>

### `AssistantModelMessage(IReadOnlyList<AssistantContentPart>)`


```csharp
public AssistantModelMessage(IReadOnlyList<AssistantContentPart> parts)
```


Creates an assistant message from ordered parts. The text, reasoning, and tool calls are derived from them.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `parts` | [IReadOnlyList&lt;AssistantContentPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L956)

<a id="Vercel_AI_Provider_AssistantModelMessage__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_GeneratedToolCall__System_String_"></a>

### `AssistantModelMessage(string?, IReadOnlyList<GeneratedToolCall>?, string?)`


```csharp
public AssistantModelMessage(string? text, IReadOnlyList<GeneratedToolCall>? toolCalls, string? reasoning)
```


Creates an assistant message.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolCalls` | [IReadOnlyList&lt;GeneratedToolCall&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `reasoning` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L918)

<a id="Vercel_AI_Provider_AssistantModelMessage__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_GeneratedToolCall__System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__"></a>

### `AssistantModelMessage(string?, IReadOnlyList<GeneratedToolCall>?, string?, IReadOnlyDictionary<string, JsonElement>?)`


```csharp
public AssistantModelMessage(string? text, IReadOnlyList<GeneratedToolCall>? toolCalls, string? reasoning, IReadOnlyDictionary<string, JsonElement>? reasoningProviderOptions)
```


Creates an assistant message whose reasoning carries provider options, such as an OpenAI
`itemId` for reasoning stored by the Responses API.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolCalls` | [IReadOnlyList&lt;GeneratedToolCall&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `reasoning` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `reasoningProviderOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L927)

<a id="Vercel_AI_Provider_AssistantModelMessage__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_GeneratedToolCall__System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_AssistantContentPart__"></a>

### `AssistantModelMessage(string?, IReadOnlyList<GeneratedToolCall>?, string?, IReadOnlyDictionary<string, JsonElement>?, IReadOnlyList<AssistantContentPart>?)`


```csharp
public AssistantModelMessage(string? text, IReadOnlyList<GeneratedToolCall>? toolCalls, string? reasoning, IReadOnlyDictionary<string, JsonElement>? reasoningProviderOptions, IReadOnlyList<AssistantContentPart>? parts)
```


Creates an assistant message that also keeps its content parts in order. When &lt;code class="paramref"&gt;parts&lt;/code&gt; is null,
the parts are the reasoning, the text, and the tool calls, in that order.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolCalls` | [IReadOnlyList&lt;GeneratedToolCall&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `reasoning` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `reasoningProviderOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `parts` | [IReadOnlyList&lt;AssistantContentPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L940)

## Properties

<a id="Vercel_AI_Provider_AssistantModelMessage_Parts_"></a>
<a id="Vercel_AI_Provider_AssistantModelMessage_Parts"></a>

### Parts


```csharp
public IReadOnlyList<AssistantContentPart> Parts { get; }
```


The content parts in the order the model produced them, each with its own provider options.

**Value:** [IReadOnlyList&lt;AssistantContentPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1039)

<a id="Vercel_AI_Provider_AssistantModelMessage_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_AssistantModelMessage_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
```


Provider-specific options for this message, such as an Amazon Bedrock `cachePoint`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1024)

<a id="Vercel_AI_Provider_AssistantModelMessage_Reasoning_"></a>
<a id="Vercel_AI_Provider_AssistantModelMessage_Reasoning"></a>

### Reasoning


```csharp
public string? Reasoning { get; }
```


Reasoning text, when the provider returned it.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1033)

<a id="Vercel_AI_Provider_AssistantModelMessage_ReasoningProviderOptions_"></a>
<a id="Vercel_AI_Provider_AssistantModelMessage_ReasoningProviderOptions"></a>

### ReasoningProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ReasoningProviderOptions { get; }
```


Provider options of the reasoning part, such as an OpenAI `itemId`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1036)

<a id="Vercel_AI_Provider_AssistantModelMessage_Text_"></a>
<a id="Vercel_AI_Provider_AssistantModelMessage_Text"></a>

### Text


```csharp
public string? Text { get; }
```


Assistant text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1027)

<a id="Vercel_AI_Provider_AssistantModelMessage_ToolCalls_"></a>
<a id="Vercel_AI_Provider_AssistantModelMessage_ToolCalls"></a>

### ToolCalls


```csharp
public IReadOnlyList<GeneratedToolCall> ToolCalls { get; }
```


Tool calls made in this turn.

**Value:** [IReadOnlyList&lt;GeneratedToolCall&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1030)

## Methods

<a id="Vercel_AI_Provider_AssistantModelMessage_FromGeneratedContent_"></a>
<a id="Vercel_AI_Provider_AssistantModelMessage_FromGeneratedContent_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_GeneratedContent__"></a>

### `FromGeneratedContent(IReadOnlyList<GeneratedContent>)`


```csharp
public static AssistantModelMessage FromGeneratedContent(IReadOnlyList<GeneratedContent> content)
```


Creates an assistant message from generated content, keeping order and each part's provider metadata as its provider options.
This is how a generate result is replayed as history. Sources and files have no assistant part and are skipped.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `content` | [IReadOnlyList&lt;GeneratedContent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [AssistantModelMessage](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L970)
