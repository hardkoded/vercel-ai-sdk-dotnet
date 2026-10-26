---
title: "LanguageModelMiddlewareExtensions"
description: "Wraps a language model with middleware. Maps to wrapLanguageModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "LanguageModelMiddlewareExtensions"
  hidden: true
---

<a id="Vercel_AI_LanguageModelMiddlewareExtensions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public static class LanguageModelMiddlewareExtensions
```


Wraps a language model with middleware. Maps to `wrapLanguageModel`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L52)

## Methods

<a id="Vercel_AI_LanguageModelMiddlewareExtensions_WrapLanguageModel_"></a>
<a id="Vercel_AI_LanguageModelMiddlewareExtensions_WrapLanguageModel_Vercel_AI_Provider_ILanguageModel_Vercel_AI_ILanguageModelMiddleware___"></a>

### WrapLanguageModel(ILanguageModel, params ILanguageModelMiddleware[])


```csharp
public static ILanguageModel WrapLanguageModel(this ILanguageModel model, params ILanguageModelMiddleware[] middleware)
```


Wraps &lt;code class="paramref"&gt;model&lt;/code&gt; so middleware runs outside-in.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `model` | [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/) |  |
| `middleware` | `ILanguageModelMiddleware\[\]` |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Middleware.cs#L55)
