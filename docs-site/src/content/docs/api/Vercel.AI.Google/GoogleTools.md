---
title: "GoogleTools"
description: "Prepares Gemini function and provider-defined tools."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleTools"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleTools"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleTools
```


Prepares Gemini function and provider-defined tools.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTools.cs#L64)

## Methods

<a id="Vercel_AI_Google_GoogleTools_Prepare_"></a>
<a id="Vercel_AI_Google_GoogleTools_Prepare_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_LanguageModelTool__System_Collections_Generic_IReadOnlyList_Vercel_AI_Google_GoogleProviderTool__Vercel_AI_Provider_ToolChoice_System_String_System_Boolean_"></a>

### `Prepare(IReadOnlyList<LanguageModelTool>?, IReadOnlyList<GoogleProviderTool>?, ToolChoice?, string, bool)`


```csharp
public static GoogleToolPreparation Prepare(IReadOnlyList<LanguageModelTool>? functionTools, IReadOnlyList<GoogleProviderTool>? providerTools, ToolChoice? toolChoice, string modelId, bool isVertexProvider)
```


Prepares tools for &lt;code class="paramref"&gt;modelId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `functionTools` | [IReadOnlyList&lt;LanguageModelTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerTools` | [IReadOnlyList&lt;GoogleProviderTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `toolChoice` | [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `isVertexProvider` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [GoogleToolPreparation](/api/Vercel.AI.Google/GoogleToolPreparation/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTools.cs#L67)
