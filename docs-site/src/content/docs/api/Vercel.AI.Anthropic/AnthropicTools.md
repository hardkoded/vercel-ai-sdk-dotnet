---
title: "AnthropicTools"
description: "Factories of the Anthropic provider-defined tools. Maps to anthropic.tools."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicTools"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicTools"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public static class AnthropicTools
```


Factories of the Anthropic provider-defined tools. Maps to `anthropic.tools`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.ComputerToolset.cs#L21)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicTools_ComputerToolset20260801Members_"></a>
<a id="Vercel_AI_Anthropic_AnthropicTools_ComputerToolset20260801Members"></a>

### ComputerToolset20260801Members


```csharp
public static IReadOnlyList<string> ComputerToolset20260801Members { get; }
```


Member tools of the computer toolset. Each member is returned by the API as its own `tool_use` block;
the AI SDK exposes it as the `action` of a single computer tool call. Maps to `computerToolset_20260801Members`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.ComputerToolset.cs#L27)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicTools_ComputerToolset20260801_"></a>
<a id="Vercel_AI_Anthropic_AnthropicTools_ComputerToolset20260801_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Anthropic_AnthropicComputerToolsetMemberConfig__System_String_"></a>

### `ComputerToolset20260801(IReadOnlyDictionary<string, AnthropicComputerToolsetMemberConfig>?, string)`


```csharp
public static Tool ComputerToolset20260801(IReadOnlyDictionary<string, AnthropicComputerToolsetMemberConfig>? configs = null, string name = "computer")
```


Creates the computer toolset tool (`anthropic.computer_toolset_20260801`). Its local input schema drops
the members that &lt;code class="paramref"&gt;configs&lt;/code&gt; disables from the `action` enum.
Members you omit keep their defaults (all members enabled, including `zoom`). Maps to `computerToolset_20260801`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `configs` | [IReadOnlyDictionary&lt;string, AnthropicComputerToolsetMemberConfig&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) | Per-member configuration, keyed by a name of [ComputerToolset20260801Members](#Vercel_AI_Anthropic_AnthropicTools_ComputerToolset20260801Members). |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Tool name. Default: `computer`. |

**Returns:** [Tool](/api/Vercel.AI/Tool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.ComputerToolset.cs#L36)
