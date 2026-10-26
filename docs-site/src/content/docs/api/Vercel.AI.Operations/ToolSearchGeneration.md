---
title: "ToolSearchGeneration"
description: "Runs the tool-search step loop used by generateText, streamText, and the tool-loop agent. The model tool list, execution timing, and catalog messages match a..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolSearchGeneration"
  hidden: true
---

<a id="Vercel_AI_Operations_ToolSearchGeneration"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class ToolSearchGeneration
```


Runs the tool-search step loop used by generateText, streamText, and the tool-loop agent.
The model tool list, execution timing, and catalog messages match across those modes.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L373)

## Methods

<a id="Vercel_AI_Operations_ToolSearchGeneration_RunAsync_"></a>
<a id="Vercel_AI_Operations_ToolSearchGeneration_RunAsync_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_SearchableTool__System_Collections_Generic_IReadOnlyDictionary_System_String_System_Collections_Generic_IReadOnlyList_System_String___System_String_System_Collections_Generic_IReadOnlyList_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ToolSearchCall___System_String_System_Threading_CancellationToken_"></a>

### `RunAsync(string, IReadOnlyDictionary<string, SearchableTool>, IReadOnlyDictionary<string, IReadOnlyList<string>>?, string, IReadOnlyList<IReadOnlyList<ToolSearchCall>>, string, CancellationToken)`


```csharp
public static Task<ToolSearchRun> RunAsync(string mode, IReadOnlyDictionary<string, SearchableTool> tools, IReadOnlyDictionary<string, IReadOnlyList<string>>? routing, string prompt, IReadOnlyList<IReadOnlyList<ToolSearchCall>> calls, string finalText, CancellationToken cancellationToken = default)
```


Runs one generation mode. Discovered tools appear on the next step.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mode` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `tools` | [IReadOnlyDictionary&lt;string, SearchableTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `routing` | [IReadOnlyDictionary&lt;string, IReadOnlyList&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `prompt` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `calls` | [IReadOnlyList&lt;IReadOnlyList&lt;ToolSearchCall&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `finalText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ToolSearchRun&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L376)
