---
title: "OpenAITools"
description: "Factories for the OpenAI provider-defined tools, the port of upstream openaiTools. Each builds the LanguageModelTool that OpenAI Responses maps to a hosted t..."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAITools"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAITools"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public static class OpenAITools
```


Factories for the OpenAI provider-defined tools, the port of upstream `openaiTools`. Each builds the
[LanguageModelTool](/api/Vercel.AI.Provider/LanguageModelTool/) that OpenAI Responses maps to a hosted tool.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L370)

## Methods

<a id="Vercel_AI_OpenAI_OpenAITools_ApplyPatch_"></a>
<a id="Vercel_AI_OpenAI_OpenAITools_ApplyPatch_Vercel_AI_OpenAI_OpenAIApplyPatchToolOptions_"></a>

### ApplyPatch(OpenAIApplyPatchToolOptions?)


```csharp
public static Tool ApplyPatch(OpenAIApplyPatchToolOptions? options = null)
```


The apply patch tool (`openai.apply_patch`). The application executes it, so the options take the execute callback.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAIApplyPatchToolOptions](/api/Vercel.AI.OpenAI/OpenAIApplyPatchToolOptions/) |  |

**Returns:** [Tool](/api/Vercel.AI/Tool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L516)

<a id="Vercel_AI_OpenAI_OpenAITools_CodeInterpreter_"></a>
<a id="Vercel_AI_OpenAI_OpenAITools_CodeInterpreter_Vercel_AI_OpenAI_OpenAICodeInterpreterToolOptions_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L572)

<a id="Vercel_AI_OpenAI_OpenAITools_Computer_"></a>
<a id="Vercel_AI_OpenAI_OpenAITools_Computer"></a>

### Computer()


```csharp
public static LanguageModelTool Computer()
```


The computer tool (`openai.computer`).

**Returns:** [LanguageModelTool](/api/Vercel.AI.Provider/LanguageModelTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L533)

<a id="Vercel_AI_OpenAI_OpenAITools_CustomTool_"></a>
<a id="Vercel_AI_OpenAI_OpenAITools_CustomTool_Vercel_AI_OpenAI_OpenAICustomToolOptions_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L669)

<a id="Vercel_AI_OpenAI_OpenAITools_FileSearch_"></a>
<a id="Vercel_AI_OpenAI_OpenAITools_FileSearch_Vercel_AI_OpenAI_OpenAIFileSearchToolOptions_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L537)

<a id="Vercel_AI_OpenAI_OpenAITools_ImageGeneration_"></a>
<a id="Vercel_AI_OpenAI_OpenAITools_ImageGeneration_Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L373)

<a id="Vercel_AI_OpenAI_OpenAITools_LocalShell_"></a>
<a id="Vercel_AI_OpenAI_OpenAITools_LocalShell"></a>

### LocalShell()


```csharp
public static LanguageModelTool LocalShell()
```


The local shell tool (`openai.local_shell`).

**Returns:** [LanguageModelTool](/api/Vercel.AI.Provider/LanguageModelTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L512)

<a id="Vercel_AI_OpenAI_OpenAITools_Mcp_"></a>
<a id="Vercel_AI_OpenAI_OpenAITools_Mcp_Vercel_AI_OpenAI_OpenAIMcpToolOptions_"></a>

### Mcp(OpenAIMcpToolOptions)


```csharp
public static LanguageModelTool Mcp(OpenAIMcpToolOptions options)
```


The MCP tool (`openai.mcp`).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAIMcpToolOptions](/api/Vercel.AI.OpenAI/OpenAIMcpToolOptions/) |  |

**Returns:** [LanguageModelTool](/api/Vercel.AI.Provider/LanguageModelTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L446)

<a id="Vercel_AI_OpenAI_OpenAITools_ProgrammaticToolCalling_"></a>
<a id="Vercel_AI_OpenAI_OpenAITools_ProgrammaticToolCalling"></a>

### ProgrammaticToolCalling()


```csharp
public static LanguageModelTool ProgrammaticToolCalling()
```


The programmatic tool calling tool (`openai.programmatic_tool_calling`).

**Returns:** [LanguageModelTool](/api/Vercel.AI.Provider/LanguageModelTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L726)

<a id="Vercel_AI_OpenAI_OpenAITools_Shell_"></a>
<a id="Vercel_AI_OpenAI_OpenAITools_Shell_Vercel_AI_OpenAI_OpenAIShellToolOptions_"></a>

### Shell(OpenAIShellToolOptions?)


```csharp
public static Tool Shell(OpenAIShellToolOptions? options = null)
```


The shell tool (`openai.shell`). A local call is executed by the application, so the options take the execute callback.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAIShellToolOptions](/api/Vercel.AI.OpenAI/OpenAIShellToolOptions/) |  |

**Returns:** [Tool](/api/Vercel.AI/Tool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L604)

<a id="Vercel_AI_OpenAI_OpenAITools_ToolSearch_"></a>
<a id="Vercel_AI_OpenAI_OpenAITools_ToolSearch_Vercel_AI_OpenAI_OpenAIToolSearchToolOptions_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L692)

<a id="Vercel_AI_OpenAI_OpenAITools_WebSearch_"></a>
<a id="Vercel_AI_OpenAI_OpenAITools_WebSearch_Vercel_AI_OpenAI_OpenAIWebSearchToolOptions_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L412)

<a id="Vercel_AI_OpenAI_OpenAITools_WebSearchPreview_"></a>
<a id="Vercel_AI_OpenAI_OpenAITools_WebSearchPreview_Vercel_AI_OpenAI_OpenAIWebSearchPreviewToolOptions_"></a>

### WebSearchPreview(OpenAIWebSearchPreviewToolOptions?)


```csharp
public static LanguageModelTool WebSearchPreview(OpenAIWebSearchPreviewToolOptions? options = null)
```


The web search preview tool (`openai.web_search_preview`).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAIWebSearchPreviewToolOptions](/api/Vercel.AI.OpenAI/OpenAIWebSearchPreviewToolOptions/) |  |

**Returns:** [LanguageModelTool](/api/Vercel.AI.Provider/LanguageModelTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L706)
