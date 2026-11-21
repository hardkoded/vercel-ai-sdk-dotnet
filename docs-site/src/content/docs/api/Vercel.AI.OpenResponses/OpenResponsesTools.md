---
title: "OpenResponsesTools"
description: "The provider tools of an Open Responses provider. Maps to createOpenResponsesTools."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenResponsesTools"
  hidden: true
---

<a id="Vercel_AI_OpenResponses_OpenResponsesTools"></a>

Namespace: [Vercel.AI.OpenResponses](/api/Vercel.AI.OpenResponses/)


```csharp
public sealed class OpenResponsesTools
```


The provider tools of an Open Responses provider. Maps to `createOpenResponsesTools`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesTools.cs#L49)

## Constructors

<a id="Vercel_AI_OpenResponses_OpenResponsesTools__ctor_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesTools__ctor_System_String_"></a>

### OpenResponsesTools(string)


```csharp
public OpenResponsesTools(string customToolId)
```


Creates the tools for a custom tool ID, which has the shape `<provider>.<tool>`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `customToolId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesTools.cs#L54)

## Methods

<a id="Vercel_AI_OpenResponses_OpenResponsesTools_CustomTool_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesTools_CustomTool_Vercel_AI_OpenResponses_OpenResponsesCustomToolOptions_"></a>

### CustomTool(OpenResponsesCustomToolOptions)


```csharp
public Tool CustomTool(OpenResponsesCustomToolOptions options)
```


Creates a caller-executed OpenAI-compatible custom tool. Custom tools accept a raw string, optionally constrained by a
grammar. Requires endpoint support.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenResponsesCustomToolOptions](/api/Vercel.AI.OpenResponses/OpenResponsesCustomToolOptions/) |  |

**Returns:** [Tool](/api/Vercel.AI/Tool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesTools.cs#L63)
