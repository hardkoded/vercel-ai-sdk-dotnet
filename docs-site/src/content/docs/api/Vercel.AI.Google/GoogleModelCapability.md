---
title: "GoogleModelCapability"
description: "Classifies Gemini model ids. Unknown future Gemini ids inherit the newest behavior."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleModelCapability"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleModelCapability"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleModelCapability
```


Classifies Gemini model ids. Unknown future Gemini ids inherit the newest behavior.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleModelCapabilities.cs#L31)

## Methods

<a id="Vercel_AI_Google_GoogleModelCapability_Get_"></a>
<a id="Vercel_AI_Google_GoogleModelCapability_Get_System_String_"></a>

### Get(string)


```csharp
public static GoogleModelCapabilities Get(string modelId)
```


Classifies &lt;code class="paramref"&gt;modelId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [GoogleModelCapabilities](/api/Vercel.AI.Google/GoogleModelCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleModelCapabilities.cs#L41)

<a id="Vercel_AI_Google_GoogleModelCapability_IsGemma_"></a>
<a id="Vercel_AI_Google_GoogleModelCapability_IsGemma_System_String_"></a>

### IsGemma(string)


```csharp
public static bool IsGemma(string modelId)
```


True when the id is a Gemma model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleModelCapabilities.cs#L53)
