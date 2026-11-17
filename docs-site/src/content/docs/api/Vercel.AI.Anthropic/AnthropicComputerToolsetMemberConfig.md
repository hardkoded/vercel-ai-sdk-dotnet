---
title: "AnthropicComputerToolsetMemberConfig"
description: "Configuration of one member of the computer toolset. Maps to a configs entry of computerToolset20260801."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicComputerToolsetMemberConfig"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicComputerToolsetMemberConfig"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicComputerToolsetMemberConfig
```


Configuration of one member of the computer toolset. Maps to a `configs` entry of `computerToolset_20260801`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.ComputerToolset.cs#L11)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicComputerToolsetMemberConfig_DeferLoading_"></a>
<a id="Vercel_AI_Anthropic_AnthropicComputerToolsetMemberConfig_DeferLoading"></a>

### DeferLoading


```csharp
public bool? DeferLoading { get; set; }
```


Whether this member tool is only loaded once discovered through tool search. Default: false.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.ComputerToolset.cs#L17)

<a id="Vercel_AI_Anthropic_AnthropicComputerToolsetMemberConfig_Enabled_"></a>
<a id="Vercel_AI_Anthropic_AnthropicComputerToolsetMemberConfig_Enabled"></a>

### Enabled


```csharp
public bool? Enabled { get; set; }
```


Whether Claude can use this member tool. Default: true.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.ComputerToolset.cs#L14)
