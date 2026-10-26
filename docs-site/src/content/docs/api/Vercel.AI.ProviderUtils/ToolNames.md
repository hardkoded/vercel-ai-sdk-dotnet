---
title: "ToolNames"
description: "Builds tool name maps. Maps to createToolNameMapping."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolNames"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ToolNames"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class ToolNames
```


Builds tool name maps. Maps to `createToolNameMapping`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ToolNameMapping.cs#L56)

## Methods

<a id="Vercel_AI_ProviderUtils_ToolNames_CreateToolNameMapping_"></a>
<a id="Vercel_AI_ProviderUtils_ToolNames_CreateToolNameMapping_System_Collections_Generic_IReadOnlyList_Vercel_AI_ProviderUtils_MappedTool__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `CreateToolNameMapping(IReadOnlyList<MappedTool>?, IReadOnlyDictionary<string, string>)`


```csharp
public static ToolNameMapping CreateToolNameMapping(IReadOnlyList<MappedTool>? tools, IReadOnlyDictionary<string, string> providerToolNames)
```


Creates a mapping from provider-defined tools whose ids are listed in &lt;code class="paramref"&gt;providerToolNames&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tools` | [IReadOnlyList&lt;MappedTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerToolNames` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [ToolNameMapping](/api/Vercel.AI.ProviderUtils/ToolNameMapping/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ToolNameMapping.cs#L59)
