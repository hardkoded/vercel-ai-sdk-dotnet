---
title: "OpenAILanguageModelCapabilities"
description: "Request limits derived from an OpenAI model id."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAILanguageModelCapabilities"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAILanguageModelCapabilities"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAILanguageModelCapabilities
```


Request limits derived from an OpenAI model id.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAILanguageModelCapabilities.cs#L11)

## Properties

<a id="Vercel_AI_OpenAI_OpenAILanguageModelCapabilities_IsReasoningModel_"></a>
<a id="Vercel_AI_OpenAI_OpenAILanguageModelCapabilities_IsReasoningModel"></a>

### IsReasoningModel


```csharp
public bool IsReasoningModel { get; }
```


Whether sampling parameters such as temperature are limited.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAILanguageModelCapabilities.cs#L34)

<a id="Vercel_AI_OpenAI_OpenAILanguageModelCapabilities_SupportedReasoningEfforts_"></a>
<a id="Vercel_AI_OpenAI_OpenAILanguageModelCapabilities_SupportedReasoningEfforts"></a>

### SupportedReasoningEfforts


```csharp
public IReadOnlyList<string>? SupportedReasoningEfforts { get; }
```


Allowed reasoning efforts. Null when the family does not constrain them.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAILanguageModelCapabilities.cs#L52)

<a id="Vercel_AI_OpenAI_OpenAILanguageModelCapabilities_SupportsAsyncToolCalling_"></a>
<a id="Vercel_AI_OpenAI_OpenAILanguageModelCapabilities_SupportsAsyncToolCalling"></a>

### SupportsAsyncToolCalling


```csharp
public bool SupportsAsyncToolCalling { get; }
```


Whether async tool calling is accepted.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAILanguageModelCapabilities.cs#L49)

<a id="Vercel_AI_OpenAI_OpenAILanguageModelCapabilities_SupportsConfigurationUpdate_"></a>
<a id="Vercel_AI_OpenAI_OpenAILanguageModelCapabilities_SupportsConfigurationUpdate"></a>

### SupportsConfigurationUpdate


```csharp
public bool SupportsConfigurationUpdate { get; }
```


Whether GPT-6 configuration updates are accepted.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAILanguageModelCapabilities.cs#L46)

<a id="Vercel_AI_OpenAI_OpenAILanguageModelCapabilities_SupportsFlexProcessing_"></a>
<a id="Vercel_AI_OpenAI_OpenAILanguageModelCapabilities_SupportsFlexProcessing"></a>

### SupportsFlexProcessing


```csharp
public bool SupportsFlexProcessing { get; }
```


Whether `service_tier: flex` is accepted.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAILanguageModelCapabilities.cs#L40)

<a id="Vercel_AI_OpenAI_OpenAILanguageModelCapabilities_SupportsNonReasoningParameters_"></a>
<a id="Vercel_AI_OpenAI_OpenAILanguageModelCapabilities_SupportsNonReasoningParameters"></a>

### SupportsNonReasoningParameters


```csharp
public bool SupportsNonReasoningParameters { get; }
```


Whether temperature and top_p stay when reasoning effort is `none`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAILanguageModelCapabilities.cs#L55)

<a id="Vercel_AI_OpenAI_OpenAILanguageModelCapabilities_SupportsPriorityProcessing_"></a>
<a id="Vercel_AI_OpenAI_OpenAILanguageModelCapabilities_SupportsPriorityProcessing"></a>

### SupportsPriorityProcessing


```csharp
public bool SupportsPriorityProcessing { get; }
```


Whether `service_tier` `priority` or `fast` is accepted.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAILanguageModelCapabilities.cs#L43)

<a id="Vercel_AI_OpenAI_OpenAILanguageModelCapabilities_SystemMessageMode_"></a>
<a id="Vercel_AI_OpenAI_OpenAILanguageModelCapabilities_SystemMessageMode"></a>

### SystemMessageMode


```csharp
public string SystemMessageMode { get; }
```


`system`, `developer`, or `remove`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAILanguageModelCapabilities.cs#L37)

## Methods

<a id="Vercel_AI_OpenAI_OpenAILanguageModelCapabilities_ForModel_"></a>
<a id="Vercel_AI_OpenAI_OpenAILanguageModelCapabilities_ForModel_System_String_"></a>

### ForModel(string)


```csharp
public static OpenAILanguageModelCapabilities ForModel(string modelId)
```


Classifies &lt;code class="paramref"&gt;modelId&lt;/code&gt; the way the OpenAI provider does.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OpenAILanguageModelCapabilities](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAILanguageModelCapabilities.cs#L58)
