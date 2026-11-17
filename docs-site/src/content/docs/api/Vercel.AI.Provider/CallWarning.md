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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L140)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L143)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L153)

## Properties

<a id="Vercel_AI_Provider_CallWarning_Feature_"></a>
<a id="Vercel_AI_Provider_CallWarning_Feature"></a>

### Feature


```csharp
public string? Feature { get; }
```


The feature that is unsupported or used in compatibility mode, or &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt; when the provider did not set one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L175)

<a id="Vercel_AI_Provider_CallWarning_Message_"></a>
<a id="Vercel_AI_Provider_CallWarning_Message"></a>

### Message


```csharp
public string Message { get; }
```


Human-readable warning. For a feature warning, this is its details text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L178)

<a id="Vercel_AI_Provider_CallWarning_Setting_"></a>
<a id="Vercel_AI_Provider_CallWarning_Setting"></a>

### Setting


```csharp
public string? Setting { get; init; }
```


The deprecated setting of a `deprecated` warning, or &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt; for other warnings.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L172)

<a id="Vercel_AI_Provider_CallWarning_Type_"></a>
<a id="Vercel_AI_Provider_CallWarning_Type"></a>

### Type


```csharp
public string Type { get; }
```


Warning category, such as `unsupported`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L169)

## Methods

<a id="Vercel_AI_Provider_CallWarning_Deprecated_"></a>
<a id="Vercel_AI_Provider_CallWarning_Deprecated_System_String_System_String_"></a>

### Deprecated(string, string)


```csharp
public static CallWarning Deprecated(string setting, string message)
```


Creates a `deprecated` warning about a setting. Maps to `{ type: 'deprecated', setting, message }`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `setting` | [string](https://learn.microsoft.com/dotnet/api/system.string) | The deprecated setting. |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Human-readable warning. |

**Returns:** [CallWarning](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L163)
