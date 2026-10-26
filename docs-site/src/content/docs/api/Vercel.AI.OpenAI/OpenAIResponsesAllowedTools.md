---
title: "OpenAIResponsesAllowedTools"
description: "The tools a request may call, sent as toolchoice: { type: \"allowedtools\" }. Maps to the allowedTools option of the OpenAI Responses provider options."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIResponsesAllowedTools"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIResponsesAllowedTools"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIResponsesAllowedTools
```


The tools a request may call, sent as `tool_choice: { type: "allowed_tools" }`. Maps to the
`allowedTools` option of the OpenAI Responses provider options.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesPrepareTools.cs#L17)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIResponsesAllowedTools__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesAllowedTools__ctor_System_Collections_Generic_IReadOnlyList_System_String__System_String_"></a>

### `OpenAIResponsesAllowedTools(IReadOnlyList<string>, string?)`


```csharp
public OpenAIResponsesAllowedTools(IReadOnlyList<string> toolNames, string? mode = null)
```


Creates the allow-list.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolNames` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) | Names of the tools the model may call. Not empty. |
| `mode` | [string](https://learn.microsoft.com/dotnet/api/system.string) | `auto` (default) or `required`. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesPrepareTools.cs#L22)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIResponsesAllowedTools_Mode_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesAllowedTools_Mode"></a>

### Mode


```csharp
public string? Mode { get; }
```


`auto` or `required`. Null means `auto`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesPrepareTools.cs#L32)

<a id="Vercel_AI_OpenAI_OpenAIResponsesAllowedTools_ToolNames_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesAllowedTools_ToolNames"></a>

### ToolNames


```csharp
public IReadOnlyList<string> ToolNames { get; }
```


Names of the tools the model may call.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesPrepareTools.cs#L29)
