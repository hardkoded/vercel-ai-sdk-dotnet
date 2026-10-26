---
title: "PreparedTool"
description: "A tool after String)."
editUrl: false
prev: false
next: false
sidebar:
  label: "PreparedTool"
  hidden: true
---

<a id="Vercel_AI_Prompt_PreparedTool"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class PreparedTool
```


A tool after `String)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L64)

## Constructors

<a id="Vercel_AI_Prompt_PreparedTool__ctor_"></a>
<a id="Vercel_AI_Prompt_PreparedTool__ctor_System_String_System_String_"></a>

### PreparedTool(string, string)


```csharp
public PreparedTool(string type, string name)
```


Creates a prepared tool.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L67)

## Properties

<a id="Vercel_AI_Prompt_PreparedTool_Args_"></a>
<a id="Vercel_AI_Prompt_PreparedTool_Args"></a>

### Args


```csharp
public JsonElement? Args { get; set; }
```


Provider tool arguments.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L98)

<a id="Vercel_AI_Prompt_PreparedTool_Description_"></a>
<a id="Vercel_AI_Prompt_PreparedTool_Description"></a>

### Description


```csharp
public string? Description { get; set; }
```


Resolved description.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L80)

<a id="Vercel_AI_Prompt_PreparedTool_Id_"></a>
<a id="Vercel_AI_Prompt_PreparedTool_Id"></a>

### Id


```csharp
public string? Id { get; set; }
```


Provider tool id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L95)

<a id="Vercel_AI_Prompt_PreparedTool_InputExamples_"></a>
<a id="Vercel_AI_Prompt_PreparedTool_InputExamples"></a>

### InputExamples


```csharp
public JsonElement? InputExamples { get; set; }
```


Input examples.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L92)

<a id="Vercel_AI_Prompt_PreparedTool_InputSchema_"></a>
<a id="Vercel_AI_Prompt_PreparedTool_InputSchema"></a>

### InputSchema


```csharp
public JsonElement? InputSchema { get; set; }
```


Input schema for function tools.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L83)

<a id="Vercel_AI_Prompt_PreparedTool_Name_"></a>
<a id="Vercel_AI_Prompt_PreparedTool_Name"></a>

### Name


```csharp
public string Name { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L77)

<a id="Vercel_AI_Prompt_PreparedTool_ProviderOptions_"></a>
<a id="Vercel_AI_Prompt_PreparedTool_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Provider options.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L89)

<a id="Vercel_AI_Prompt_PreparedTool_Strict_"></a>
<a id="Vercel_AI_Prompt_PreparedTool_Strict"></a>

### Strict


```csharp
public bool? Strict { get; set; }
```


Strict mode.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L86)

<a id="Vercel_AI_Prompt_PreparedTool_Type_"></a>
<a id="Vercel_AI_Prompt_PreparedTool_Type"></a>

### Type


```csharp
public string Type { get; }
```


`function` or `provider`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L74)
