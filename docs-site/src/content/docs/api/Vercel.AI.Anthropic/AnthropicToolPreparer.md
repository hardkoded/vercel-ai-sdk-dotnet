---
title: "AnthropicToolPreparer"
description: "Builds the Anthropic tools and toolchoice fields."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicToolPreparer"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicToolPreparer"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public static class AnthropicToolPreparer
```


Builds the Anthropic `tools` and `tool_choice` fields.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L67)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicToolPreparer_Prepare_"></a>
<a id="Vercel_AI_Anthropic_AnthropicToolPreparer_Prepare_System_Collections_Generic_IReadOnlyList_Vercel_AI_Anthropic_AnthropicToolDefinition__System_String_System_String_System_Nullable_System_Boolean__Vercel_AI_Anthropic_AnthropicCacheControlValidator_System_Boolean_System_Boolean_System_Boolean_System_Boolean_"></a>

### `Prepare(IReadOnlyList<AnthropicToolDefinition>?, string?, string?, bool?, AnthropicCacheControlValidator?, bool, bool, bool, bool)`


```csharp
public static AnthropicPreparedTools Prepare(IReadOnlyList<AnthropicToolDefinition>? tools, string? toolChoiceType, string? toolName, bool? disableParallelToolUse, AnthropicCacheControlValidator? cacheControl = null, bool supportsStructuredOutput = false, bool supportsStrictTools = false, bool defaultEagerInputStreaming = false, bool rejectsForcedToolUse = false)
```


Prepares tools. An empty list omits both tools and tool choice.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tools` | [IReadOnlyList&lt;AnthropicToolDefinition&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `toolChoiceType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `disableParallelToolUse` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `cacheControl` | [AnthropicCacheControlValidator](/api/Vercel.AI.Anthropic/AnthropicCacheControlValidator/) |  |
| `supportsStructuredOutput` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `supportsStrictTools` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `defaultEagerInputStreaming` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `rejectsForcedToolUse` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [AnthropicPreparedTools](/api/Vercel.AI.Anthropic/AnthropicPreparedTools/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L70)
