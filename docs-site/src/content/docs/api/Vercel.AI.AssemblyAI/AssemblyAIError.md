---
title: "AssemblyAIError"
description: "Parses AssemblyAI error JSON."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAIError"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAIError"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public static class AssemblyAIError
```


Parses AssemblyAI error JSON.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L76)

## Methods

<a id="Vercel_AI_AssemblyAI_AssemblyAIError_Parse_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIError_Parse_System_String_"></a>

### Parse(string?)


```csharp
public static AssemblyAIErrorParseResult Parse(string? json)
```


Parses an error body in the flat or the nested shape.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `json` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [AssemblyAIErrorParseResult](/api/Vercel.AI.AssemblyAI/AssemblyAIErrorParseResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L79)

<a id="Vercel_AI_AssemblyAI_AssemblyAIError_ToMessage_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIError_ToMessage_Vercel_AI_AssemblyAI_AssemblyAIErrorData_"></a>

### ToMessage(AssemblyAIErrorData)


```csharp
public static string ToMessage(AssemblyAIErrorData data)
```


Maps parsed error data to the most informative message: the flat `error` string, or the nested `error.message`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [AssemblyAIErrorData](/api/Vercel.AI.AssemblyAI/AssemblyAIErrorData/) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L117)
