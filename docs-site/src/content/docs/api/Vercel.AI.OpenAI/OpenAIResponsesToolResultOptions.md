---
title: "OpenAIResponsesToolResultOptions"
description: "The openai options of an ordinary tool result in Responses history. Maps to OpenAIResponsesToolResultOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIResponsesToolResultOptions"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIResponsesToolResultOptions"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIResponsesToolResultOptions
```


The `openai` options of an ordinary tool result in Responses history. Maps to
`OpenAIResponsesToolResultOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesToolResultOptions.cs#L14)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIResponsesToolResultOptions_AdditionalTools_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesToolResultOptions_AdditionalTools"></a>

### AdditionalTools


```csharp
public IReadOnlyList<OpenAIResponsesAdditionalTool>? AdditionalTools { get; set; }
```


Tool definitions to load immediately after this ordinary tool result.

**Value:** [IReadOnlyList&lt;OpenAIResponsesAdditionalTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesToolResultOptions.cs#L17)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIResponsesToolResultOptions_ToProviderMetadata_"></a>
<a id="Vercel_AI_OpenAI_OpenAIResponsesToolResultOptions_ToProviderMetadata_System_String_"></a>

### ToProviderMetadata(string)


```csharp
public JsonElement ToProviderMetadata(string providerOptionsName = "openai")
```


Returns the options as provider metadata under &lt;code class="paramref"&gt;providerOptionsName&lt;/code&gt; (`openai` by
default), ready for `ToolResultPart.ProviderMetadata`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptionsName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIResponsesToolResultOptions.cs#L23)
