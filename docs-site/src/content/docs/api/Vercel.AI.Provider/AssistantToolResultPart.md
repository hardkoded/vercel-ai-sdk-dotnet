---
title: "AssistantToolResultPart"
description: "A provider-executed tool result in an assistant message."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssistantToolResultPart"
  hidden: true
---

<a id="Vercel_AI_Provider_AssistantToolResultPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class AssistantToolResultPart : AssistantContentPart
```


A provider-executed tool result in an assistant message.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [AssistantContentPart](/api/Vercel.AI.Provider/AssistantContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1116)

## Constructors

<a id="Vercel_AI_Provider_AssistantToolResultPart__ctor_"></a>
<a id="Vercel_AI_Provider_AssistantToolResultPart__ctor_System_String_System_String_Vercel_AI_Provider_ToolModelMessage_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__"></a>

### `AssistantToolResultPart(string, string, ToolModelMessage, IReadOnlyDictionary<string, JsonElement>?)`


```csharp
public AssistantToolResultPart(string toolCallId, string toolName, ToolModelMessage output, IReadOnlyDictionary<string, JsonElement>? providerOptions = null)
```


Creates a tool-result part. &lt;code class="paramref"&gt;output&lt;/code&gt; carries the output type and value.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `output` | [ToolModelMessage](/api/Vercel.AI.Provider/ToolModelMessage/) |  |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1119)

## Properties

<a id="Vercel_AI_Provider_AssistantToolResultPart_Output_"></a>
<a id="Vercel_AI_Provider_AssistantToolResultPart_Output"></a>

### Output


```csharp
public ToolModelMessage Output { get; }
```


The output, as a tool message with the same call id and name. Use its [OutputType](/api/Vercel.AI.Provider/ToolModelMessage/#Vercel_AI_Provider_ToolModelMessage_OutputType) and [OutputJson](/api/Vercel.AI.Provider/ToolModelMessage/#Vercel_AI_Provider_ToolModelMessage_OutputJson).

**Value:** [ToolModelMessage](/api/Vercel.AI.Provider/ToolModelMessage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1138)

<a id="Vercel_AI_Provider_AssistantToolResultPart_ToolCallId_"></a>
<a id="Vercel_AI_Provider_AssistantToolResultPart_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Matching tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1132)

<a id="Vercel_AI_Provider_AssistantToolResultPart_ToolName_"></a>
<a id="Vercel_AI_Provider_AssistantToolResultPart_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1135)
