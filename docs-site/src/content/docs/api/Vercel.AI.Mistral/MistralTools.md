---
title: "MistralTools"
description: "Provider-executed tools of the Mistral Conversations API. Maps to mistralTools in tool/index.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralTools"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralTools"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public static class MistralTools
```


Provider-executed tools of the Mistral Conversations API. Maps to `mistralTools` in `tool/index.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralTools.cs#L11)

## Methods

<a id="Vercel_AI_Mistral_MistralTools_WebSearch_"></a>
<a id="Vercel_AI_Mistral_MistralTools_WebSearch_System_String_"></a>

### WebSearch(string)


```csharp
public static LanguageModelTool WebSearch(string name = "web_search")
```


Searches the web using the Mistral Conversations API. Maps to `webSearch`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) | The name the tool goes by in the call and its result. It is the key of the tool in upstream's `tools` record. |

**Returns:** [LanguageModelTool](/api/Vercel.AI.Provider/LanguageModelTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralTools.cs#L15)

<a id="Vercel_AI_Mistral_MistralTools_WebSearchPremium_"></a>
<a id="Vercel_AI_Mistral_MistralTools_WebSearchPremium_System_String_"></a>

### WebSearchPremium(string)


```csharp
public static LanguageModelTool WebSearchPremium(string name = "web_search_premium")
```


Searches the web and verified news using the Mistral Conversations API. Maps to `webSearchPremium`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) | The name the tool goes by in the call and its result. |

**Returns:** [LanguageModelTool](/api/Vercel.AI.Provider/LanguageModelTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralTools.cs#L22)
