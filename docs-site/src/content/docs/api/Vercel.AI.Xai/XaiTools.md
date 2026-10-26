---
title: "XaiTools"
description: "Factories for the xAI provider-defined tools. Each returns a Tool that xAI runs. Maps to xai.tools. The name is the name the model and the results use. It de..."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiTools"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiTools"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public static class XaiTools
```


Factories for the xAI provider-defined tools. Each returns a [Tool](/api/Vercel.AI/Tool/) that xAI runs. Maps to `xai.tools`.
The `name` is the name the model and the results use. It defaults to the tool's wire name.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L116)

## Methods

<a id="Vercel_AI_Xai_XaiTools_CodeExecution_"></a>
<a id="Vercel_AI_Xai_XaiTools_CodeExecution_System_String_"></a>

### CodeExecution(string)


```csharp
public static Tool CodeExecution(string name = "code_execution")
```


The `xai.code_execution` tool. Maps to `codeExecution`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Tool](/api/Vercel.AI/Tool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L119)

<a id="Vercel_AI_Xai_XaiTools_FileSearch_"></a>
<a id="Vercel_AI_Xai_XaiTools_FileSearch_Vercel_AI_Xai_XaiFileSearchOptions_System_String_"></a>

### FileSearch(XaiFileSearchOptions, string)


```csharp
public static Tool FileSearch(XaiFileSearchOptions options, string name = "file_search")
```


The `xai.file_search` tool. Maps to `fileSearch`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [XaiFileSearchOptions](/api/Vercel.AI.Xai/XaiFileSearchOptions/) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Tool](/api/Vercel.AI/Tool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L125)

<a id="Vercel_AI_Xai_XaiTools_ImageGeneration_"></a>
<a id="Vercel_AI_Xai_XaiTools_ImageGeneration_Vercel_AI_Xai_XaiImageGenerationOptions_System_String_"></a>

### ImageGeneration(XaiImageGenerationOptions?, string)


```csharp
public static Tool ImageGeneration(XaiImageGenerationOptions? options = null, string name = "image_generation")
```


The `xai.image_generation` tool. Maps to `imageGeneration`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [XaiImageGenerationOptions](/api/Vercel.AI.Xai/XaiImageGenerationOptions/) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Tool](/api/Vercel.AI/Tool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L138)

<a id="Vercel_AI_Xai_XaiTools_McpServer_"></a>
<a id="Vercel_AI_Xai_XaiTools_McpServer_Vercel_AI_Xai_XaiMcpServerOptions_System_String_"></a>

### McpServer(XaiMcpServerOptions, string)


```csharp
public static Tool McpServer(XaiMcpServerOptions options, string name = "mcp")
```


The `xai.mcp` tool. Maps to `mcpServer`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [XaiMcpServerOptions](/api/Vercel.AI.Xai/XaiMcpServerOptions/) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Tool](/api/Vercel.AI/Tool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L151)

<a id="Vercel_AI_Xai_XaiTools_ViewImage_"></a>
<a id="Vercel_AI_Xai_XaiTools_ViewImage_System_String_"></a>

### ViewImage(string)


```csharp
public static Tool ViewImage(string name = "view_image")
```


The `xai.view_image` tool. Maps to `viewImage`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Tool](/api/Vercel.AI/Tool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L180)

<a id="Vercel_AI_Xai_XaiTools_ViewXVideo_"></a>
<a id="Vercel_AI_Xai_XaiTools_ViewXVideo_System_String_"></a>

### ViewXVideo(string)


```csharp
public static Tool ViewXVideo(string name = "view_x_video")
```


The `xai.view_x_video` tool. Maps to `viewXVideo`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Tool](/api/Vercel.AI/Tool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L186)

<a id="Vercel_AI_Xai_XaiTools_WebSearch_"></a>
<a id="Vercel_AI_Xai_XaiTools_WebSearch_Vercel_AI_Xai_XaiWebSearchOptions_System_String_"></a>

### WebSearch(XaiWebSearchOptions?, string)


```csharp
public static Tool WebSearch(XaiWebSearchOptions? options = null, string name = "web_search")
```


The `xai.web_search` tool. Maps to `webSearch`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [XaiWebSearchOptions](/api/Vercel.AI.Xai/XaiWebSearchOptions/) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Tool](/api/Vercel.AI/Tool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L192)

<a id="Vercel_AI_Xai_XaiTools_XSearch_"></a>
<a id="Vercel_AI_Xai_XaiTools_XSearch_Vercel_AI_Xai_XaiXSearchOptions_System_String_"></a>

### XSearch(XaiXSearchOptions?, string)


```csharp
public static Tool XSearch(XaiXSearchOptions? options = null, string name = "x_search")
```


The `xai.x_search` tool. Maps to `xSearch`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [XaiXSearchOptions](/api/Vercel.AI.Xai/XaiXSearchOptions/) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Tool](/api/Vercel.AI/Tool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L202)
