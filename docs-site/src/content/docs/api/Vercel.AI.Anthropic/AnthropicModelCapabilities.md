---
title: "AnthropicModelCapabilities"
description: "What a Claude model id accepts on the Messages API."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicModelCapabilities"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicModelCapabilities
```


What a Claude model id accepts on the Messages API.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelCapabilities.cs#L10)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities__ctor_System_Int32_System_Boolean_System_Boolean_System_Boolean_System_Boolean_System_Boolean_System_Boolean_System_Boolean_System_Boolean_System_Boolean_System_Boolean_"></a>

### AnthropicModelCapabilities(int, bool, bool, bool, bool, bool, bool, bool, bool, bool, bool)


```csharp
public AnthropicModelCapabilities(int maxOutputTokens, bool supportsStructuredOutput, bool supportsAdaptiveThinking, bool rejectsSamplingParameters, bool supportsXhighEffort, bool rejectsThinkingDisabledAboveHighEffort, bool rejectsThinkingDisabled, bool rejectsBudgetThinking, bool rejectsForcedToolUse, bool isKnownModel, bool supportsBetweenToolsThinking = false)
```


Creates a capability set.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `maxOutputTokens` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `supportsStructuredOutput` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `supportsAdaptiveThinking` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `rejectsSamplingParameters` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `supportsXhighEffort` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `rejectsThinkingDisabledAboveHighEffort` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `rejectsThinkingDisabled` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `rejectsBudgetThinking` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `rejectsForcedToolUse` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `isKnownModel` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `supportsBetweenToolsThinking` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelCapabilities.cs#L13)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_IsKnownModel_"></a>
<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_IsKnownModel"></a>

### IsKnownModel


```csharp
public bool IsKnownModel { get; }
```


The id is a recognized Claude model rather than a forward-compatible guess.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelCapabilities.cs#L70)

<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_MaxOutputTokens_"></a>
<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public int MaxOutputTokens { get; }
```


Model output token ceiling.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelCapabilities.cs#L40)

<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_RejectsBudgetThinking_"></a>
<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_RejectsBudgetThinking"></a>

### RejectsBudgetThinking


```csharp
public bool RejectsBudgetThinking { get; }
```


Budget-based thinking is rejected and sent as adaptive thinking.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelCapabilities.cs#L61)

<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_RejectsForcedToolUse_"></a>
<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_RejectsForcedToolUse"></a>

### RejectsForcedToolUse


```csharp
public bool RejectsForcedToolUse { get; }
```


Forced tool choice is rejected.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelCapabilities.cs#L64)

<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_RejectsSamplingParameters_"></a>
<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_RejectsSamplingParameters"></a>

### RejectsSamplingParameters


```csharp
public bool RejectsSamplingParameters { get; }
```


Temperature, top-p, and top-k are rejected.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelCapabilities.cs#L49)

<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_RejectsThinkingDisabled_"></a>
<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_RejectsThinkingDisabled"></a>

### RejectsThinkingDisabled


```csharp
public bool RejectsThinkingDisabled { get; }
```


The model always thinks and rejects `disabled` thinking.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelCapabilities.cs#L58)

<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_RejectsThinkingDisabledAboveHighEffort_"></a>
<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_RejectsThinkingDisabledAboveHighEffort"></a>

### RejectsThinkingDisabledAboveHighEffort


```csharp
public bool RejectsThinkingDisabledAboveHighEffort { get; }
```


Thinking cannot be disabled at effort above `high`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelCapabilities.cs#L55)

<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_SupportsAdaptiveThinking_"></a>
<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_SupportsAdaptiveThinking"></a>

### SupportsAdaptiveThinking


```csharp
public bool SupportsAdaptiveThinking { get; }
```


Thinking is adaptive rather than budget-based.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelCapabilities.cs#L46)

<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_SupportsBetweenToolsThinking_"></a>
<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_SupportsBetweenToolsThinking"></a>

### SupportsBetweenToolsThinking


```csharp
public bool SupportsBetweenToolsThinking { get; }
```


Thinking type `between_tools` is accepted. It is the lowest thinking setting on models that reject disabled thinking.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelCapabilities.cs#L67)

<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_SupportsStructuredOutput_"></a>
<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_SupportsStructuredOutput"></a>

### SupportsStructuredOutput


```csharp
public bool SupportsStructuredOutput { get; }
```


Native `output_config.format` is available.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelCapabilities.cs#L43)

<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_SupportsXhighEffort_"></a>
<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_SupportsXhighEffort"></a>

### SupportsXhighEffort


```csharp
public bool SupportsXhighEffort { get; }
```


Effort `xhigh` is accepted.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelCapabilities.cs#L52)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_Get_"></a>
<a id="Vercel_AI_Anthropic_AnthropicModelCapabilities_Get_System_String_"></a>

### Get(string)


```csharp
public static AnthropicModelCapabilities Get(string modelId)
```


Resolves capabilities from a model id, including Bedrock-style prefixes.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [AnthropicModelCapabilities](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicModelCapabilities.cs#L73)
