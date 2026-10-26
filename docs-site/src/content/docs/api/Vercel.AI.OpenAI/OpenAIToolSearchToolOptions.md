---
title: "OpenAIToolSearchToolOptions"
description: "Options of ToolSearch(OpenAIToolSearchToolOptions)."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIToolSearchToolOptions"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIToolSearchToolOptions"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIToolSearchToolOptions
```


Options of [ToolSearch(OpenAIToolSearchToolOptions)](/api/Vercel.AI.OpenAI/OpenAITools/#Vercel_AI_OpenAI_OpenAITools_ToolSearch_Vercel_AI_OpenAI_OpenAIToolSearchToolOptions_).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L220)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIToolSearchToolOptions_Description_"></a>
<a id="Vercel_AI_OpenAI_OpenAIToolSearchToolOptions_Description"></a>

### Description


```csharp
public string? Description { get; set; }
```


Description of the tool.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L226)

<a id="Vercel_AI_OpenAI_OpenAIToolSearchToolOptions_Execution_"></a>
<a id="Vercel_AI_OpenAI_OpenAIToolSearchToolOptions_Execution"></a>

### Execution


```csharp
public string? Execution { get; set; }
```


Where the search runs: `server` or `client`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L223)

<a id="Vercel_AI_OpenAI_OpenAIToolSearchToolOptions_Parameters_"></a>
<a id="Vercel_AI_OpenAI_OpenAIToolSearchToolOptions_Parameters"></a>

### Parameters


```csharp
public JsonObject? Parameters { get; set; }
```


JSON schema of the search arguments.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L229)
