---
title: "FingerprintTool"
description: "A tool definition pinned by FingerprintTool%7d)."
editUrl: false
prev: false
next: false
sidebar:
  label: "FingerprintTool"
  hidden: true
---

<a id="Vercel_AI_GenerateText_FingerprintTool"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class FingerprintTool
```


A tool definition pinned by `FingerprintTool%7d)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolFingerprint.cs#L12)

## Constructors

<a id="Vercel_AI_GenerateText_FingerprintTool__ctor_"></a>
<a id="Vercel_AI_GenerateText_FingerprintTool__ctor_System_Text_Json_JsonElement_System_String_System_Boolean_System_String_"></a>

### FingerprintTool(JsonElement, string?, bool, string?)


```csharp
public FingerprintTool(JsonElement inputSchema, string? description = null, bool descriptionIsFunction = false, string? title = null)
```


Creates a tool definition.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `inputSchema` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `descriptionIsFunction` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `title` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolFingerprint.cs#L15)

## Properties

<a id="Vercel_AI_GenerateText_FingerprintTool_Description_"></a>
<a id="Vercel_AI_GenerateText_FingerprintTool_Description"></a>

### Description


```csharp
public string? Description { get; }
```


String description. Ignored when [DescriptionIsFunction](#Vercel_AI_GenerateText_FingerprintTool_DescriptionIsFunction) is true.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolFingerprint.cs#L27)

<a id="Vercel_AI_GenerateText_FingerprintTool_DescriptionIsFunction_"></a>
<a id="Vercel_AI_GenerateText_FingerprintTool_DescriptionIsFunction"></a>

### DescriptionIsFunction


```csharp
public bool DescriptionIsFunction { get; }
```


True when the description is a function. Only the presence of a function is pinned.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolFingerprint.cs#L30)

<a id="Vercel_AI_GenerateText_FingerprintTool_InputSchema_"></a>
<a id="Vercel_AI_GenerateText_FingerprintTool_InputSchema"></a>

### InputSchema


```csharp
public JsonElement InputSchema { get; }
```


JSON Schema for the tool input.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolFingerprint.cs#L24)

<a id="Vercel_AI_GenerateText_FingerprintTool_Title_"></a>
<a id="Vercel_AI_GenerateText_FingerprintTool_Title"></a>

### Title


```csharp
public string? Title { get; }
```


Tool title.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolFingerprint.cs#L33)
