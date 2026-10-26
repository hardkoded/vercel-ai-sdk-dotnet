---
title: "StreamErrors"
description: "Normalizes provider stream failures. Maps to normalizeStreamProviderError."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamErrors"
  hidden: true
---

<a id="Vercel_AI_Prompt_StreamErrors"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public static class StreamErrors
```


Normalizes provider stream failures. Maps to `normalizeStreamProviderError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L88)

## Methods

<a id="Vercel_AI_Prompt_StreamErrors_NormalizeStreamProviderError_"></a>
<a id="Vercel_AI_Prompt_StreamErrors_NormalizeStreamProviderError_System_Object_"></a>

### NormalizeStreamProviderError(object?)


```csharp
public static object? NormalizeStreamProviderError(object? error)
```


Turns a provider payload into [StreamProviderError](/api/Vercel.AI.Prompt/StreamProviderError/).
Existing exceptions, preserved errors, and values without a string message are returned unchanged.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L94)
