---
title: "PromptTools"
description: "Tool preparation. Maps to prepareTools."
editUrl: false
prev: false
next: false
sidebar:
  label: "PromptTools"
  hidden: true
---

<a id="Vercel_AI_Prompt_PromptTools"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public static class PromptTools
```


Tool preparation. Maps to `prepareTools`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L102)

## Methods

<a id="Vercel_AI_Prompt_PromptTools_PrepareTools_"></a>
<a id="Vercel_AI_Prompt_PromptTools_PrepareTools_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Prompt_PromptTool__System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_Object__System_String_"></a>

### `PrepareTools(IReadOnlyDictionary<string, PromptTool>?, IReadOnlyList<string>?, IReadOnlyDictionary<string, object?>?, string?)`


```csharp
public static IReadOnlyList<PreparedTool>? PrepareTools(IReadOnlyDictionary<string, PromptTool>? tools, IReadOnlyList<string>? toolOrder = null, IReadOnlyDictionary<string, object?>? toolsContext = null, string? sandboxDescription = null)
```


Converts tools into the language-model list. Returns null when &lt;code class="paramref"&gt;tools&lt;/code&gt; is null or empty.
&lt;code class="paramref"&gt;toolOrder&lt;/code&gt; names come first. The rest are sorted by name.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tools` | [IReadOnlyDictionary&lt;string, PromptTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `toolOrder` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `toolsContext` | [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `sandboxDescription` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IReadOnlyList&lt;PreparedTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/PrepareTools.cs#L108)
