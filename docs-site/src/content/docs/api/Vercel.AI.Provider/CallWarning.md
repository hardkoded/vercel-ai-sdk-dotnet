---
title: "CallWarning"
description: "A warning produced while calling a model."
editUrl: false
prev: false
next: false
sidebar:
  label: "CallWarning"
  hidden: true
---

<a id="Vercel_AI_Provider_CallWarning"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class CallWarning
```


A warning produced while calling a model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L138)

## Constructors

<a id="Vercel_AI_Provider_CallWarning__ctor_"></a>
<a id="Vercel_AI_Provider_CallWarning__ctor_System_String_System_String_"></a>

### CallWarning(string, string)


```csharp
public CallWarning(string type, string message)
```


Creates a warning.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L141)

<a id="Vercel_AI_Provider_CallWarning__ctor_System_String_System_String_System_String_"></a>

### CallWarning(string, string, string?)


```csharp
public CallWarning(string type, string feature, string? details)
```


Creates an `unsupported` or `compatibility` warning about a feature.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Warning category, such as `unsupported`. |
| `feature` | [string](https://learn.microsoft.com/dotnet/api/system.string) | The feature the warning is about. |
| `details` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Additional details. When omitted, [Message](#Vercel_AI_Provider_CallWarning_Message) is the feature name. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L151)

## Properties

<a id="Vercel_AI_Provider_CallWarning_Feature_"></a>
<a id="Vercel_AI_Provider_CallWarning_Feature"></a>

### Feature


```csharp
public string? Feature { get; }
```


The feature that is unsupported or used in compatibility mode, or &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt; when the provider did not set one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L162)

<a id="Vercel_AI_Provider_CallWarning_Message_"></a>
<a id="Vercel_AI_Provider_CallWarning_Message"></a>

### Message


```csharp
public string Message { get; }
```


Human-readable warning. For a feature warning, this is its details text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L165)

<a id="Vercel_AI_Provider_CallWarning_Type_"></a>
<a id="Vercel_AI_Provider_CallWarning_Type"></a>

### Type


```csharp
public string Type { get; }
```


Warning category, such as `unsupported`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L159)
