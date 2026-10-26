---
title: "PromptTool"
description: "A tool passed to String)."
editUrl: false
prev: false
next: false
sidebar:
  label: "PromptTool"
  hidden: true
---

<a id="Vercel_AI_Prompt_PromptTool"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class PromptTool
```


A tool passed to `String)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L27)

## Constructors

<a id="Vercel_AI_Prompt_PromptTool__ctor_"></a>
<a id="Vercel_AI_Prompt_PromptTool__ctor_System_String_"></a>

### PromptTool(string?)


```csharp
public PromptTool(string? type = null)
```


Creates a function, dynamic, or provider tool.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L30)

## Properties

<a id="Vercel_AI_Prompt_PromptTool_Args_"></a>
<a id="Vercel_AI_Prompt_PromptTool_Args"></a>

### Args


```csharp
public JsonElement? Args { get; set; }
```


Provider tool arguments.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L60)

<a id="Vercel_AI_Prompt_PromptTool_Description_"></a>
<a id="Vercel_AI_Prompt_PromptTool_Description"></a>

### Description


```csharp
public string? Description { get; set; }
```


String description.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L39)

<a id="Vercel_AI_Prompt_PromptTool_DescriptionFactory_"></a>
<a id="Vercel_AI_Prompt_PromptTool_DescriptionFactory"></a>

### DescriptionFactory


```csharp
public Func<ToolDescriptionContext, string>? DescriptionFactory { get; set; }
```


Description resolved from context and the sandbox.

**Value:** [Func&lt;ToolDescriptionContext, string&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L42)

<a id="Vercel_AI_Prompt_PromptTool_Id_"></a>
<a id="Vercel_AI_Prompt_PromptTool_Id"></a>

### Id


```csharp
public string? Id { get; set; }
```


Provider tool id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L57)

<a id="Vercel_AI_Prompt_PromptTool_InputExamples_"></a>
<a id="Vercel_AI_Prompt_PromptTool_InputExamples"></a>

### InputExamples


```csharp
public JsonElement? InputExamples { get; set; }
```


Input examples.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L54)

<a id="Vercel_AI_Prompt_PromptTool_InputSchema_"></a>
<a id="Vercel_AI_Prompt_PromptTool_InputSchema"></a>

### InputSchema


```csharp
public JsonElement InputSchema { get; set; }
```


JSON Schema for function tools.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L45)

<a id="Vercel_AI_Prompt_PromptTool_ProviderOptions_"></a>
<a id="Vercel_AI_Prompt_PromptTool_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Provider options.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L51)

<a id="Vercel_AI_Prompt_PromptTool_Strict_"></a>
<a id="Vercel_AI_Prompt_PromptTool_Strict"></a>

### Strict


```csharp
public bool? Strict { get; set; }
```


Strict mode.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L48)

<a id="Vercel_AI_Prompt_PromptTool_Type_"></a>
<a id="Vercel_AI_Prompt_PromptTool_Type"></a>

### Type


```csharp
public string? Type { get; }
```


Tool type. Null, `function`, and `dynamic` are function tools. `provider` is provider-defined.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L36)
