---
title: "ContentToolModelOutput"
description: "Multi-part tool output."
editUrl: false
prev: false
next: false
sidebar:
  label: "ContentToolModelOutput"
  hidden: true
---

<a id="Vercel_AI_Prompt_ContentToolModelOutput"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class ContentToolModelOutput : ToolModelOutput
```


Multi-part tool output.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ToolModelOutput](/api/Vercel.AI.Prompt/ToolModelOutput/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L126)

## Constructors

<a id="Vercel_AI_Prompt_ContentToolModelOutput__ctor_"></a>
<a id="Vercel_AI_Prompt_ContentToolModelOutput__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Prompt_ToolModelContentPart__"></a>

### `ContentToolModelOutput(IReadOnlyList<ToolModelContentPart>)`


```csharp
public ContentToolModelOutput(IReadOnlyList<ToolModelContentPart> value)
```


Creates content output.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [IReadOnlyList&lt;ToolModelContentPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L129)

## Properties

<a id="Vercel_AI_Prompt_ContentToolModelOutput_Value_"></a>
<a id="Vercel_AI_Prompt_ContentToolModelOutput_Value"></a>

### Value


```csharp
public IReadOnlyList<ToolModelContentPart> Value { get; }
```


Content parts.

**Value:** [IReadOnlyList&lt;ToolModelContentPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L136)
