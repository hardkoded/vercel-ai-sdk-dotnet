---
title: "OpenAIChatTools"
description: "Prepares Chat Completions function tools."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIChatTools"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIChatTools"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public static class OpenAIChatTools
```


Prepares Chat Completions function tools.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatTools.cs#L48)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIChatTools_Prepare_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatTools_Prepare_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_LanguageModelTool__System_Collections_Generic_IReadOnlyList_Vercel_AI_OpenAI_OpenAIUnsupportedTool__Vercel_AI_Provider_ToolChoice_"></a>

### `Prepare(IReadOnlyList<LanguageModelTool>?, IReadOnlyList<OpenAIUnsupportedTool>?, ToolChoice?)`


```csharp
public static OpenAIChatToolPreparation Prepare(IReadOnlyList<LanguageModelTool>? tools, IReadOnlyList<OpenAIUnsupportedTool>? unsupported, ToolChoice? toolChoice)
```


Prepares function tools and records unsupported tool types.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tools` | [IReadOnlyList&lt;LanguageModelTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `unsupported` | [IReadOnlyList&lt;OpenAIUnsupportedTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `toolChoice` | [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/) |  |

**Returns:** [OpenAIChatToolPreparation](/api/Vercel.AI.OpenAI/OpenAIChatToolPreparation/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatTools.cs#L57)

<a id="Vercel_AI_OpenAI_OpenAIChatTools_Prepare_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_LanguageModelTool__Vercel_AI_Provider_ToolChoice_"></a>

### `Prepare(IReadOnlyList<LanguageModelTool>?, ToolChoice?)`


```csharp
public static OpenAIChatToolPreparation Prepare(IReadOnlyList<LanguageModelTool>? tools, ToolChoice? toolChoice)
```


Prepares function tools. Strict is omitted when [Strict](/api/Vercel.AI.Provider/LanguageModelTool/#Vercel_AI_Provider_LanguageModelTool_Strict) is null.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tools` | [IReadOnlyList&lt;LanguageModelTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `toolChoice` | [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/) |  |

**Returns:** [OpenAIChatToolPreparation](/api/Vercel.AI.OpenAI/OpenAIChatToolPreparation/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatTools.cs#L51)
