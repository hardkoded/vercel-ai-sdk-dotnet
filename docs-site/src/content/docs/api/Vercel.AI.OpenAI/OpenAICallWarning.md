---
title: "OpenAICallWarning"
description: "A warning produced while preparing an OpenAI request."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICallWarning"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAICallWarning"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAICallWarning
```


A warning produced while preparing an OpenAI request.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAICallWarning.cs#L10)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAICallWarning__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAICallWarning__ctor_System_String_System_String_System_String_System_String_"></a>

### OpenAICallWarning(string, string?, string?, string?)


```csharp
public OpenAICallWarning(string type, string? feature, string? details, string? message = null)
```


Creates a warning. &lt;code class="paramref"&gt;details&lt;/code&gt; or &lt;code class="paramref"&gt;message&lt;/code&gt; is the text callers read.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `feature` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `details` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAICallWarning.cs#L13)

## Properties

<a id="Vercel_AI_OpenAI_OpenAICallWarning_Details_"></a>
<a id="Vercel_AI_OpenAI_OpenAICallWarning_Details"></a>

### Details


```csharp
public string? Details { get; }
```


Why the request was changed.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAICallWarning.cs#L28)

<a id="Vercel_AI_OpenAI_OpenAICallWarning_Feature_"></a>
<a id="Vercel_AI_OpenAI_OpenAICallWarning_Feature"></a>

### Feature


```csharp
public string? Feature { get; }
```


Feature the warning refers to.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAICallWarning.cs#L25)

<a id="Vercel_AI_OpenAI_OpenAICallWarning_Message_"></a>
<a id="Vercel_AI_OpenAI_OpenAICallWarning_Message"></a>

### Message


```csharp
public string? Message { get; }
```


Free-form message used by `other` warnings.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAICallWarning.cs#L31)

<a id="Vercel_AI_OpenAI_OpenAICallWarning_Type_"></a>
<a id="Vercel_AI_OpenAI_OpenAICallWarning_Type"></a>

### Type


```csharp
public string Type { get; }
```


Warning category, such as `unsupported` or `compatibility`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAICallWarning.cs#L22)

## Methods

<a id="Vercel_AI_OpenAI_OpenAICallWarning_ToCallWarning_"></a>
<a id="Vercel_AI_OpenAI_OpenAICallWarning_ToCallWarning"></a>

### ToCallWarning()


```csharp
public CallWarning ToCallWarning()
```


The warning text stored on language-model results.

**Returns:** [CallWarning](/api/Vercel.AI.Provider/CallWarning/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAICallWarning.cs#L34)
