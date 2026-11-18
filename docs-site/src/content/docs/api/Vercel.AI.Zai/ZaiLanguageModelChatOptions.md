---
title: "ZaiLanguageModelChatOptions"
description: "The zai provider options of a chat call. Maps to ZaiLanguageModelChatOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "ZaiLanguageModelChatOptions"
  hidden: true
---

<a id="Vercel_AI_Zai_ZaiLanguageModelChatOptions"></a>

Namespace: [Vercel.AI.Zai](/api/Vercel.AI.Zai/)


```csharp
public sealed class ZaiLanguageModelChatOptions
```


The `zai` provider options of a chat call. Maps to `ZaiLanguageModelChatOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiLanguageModelChatOptions.cs#L57)

## Properties

<a id="Vercel_AI_Zai_ZaiLanguageModelChatOptions_DoSample_"></a>
<a id="Vercel_AI_Zai_ZaiLanguageModelChatOptions_DoSample"></a>

### DoSample


```csharp
public bool? DoSample { get; set; }
```


Enables or disables sampling. When disabled, temperature and topP do not take effect.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiLanguageModelChatOptions.cs#L60)

<a id="Vercel_AI_Zai_ZaiLanguageModelChatOptions_ReasoningEffort_"></a>
<a id="Vercel_AI_Zai_ZaiLanguageModelChatOptions_ReasoningEffort"></a>

### ReasoningEffort


```csharp
public ZaiReasoningEffort? ReasoningEffort { get; set; }
```


Controls reasoning effort for GLM-5.2 and later models.

**Value:** `ZaiReasoningEffort?`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiLanguageModelChatOptions.cs#L66)

<a id="Vercel_AI_Zai_ZaiLanguageModelChatOptions_RequestId_"></a>
<a id="Vercel_AI_Zai_ZaiLanguageModelChatOptions_RequestId"></a>

### RequestId


```csharp
public string? RequestId { get; set; }
```


A caller-provided request identifier between 6 and 64 characters.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiLanguageModelChatOptions.cs#L72)

<a id="Vercel_AI_Zai_ZaiLanguageModelChatOptions_Thinking_"></a>
<a id="Vercel_AI_Zai_ZaiLanguageModelChatOptions_Thinking"></a>

### Thinking


```csharp
public ZaiThinking? Thinking { get; set; }
```


Controls model thinking and whether reasoning from earlier turns is kept.

**Value:** [ZaiThinking](/api/Vercel.AI.Zai/ZaiThinking/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiLanguageModelChatOptions.cs#L63)

<a id="Vercel_AI_Zai_ZaiLanguageModelChatOptions_ToolStream_"></a>
<a id="Vercel_AI_Zai_ZaiLanguageModelChatOptions_ToolStream"></a>

### ToolStream


```csharp
public bool? ToolStream { get; set; }
```


Enables incremental function-call argument streaming on supported models.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiLanguageModelChatOptions.cs#L69)

<a id="Vercel_AI_Zai_ZaiLanguageModelChatOptions_UserId_"></a>
<a id="Vercel_AI_Zai_ZaiLanguageModelChatOptions_UserId"></a>

### UserId


```csharp
public string? UserId { get; set; }
```


A non-sensitive end-user identifier between 6 and 128 characters.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiLanguageModelChatOptions.cs#L75)

## Methods

<a id="Vercel_AI_Zai_ZaiLanguageModelChatOptions_ToProviderOptions_"></a>
<a id="Vercel_AI_Zai_ZaiLanguageModelChatOptions_ToProviderOptions_System_String_"></a>

### ToProviderOptions(string)


```csharp
public IReadOnlyDictionary<string, JsonElement> ToProviderOptions(string providerOptionsName = "zai")
```


Validates the options and returns them as call provider options under &lt;code class="paramref"&gt;providerOptionsName&lt;/code&gt;
(`zai` by default), ready for `LanguageModelCallOptions.ProviderOptions`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptionsName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/) | An option has the wrong value. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiLanguageModelChatOptions.cs#L82)
