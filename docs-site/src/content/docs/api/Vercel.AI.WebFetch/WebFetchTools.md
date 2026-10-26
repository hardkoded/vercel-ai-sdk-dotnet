---
title: "WebFetchTools"
description: "A provider-independent web fetch tool. Maps to @ai-sdk/tool-web-fetch."
editUrl: false
prev: false
next: false
sidebar:
  label: "WebFetchTools"
  hidden: true
---

<a id="Vercel_AI_WebFetch_WebFetchTools"></a>

Namespace: [Vercel.AI.WebFetch](/api/Vercel.AI.WebFetch/)


```csharp
public static class WebFetchTools
```


A provider-independent web fetch tool. Maps to `@ai-sdk/tool-web-fetch`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/WebFetch/WebFetchTools.cs#L47)

## Fields

<a id="Vercel_AI_WebFetch_WebFetchTools_ToolName"></a>

### ToolName


```csharp
public const string ToolName = "webFetch"
```


Name of the tool the model sees.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/WebFetch/WebFetchTools.cs#L50)

## Methods

<a id="Vercel_AI_WebFetch_WebFetchTools_Create_"></a>
<a id="Vercel_AI_WebFetch_WebFetchTools_Create_Vercel_AI_WebFetch_WebFetchToolOptions_"></a>

### Create(WebFetchToolOptions?)


```csharp
public static Tool Create(WebFetchToolOptions? options = null)
```


Creates a provider-independent web fetch tool.
The model calls the tool with a URL. HTML pages are converted to markdown with the main content extracted, and text
formats are returned as is. JavaScript is not executed. Failures are returned as results so the model can recover.
Requests are GET only, without cookies or credentials. Every hop, including redirects, is checked against the domain
filter and private addresses are blocked.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [WebFetchToolOptions](/api/Vercel.AI.WebFetch/WebFetchToolOptions/) |  |

**Returns:** [Tool](/api/Vercel.AI/Tool/)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidArgumentError](/api/Vercel.AI.Util/InvalidArgumentError/) | An option is invalid. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/WebFetch/WebFetchTools.cs#L83)
