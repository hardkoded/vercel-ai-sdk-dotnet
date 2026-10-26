---
title: "AssistantToolCallPart"
description: "A tool call in an assistant message."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssistantToolCallPart"
  hidden: true
---

<a id="Vercel_AI_Provider_AssistantToolCallPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class AssistantToolCallPart : AssistantContentPart
```


A tool call in an assistant message.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [AssistantContentPart](/api/Vercel.AI.Provider/AssistantContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L863)

## Constructors

<a id="Vercel_AI_Provider_AssistantToolCallPart__ctor_"></a>
<a id="Vercel_AI_Provider_AssistantToolCallPart__ctor_Vercel_AI_Provider_GeneratedToolCall_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__"></a>

### `AssistantToolCallPart(GeneratedToolCall, IReadOnlyDictionary<string, JsonElement>?)`


```csharp
public AssistantToolCallPart(GeneratedToolCall toolCall, IReadOnlyDictionary<string, JsonElement>? providerOptions = null)
```


Creates a tool-call part. Without &lt;code class="paramref"&gt;providerOptions&lt;/code&gt;, the call's provider metadata is used.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCall` | [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/) |  |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L866)

## Properties

<a id="Vercel_AI_Provider_AssistantToolCallPart_ToolCall_"></a>
<a id="Vercel_AI_Provider_AssistantToolCallPart_ToolCall"></a>

### ToolCall


```csharp
public GeneratedToolCall ToolCall { get; }
```


The call.

**Value:** [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L873)
