---
title: "GoogleInteractionsTools"
description: "Maps tool definitions and toolChoice onto the Gemini Interactions tools[] and toolchoice request fields. Port of prepare-google-interactions-tools.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleInteractionsTools"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleInteractionsTools"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleInteractionsTools
```


Maps tool definitions and `toolChoice` onto the Gemini Interactions `tools[]` and `tool_choice` request fields.
Port of `prepare-google-interactions-tools.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractionsTools.cs#L40)

## Methods

<a id="Vercel_AI_Google_GoogleInteractionsTools_Prepare_"></a>
<a id="Vercel_AI_Google_GoogleInteractionsTools_Prepare_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_LanguageModelTool__Vercel_AI_Provider_ToolChoice_"></a>

### `Prepare(IReadOnlyList<LanguageModelTool>?, ToolChoice?)`


```csharp
public static PrepareGoogleInteractionsToolsResult Prepare(IReadOnlyList<LanguageModelTool>? tools, ToolChoice? toolChoice = null)
```


Maps &lt;code class="paramref"&gt;tools&lt;/code&gt; and &lt;code class="paramref"&gt;toolChoice&lt;/code&gt;. `toolChoice: none` omits every tool.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tools` | [IReadOnlyList&lt;LanguageModelTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `toolChoice` | [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/) |  |

**Returns:** [PrepareGoogleInteractionsToolsResult](/api/Vercel.AI.Google/PrepareGoogleInteractionsToolsResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractionsTools.cs#L43)
