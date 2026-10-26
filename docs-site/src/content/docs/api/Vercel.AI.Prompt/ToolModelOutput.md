---
title: "ToolModelOutput"
description: "A tool result sent back to the model. Maps to ToolResultOutput."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolModelOutput"
  hidden: true
---

<a id="Vercel_AI_Prompt_ToolModelOutput"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public abstract class ToolModelOutput
```


A tool result sent back to the model. Maps to `ToolResultOutput`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [ContentToolModelOutput](/api/Vercel.AI.Prompt/ContentToolModelOutput/), [ErrorJsonToolModelOutput](/api/Vercel.AI.Prompt/ErrorJsonToolModelOutput/), [ErrorTextToolModelOutput](/api/Vercel.AI.Prompt/ErrorTextToolModelOutput/), [JsonToolModelOutput](/api/Vercel.AI.Prompt/JsonToolModelOutput/), [TextToolModelOutput](/api/Vercel.AI.Prompt/TextToolModelOutput/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L40)

## Constructors

<a id="Vercel_AI_Prompt_ToolModelOutput__ctor_"></a>
<a id="Vercel_AI_Prompt_ToolModelOutput__ctor_System_String_"></a>

### ToolModelOutput(string)


```csharp
protected ToolModelOutput(string type)
```


Creates an output.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L43)

## Properties

<a id="Vercel_AI_Prompt_ToolModelOutput_Type_"></a>
<a id="Vercel_AI_Prompt_ToolModelOutput_Type"></a>

### Type


```csharp
public string Type { get; }
```


Output type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L49)
