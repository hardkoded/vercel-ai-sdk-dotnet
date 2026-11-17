---
title: "DeepSeekChatConfig"
description: "The configuration of a DeepSeekChatLanguageModel. Maps to DeepSeekChatConfig. Transport is .NET-only. It replaces upstream's headers and fetch."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekChatConfig"
  hidden: true
---

<a id="Vercel_AI_DeepSeek_DeepSeekChatConfig"></a>

Namespace: [Vercel.AI.DeepSeek](/api/Vercel.AI.DeepSeek/)


```csharp
public sealed class DeepSeekChatConfig
```


The configuration of a [DeepSeekChatLanguageModel](/api/Vercel.AI.DeepSeek/DeepSeekChatLanguageModel/). Maps to `DeepSeekChatConfig`.
[Transport](#Vercel_AI_DeepSeek_DeepSeekChatConfig_Transport) is .NET-only. It replaces upstream's `headers` and `fetch`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModel.cs#L21)

## Constructors

<a id="Vercel_AI_DeepSeek_DeepSeekChatConfig__ctor_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekChatConfig__ctor_System_String_Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_System_Func_System_String_System_String_System_Uri__"></a>

### `DeepSeekChatConfig(string, OpenAICompatibleProvider, Func<string, string, Uri>)`


```csharp
public DeepSeekChatConfig(string provider, OpenAICompatibleProvider transport, Func<string, string, Uri> url)
```


Creates a configuration.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Provider id, such as `deepseek.chat`. The segment before the first dot is the provider-options key. |
| `transport` | [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/) | The provider whose HTTP client and headers send the requests. |
| `url` | [Func&lt;string, string, Uri&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) | Builds the request URL from the model id and the path, such as `/chat/completions`. Maps to `url`. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModel.cs#L27)

## Properties

<a id="Vercel_AI_DeepSeek_DeepSeekChatConfig_Provider_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekChatConfig_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id, such as `deepseek.chat`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModel.cs#L39)

<a id="Vercel_AI_DeepSeek_DeepSeekChatConfig_SupportsAssistantPrefixCompletion_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekChatConfig_SupportsAssistantPrefixCompletion"></a>

### SupportsAssistantPrefixCompletion


```csharp
public bool? SupportsAssistantPrefixCompletion { get; init; }
```


Whether the endpoint supports assistant prefix completion. Maps to `supportsAssistantPrefixCompletion`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModel.cs#L48)

<a id="Vercel_AI_DeepSeek_DeepSeekChatConfig_SupportsPenaltySampling_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekChatConfig_SupportsPenaltySampling"></a>

### SupportsPenaltySampling


```csharp
public bool? SupportsPenaltySampling { get; init; }
```


Whether the endpoint supports penalty sampling. Maps to `supportsPenaltySampling`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModel.cs#L54)

<a id="Vercel_AI_DeepSeek_DeepSeekChatConfig_SupportsStrictToolCalls_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekChatConfig_SupportsStrictToolCalls"></a>

### SupportsStrictToolCalls


```csharp
public bool? SupportsStrictToolCalls { get; init; }
```


Whether the endpoint supports strict tool calls. Maps to `supportsStrictToolCalls`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModel.cs#L51)

<a id="Vercel_AI_DeepSeek_DeepSeekChatConfig_SupportsStructuredOutputs_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekChatConfig_SupportsStructuredOutputs"></a>

### SupportsStructuredOutputs


```csharp
public bool? SupportsStructuredOutputs { get; init; }
```


Whether the endpoint supports JSON schema response formats. Maps to `supportsStructuredOutputs`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModel.cs#L60)

<a id="Vercel_AI_DeepSeek_DeepSeekChatConfig_SupportsThinking_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekChatConfig_SupportsThinking"></a>

### SupportsThinking


```csharp
public bool? SupportsThinking { get; init; }
```


Whether the endpoint supports thinking. Maps to `supportsThinking`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModel.cs#L57)

<a id="Vercel_AI_DeepSeek_DeepSeekChatConfig_Transport_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekChatConfig_Transport"></a>

### Transport


```csharp
public OpenAICompatibleProvider Transport { get; }
```


The provider whose HTTP client and headers send the requests.

**Value:** [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModel.cs#L42)

<a id="Vercel_AI_DeepSeek_DeepSeekChatConfig_Url_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekChatConfig_Url"></a>

### Url


```csharp
public Func<string, string, Uri> Url { get; }
```


Builds the request URL from the model id and the path. Maps to `url`.

**Value:** [Func&lt;string, string, Uri&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModel.cs#L45)
