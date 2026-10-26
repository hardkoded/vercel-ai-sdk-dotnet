---
title: "GoogleModelCapabilities"
description: "Which Gemini request features a model id accepts."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleModelCapabilities"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleModelCapabilities"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleModelCapabilities
```


Which Gemini request features a model id accepts.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleModelCapabilities.cs#L10)

## Constructors

<a id="Vercel_AI_Google_GoogleModelCapabilities__ctor_"></a>
<a id="Vercel_AI_Google_GoogleModelCapabilities__ctor_System_Boolean_System_Boolean_System_Boolean_"></a>

### GoogleModelCapabilities(bool, bool, bool)


```csharp
public GoogleModelCapabilities(bool supportsGemini2Tools, bool supportsFileSearch, bool usesGemini3Features)
```


Creates a capability set.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `supportsGemini2Tools` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `supportsFileSearch` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `usesGemini3Features` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleModelCapabilities.cs#L13)

## Properties

<a id="Vercel_AI_Google_GoogleModelCapabilities_SupportsFileSearch_"></a>
<a id="Vercel_AI_Google_GoogleModelCapabilities_SupportsFileSearch"></a>

### SupportsFileSearch


```csharp
public bool SupportsFileSearch { get; }
```


File Search tool.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleModelCapabilities.cs#L24)

<a id="Vercel_AI_Google_GoogleModelCapabilities_SupportsGemini2Tools_"></a>
<a id="Vercel_AI_Google_GoogleModelCapabilities_SupportsGemini2Tools"></a>

### SupportsGemini2Tools


```csharp
public bool SupportsGemini2Tools { get; }
```


Google Search, URL context, code execution, and similar tools.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleModelCapabilities.cs#L21)

<a id="Vercel_AI_Google_GoogleModelCapabilities_UsesGemini3Features_"></a>
<a id="Vercel_AI_Google_GoogleModelCapabilities_UsesGemini3Features"></a>

### UsesGemini3Features


```csharp
public bool UsesGemini3Features { get; }
```


Gemini 3 request shape, including combined tools and thought-signature handling.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleModelCapabilities.cs#L27)
