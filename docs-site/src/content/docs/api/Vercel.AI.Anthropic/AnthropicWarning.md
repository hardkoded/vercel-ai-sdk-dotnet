---
title: "AnthropicWarning"
description: "A warning produced while preparing an Anthropic request."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicWarning"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicWarning"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicWarning
```


A warning produced while preparing an Anthropic request.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicWarning.cs#L8)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicWarning__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicWarning__ctor_System_String_System_String_System_String_System_String_"></a>

### AnthropicWarning(string, string?, string?, string?)


```csharp
public AnthropicWarning(string type, string? feature = null, string? details = null, string? message = null)
```


Creates a warning.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `feature` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `details` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicWarning.cs#L11)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicWarning_Details_"></a>
<a id="Vercel_AI_Anthropic_AnthropicWarning_Details"></a>

### Details


```csharp
public string? Details { get; }
```


Extra detail.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicWarning.cs#L26)

<a id="Vercel_AI_Anthropic_AnthropicWarning_Feature_"></a>
<a id="Vercel_AI_Anthropic_AnthropicWarning_Feature"></a>

### Feature


```csharp
public string? Feature { get; }
```


Feature the warning refers to.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicWarning.cs#L23)

<a id="Vercel_AI_Anthropic_AnthropicWarning_Message_"></a>
<a id="Vercel_AI_Anthropic_AnthropicWarning_Message"></a>

### Message


```csharp
public string? Message { get; }
```


Free-form message used by `other` warnings.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicWarning.cs#L29)

<a id="Vercel_AI_Anthropic_AnthropicWarning_Type_"></a>
<a id="Vercel_AI_Anthropic_AnthropicWarning_Type"></a>

### Type


```csharp
public string Type { get; }
```


Warning category, such as `unsupported` or `compatibility`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicWarning.cs#L20)
