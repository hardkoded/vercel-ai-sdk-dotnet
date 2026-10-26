---
title: "AnthropicMessagesRequest"
description: "Builds an Anthropic Messages request from SDK call options."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicMessagesRequest"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicMessagesRequest"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public static class AnthropicMessagesRequest
```


Builds an Anthropic Messages request from SDK call options.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L53)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicMessagesRequest_Prepare_"></a>
<a id="Vercel_AI_Anthropic_AnthropicMessagesRequest_Prepare_System_String_Vercel_AI_Provider_LanguageModelCallOptions_System_Boolean_System_String_System_Boolean_System_Boolean_Vercel_AI_Anthropic_AnthropicModelSettings_"></a>

### Prepare(string, LanguageModelCallOptions, bool, string, bool, bool, AnthropicModelSettings?)


```csharp
public static AnthropicPreparedRequest Prepare(string modelId, LanguageModelCallOptions options, bool stream, string providerName, bool supportsNativeStructuredOutput = true, bool supportsStrictTools = true, AnthropicModelSettings? settings = null)
```


Prepares the request. &lt;code class="paramref"&gt;stream&lt;/code&gt; adds `stream: true` and eager tool streaming.
Hosts without the output format or strict tools turn them off with the last two flags.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `stream` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `providerName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `supportsNativeStructuredOutput` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `supportsStrictTools` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `settings` | [AnthropicModelSettings](/api/Vercel.AI.Anthropic/AnthropicModelSettings/) |  |

**Returns:** [AnthropicPreparedRequest](/api/Vercel.AI.Anthropic/AnthropicPreparedRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicMessagesRequest.cs#L59)
