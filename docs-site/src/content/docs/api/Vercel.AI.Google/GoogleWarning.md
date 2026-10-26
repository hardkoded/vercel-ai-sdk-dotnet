---
title: "GoogleWarning"
description: "A warning produced while preparing a Gemini request."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleWarning"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleWarning"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleWarning
```


A warning produced while preparing a Gemini request.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleWarning.cs#L10)

## Constructors

<a id="Vercel_AI_Google_GoogleWarning__ctor_"></a>
<a id="Vercel_AI_Google_GoogleWarning__ctor_System_String_System_String_System_String_System_String_"></a>

### GoogleWarning(string, string, string?, string?)


```csharp
public GoogleWarning(string type, string message, string? feature = null, string? details = null)
```


Creates a warning.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `feature` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `details` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleWarning.cs#L13)

## Properties

<a id="Vercel_AI_Google_GoogleWarning_Details_"></a>
<a id="Vercel_AI_Google_GoogleWarning_Details"></a>

### Details


```csharp
public string? Details { get; }
```


Extra detail.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleWarning.cs#L31)

<a id="Vercel_AI_Google_GoogleWarning_Feature_"></a>
<a id="Vercel_AI_Google_GoogleWarning_Feature"></a>

### Feature


```csharp
public string? Feature { get; }
```


Unsupported feature name, when the warning is about one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleWarning.cs#L28)

<a id="Vercel_AI_Google_GoogleWarning_Message_"></a>
<a id="Vercel_AI_Google_GoogleWarning_Message"></a>

### Message


```csharp
public string Message { get; }
```


Human-readable warning.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleWarning.cs#L25)

<a id="Vercel_AI_Google_GoogleWarning_Type_"></a>
<a id="Vercel_AI_Google_GoogleWarning_Type"></a>

### Type


```csharp
public string Type { get; }
```


Warning category: `unsupported`, `compatibility`, or `other`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleWarning.cs#L22)

## Methods

<a id="Vercel_AI_Google_GoogleWarning_Compatibility_"></a>
<a id="Vercel_AI_Google_GoogleWarning_Compatibility_System_String_System_String_"></a>

### Compatibility(string, string)


```csharp
public static GoogleWarning Compatibility(string feature, string details)
```


Compatibility mapping warning.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `feature` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `details` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [GoogleWarning](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleWarning.cs#L57)

<a id="Vercel_AI_Google_GoogleWarning_ToCallWarning_"></a>
<a id="Vercel_AI_Google_GoogleWarning_ToCallWarning"></a>

### ToCallWarning()


```csharp
public CallWarning ToCallWarning()
```


Maps this warning onto the shared call-warning type.

**Returns:** [CallWarning](/api/Vercel.AI.Provider/CallWarning/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleWarning.cs#L34)

<a id="Vercel_AI_Google_GoogleWarning_Unsupported_"></a>
<a id="Vercel_AI_Google_GoogleWarning_Unsupported_System_String_System_String_"></a>

### Unsupported(string, string?)


```csharp
public static GoogleWarning Unsupported(string feature, string? details = null)
```


Unsupported feature warning.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `feature` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `details` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [GoogleWarning](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleWarning.cs#L50)
