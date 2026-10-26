---
title: "ToolChoices"
description: "Tool-choice normalization. Maps to prepareToolChoice."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolChoices"
  hidden: true
---

<a id="Vercel_AI_Prompt_ToolChoices"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public static class ToolChoices
```


Tool-choice normalization. Maps to `prepareToolChoice`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareToolChoice.cs#L27)

## Methods

<a id="Vercel_AI_Prompt_ToolChoices_PrepareToolChoice_"></a>
<a id="Vercel_AI_Prompt_ToolChoices_PrepareToolChoice_System_String_"></a>

### PrepareToolChoice(string?)


```csharp
public static PreparedToolChoice PrepareToolChoice(string? toolChoice)
```


Returns auto when &lt;code class="paramref"&gt;toolChoice&lt;/code&gt; is null. String values are auto, none, or required.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolChoice` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [PreparedToolChoice](/api/Vercel.AI.Prompt/PreparedToolChoice/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareToolChoice.cs#L46)

<a id="Vercel_AI_Prompt_ToolChoices_PrepareToolChoice_Vercel_AI_Provider_ToolChoice_"></a>

### PrepareToolChoice(ToolChoice?)


```csharp
public static PreparedToolChoice PrepareToolChoice(ToolChoice? toolChoice)
```


Returns auto when &lt;code class="paramref"&gt;toolChoice&lt;/code&gt; is null.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolChoice` | [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/) |  |

**Returns:** [PreparedToolChoice](/api/Vercel.AI.Prompt/PreparedToolChoice/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareToolChoice.cs#L30)
