---
title: "OpenAIResponsesSystemMessageOptions"
description: "The openai options of a system message in Responses history. Maps to OpenAIResponsesSystemMessageOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIResponsesSystemMessageOptions"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIResponsesSystemMessageOptions"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIResponsesSystemMessageOptions
```


The `openai` options of a system message in Responses history. Maps to
`OpenAIResponsesSystemMessageOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesSystemMessageOptions.cs#L14)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIResponsesSystemMessageOptions_AdditionalTools_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesSystemMessageOptions_AdditionalTools"></a>

### AdditionalTools


```csharp
public IReadOnlyList<OpenAIResponsesAdditionalTool>? AdditionalTools { get; set; }
```


Tool definitions to load at this position in history. Requires empty system message content.

**Value:** [IReadOnlyList&lt;OpenAIResponsesAdditionalTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesSystemMessageOptions.cs#L29)

<a id="Vercel_AI_OpenAI_OpenAIResponsesSystemMessageOptions_ReasoningEffortUpdate_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesSystemMessageOptions_ReasoningEffortUpdate"></a>

### ReasoningEffortUpdate


```csharp
public string? ReasoningEffortUpdate { get; set; }
```


Emits a configuration update at this position in history: `none`, `low`, `medium`,
`high`, `xhigh` or `max`. Requires empty system message content.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesSystemMessageOptions.cs#L20)

<a id="Vercel_AI_OpenAI_OpenAIResponsesSystemMessageOptions_TextFormatUpdate_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesSystemMessageOptions_TextFormatUpdate"></a>

### TextFormatUpdate


```csharp
public JsonElement? TextFormatUpdate { get; set; }
```


Replaces the response format at this position in history (a `text` or `json_schema` format).
Requires empty system message content.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesSystemMessageOptions.cs#L26)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIResponsesSystemMessageOptions_ToProviderOptions_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesSystemMessageOptions_ToProviderOptions_System_String_"></a>

### ToProviderOptions(string)


```csharp
public IReadOnlyDictionary<string, JsonElement> ToProviderOptions(string providerOptionsName = "openai")
```


Returns the options as provider options under &lt;code class="paramref"&gt;providerOptionsName&lt;/code&gt; (`openai` by
default), ready for `SystemModelMessage.ProviderOptions`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptionsName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesSystemMessageOptions.cs#L35)
