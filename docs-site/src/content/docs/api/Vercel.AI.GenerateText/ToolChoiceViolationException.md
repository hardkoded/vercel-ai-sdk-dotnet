---
title: "ToolChoiceViolationException"
description: "The model response did not satisfy toolChoice. Maps to ToolChoiceViolationError."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolChoiceViolationException"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ToolChoiceViolationException"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class ToolChoiceViolationException : AiSdkException, ISerializable
```


The model response did not satisfy `toolChoice`. Maps to `ToolChoiceViolationError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolChoiceViolationException.cs#L10)

## Constructors

<a id="Vercel_AI_GenerateText_ToolChoiceViolationException__ctor_"></a>
<a id="Vercel_AI_GenerateText_ToolChoiceViolationException__ctor_System_String_Vercel_AI_Provider_ToolChoice_Vercel_AI_Provider_FinishReason_System_String_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_GeneratedContent__"></a>

### `ToolChoiceViolationException(string, ToolChoice, FinishReason, string, string, IReadOnlyList<GeneratedContent>)`


```csharp
public ToolChoiceViolationException(string message, ToolChoice toolChoice, FinishReason finishReason, string provider, string modelId, IReadOnlyList<GeneratedContent> content)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolChoice` | [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/) |  |
| `finishReason` | [FinishReason](/api/Vercel.AI.Provider/FinishReason/) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `content` | [IReadOnlyList&lt;GeneratedContent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolChoiceViolationException.cs#L13)

## Properties

<a id="Vercel_AI_GenerateText_ToolChoiceViolationException_Content_"></a>
<a id="Vercel_AI_GenerateText_ToolChoiceViolationException_Content"></a>

### Content


```csharp
public IReadOnlyList<GeneratedContent> Content { get; }
```


Content from the rejected model call.

**Value:** [IReadOnlyList&lt;GeneratedContent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolChoiceViolationException.cs#L45)

<a id="Vercel_AI_GenerateText_ToolChoiceViolationException_FinishReason_"></a>
<a id="Vercel_AI_GenerateText_ToolChoiceViolationException_FinishReason"></a>

### FinishReason


```csharp
public FinishReason FinishReason { get; }
```


Finish reason from the model call.

**Value:** [FinishReason](/api/Vercel.AI.Provider/FinishReason/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolChoiceViolationException.cs#L36)

<a id="Vercel_AI_GenerateText_ToolChoiceViolationException_ModelId_"></a>
<a id="Vercel_AI_GenerateText_ToolChoiceViolationException_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolChoiceViolationException.cs#L42)

<a id="Vercel_AI_GenerateText_ToolChoiceViolationException_Name_"></a>
<a id="Vercel_AI_GenerateText_ToolChoiceViolationException_Name"></a>

### Name


```csharp
public string Name { get; }
```


Stable error name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolChoiceViolationException.cs#L30)

<a id="Vercel_AI_GenerateText_ToolChoiceViolationException_Provider_"></a>
<a id="Vercel_AI_GenerateText_ToolChoiceViolationException_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Model provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolChoiceViolationException.cs#L39)

<a id="Vercel_AI_GenerateText_ToolChoiceViolationException_ToolChoice_"></a>
<a id="Vercel_AI_GenerateText_ToolChoiceViolationException_ToolChoice"></a>

### ToolChoice


```csharp
public ToolChoice ToolChoice { get; }
```


Tool choice that was violated.

**Value:** [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolChoiceViolationException.cs#L33)
