---
title: "AzureOpenAITools"
description: "Factories for the provider-defined tools Azure OpenAI Responses supports, the port of upstream azureOpenaiTools. They are the OpenAI factories of OpenAITools..."
editUrl: false
prev: false
next: false
sidebar:
  label: "AzureOpenAITools"
  hidden: true
---

<a id="Vercel_AI_Azure_AzureOpenAITools"></a>

Namespace: [Vercel.AI.Azure](/api/Vercel.AI.Azure/)


```csharp
public static class AzureOpenAITools
```


Factories for the provider-defined tools Azure OpenAI Responses supports, the port of upstream `azureOpenaiTools`.
They are the OpenAI factories of [OpenAITools](/api/Vercel.AI.OpenAI/OpenAITools/): Azure sends the same tool definitions and reads the item
metadata under the `azure` provider key.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAITools.cs#L15)

## Methods

<a id="Vercel_AI_Azure_AzureOpenAITools_ApplyPatch_"></a>
<a id="Vercel_AI_Azure_AzureOpenAITools_ApplyPatch_Vercel_AI_OpenAI_OpenAIApplyPatchToolOptions_"></a>

### ApplyPatch(OpenAIApplyPatchToolOptions?)


```csharp
public static Tool ApplyPatch(OpenAIApplyPatchToolOptions? options = null)
```


The apply patch tool (`openai.apply_patch`).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAIApplyPatchToolOptions](/api/Vercel.AI.OpenAI/OpenAIApplyPatchToolOptions/) |  |

**Returns:** [Tool](/api/Vercel.AI/Tool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAITools.cs#L18)

<a id="Vercel_AI_Azure_AzureOpenAITools_CodeInterpreter_"></a>
<a id="Vercel_AI_Azure_AzureOpenAITools_CodeInterpreter_Vercel_AI_OpenAI_OpenAICodeInterpreterToolOptions_"></a>

### CodeInterpreter(OpenAICodeInterpreterToolOptions?)


```csharp
public static LanguageModelTool CodeInterpreter(OpenAICodeInterpreterToolOptions? options = null)
```


The code interpreter tool (`openai.code_interpreter`).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICodeInterpreterToolOptions](/api/Vercel.AI.OpenAI/OpenAICodeInterpreterToolOptions/) |  |

**Returns:** [LanguageModelTool](/api/Vercel.AI.Provider/LanguageModelTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAITools.cs#L22)

<a id="Vercel_AI_Azure_AzureOpenAITools_CustomTool_"></a>
<a id="Vercel_AI_Azure_AzureOpenAITools_CustomTool_Vercel_AI_OpenAI_OpenAICustomToolOptions_"></a>

### CustomTool(OpenAICustomToolOptions)


```csharp
public static LanguageModelTool CustomTool(OpenAICustomToolOptions options)
```


A custom tool (`openai.custom`) with the name given in &lt;code class="paramref"&gt;options&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICustomToolOptions](/api/Vercel.AI.OpenAI/OpenAICustomToolOptions/) |  |

**Returns:** [LanguageModelTool](/api/Vercel.AI.Provider/LanguageModelTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAITools.cs#L27)

<a id="Vercel_AI_Azure_AzureOpenAITools_FileSearch_"></a>
<a id="Vercel_AI_Azure_AzureOpenAITools_FileSearch_Vercel_AI_OpenAI_OpenAIFileSearchToolOptions_"></a>

### FileSearch(OpenAIFileSearchToolOptions)


```csharp
public static LanguageModelTool FileSearch(OpenAIFileSearchToolOptions options)
```


The file search tool (`openai.file_search`).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAIFileSearchToolOptions](/api/Vercel.AI.OpenAI/OpenAIFileSearchToolOptions/) |  |

**Returns:** [LanguageModelTool](/api/Vercel.AI.Provider/LanguageModelTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAITools.cs#L31)

<a id="Vercel_AI_Azure_AzureOpenAITools_ImageGeneration_"></a>
<a id="Vercel_AI_Azure_AzureOpenAITools_ImageGeneration_Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_"></a>

### ImageGeneration(OpenAIImageGenerationToolOptions?)


```csharp
public static LanguageModelTool ImageGeneration(OpenAIImageGenerationToolOptions? options = null)
```


The image generation tool (`openai.image_generation`).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAIImageGenerationToolOptions](/api/Vercel.AI.OpenAI/OpenAIImageGenerationToolOptions/) |  |

**Returns:** [LanguageModelTool](/api/Vercel.AI.Provider/LanguageModelTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAITools.cs#L35)

<a id="Vercel_AI_Azure_AzureOpenAITools_Shell_"></a>
<a id="Vercel_AI_Azure_AzureOpenAITools_Shell_Vercel_AI_OpenAI_OpenAIShellToolOptions_"></a>

### Shell(OpenAIShellToolOptions?)


```csharp
public static Tool Shell(OpenAIShellToolOptions? options = null)
```


The shell tool (`openai.shell`).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAIShellToolOptions](/api/Vercel.AI.OpenAI/OpenAIShellToolOptions/) |  |

**Returns:** [Tool](/api/Vercel.AI/Tool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAITools.cs#L40)

<a id="Vercel_AI_Azure_AzureOpenAITools_ToolSearch_"></a>
<a id="Vercel_AI_Azure_AzureOpenAITools_ToolSearch_Vercel_AI_OpenAI_OpenAIToolSearchToolOptions_"></a>

### ToolSearch(OpenAIToolSearchToolOptions?)


```csharp
public static LanguageModelTool ToolSearch(OpenAIToolSearchToolOptions? options = null)
```


The tool search tool (`openai.tool_search`).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAIToolSearchToolOptions](/api/Vercel.AI.OpenAI/OpenAIToolSearchToolOptions/) |  |

**Returns:** [LanguageModelTool](/api/Vercel.AI.Provider/LanguageModelTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAITools.cs#L43)

<a id="Vercel_AI_Azure_AzureOpenAITools_WebSearch_"></a>
<a id="Vercel_AI_Azure_AzureOpenAITools_WebSearch_Vercel_AI_OpenAI_OpenAIWebSearchToolOptions_"></a>

### WebSearch(OpenAIWebSearchToolOptions?)


```csharp
public static LanguageModelTool WebSearch(OpenAIWebSearchToolOptions? options = null)
```


The web search tool (`openai.web_search`).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAIWebSearchToolOptions](/api/Vercel.AI.OpenAI/OpenAIWebSearchToolOptions/) |  |

**Returns:** [LanguageModelTool](/api/Vercel.AI.Provider/LanguageModelTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAITools.cs#L47)

<a id="Vercel_AI_Azure_AzureOpenAITools_WebSearchPreview_"></a>
<a id="Vercel_AI_Azure_AzureOpenAITools_WebSearchPreview_Vercel_AI_OpenAI_OpenAIWebSearchPreviewToolOptions_"></a>

### WebSearchPreview(OpenAIWebSearchPreviewToolOptions?)


```csharp
[Obsolete("Use WebSearch instead.")]
public static LanguageModelTool WebSearchPreview(OpenAIWebSearchPreviewToolOptions? options = null)
```


The web search preview tool (`openai.web_search_preview`).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAIWebSearchPreviewToolOptions](/api/Vercel.AI.OpenAI/OpenAIWebSearchPreviewToolOptions/) |  |

**Returns:** [LanguageModelTool](/api/Vercel.AI.Provider/LanguageModelTool/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureOpenAITools.cs#L51)
