---
title: "DeepSeekTools"
description: "DeepSeek server-side tools for the Responses API. Maps to deepseekTools."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekTools"
  hidden: true
---

<a id="Vercel_AI_DeepSeek_DeepSeekTools"></a>

Namespace: [Vercel.AI.DeepSeek](/api/Vercel.AI.DeepSeek/)


```csharp
public static class DeepSeekTools
```


DeepSeek server-side tools for the Responses API. Maps to `deepseekTools`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekTools.cs#L10)

## Fields

<a id="Vercel_AI_DeepSeek_DeepSeekTools_WebSearchToolId"></a>

### WebSearchToolId


```csharp
public const string WebSearchToolId = "deepseek.web_search"
```


Id of the [WebSearch(string)](#Vercel_AI_DeepSeek_DeepSeekTools_WebSearch_System_String_) tool.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekTools.cs#L13)

## Methods

<a id="Vercel_AI_DeepSeek_DeepSeekTools_WebSearch_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekTools_WebSearch_System_String_"></a>

### WebSearch(string)


```csharp
public static Tool WebSearch(string name = "web_search")
```


DeepSeek's server-side web search for the Responses API. DeepSeek runs the searches and page reads during
generation and adds the results to the model context. Supported with `deepseek-v4-pro`.
Successful searches and page reads are billed per operation and reported in `usage.raw.server_tool_use`
and in the `serverToolUse` provider metadata. Maps to `webSearch`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Name the tool goes by in the call and its result. |

**Returns:** [Tool](/api/Vercel.AI/Tool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekTools.cs#L22)
