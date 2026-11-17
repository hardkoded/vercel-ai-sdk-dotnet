---
title: "DeepSeekMessageProviderOptions"
description: "The deepseek provider options of a chat message. Maps to DeepSeekMessageProviderOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekMessageProviderOptions"
  hidden: true
---

<a id="Vercel_AI_DeepSeek_DeepSeekMessageProviderOptions"></a>

Namespace: [Vercel.AI.DeepSeek](/api/Vercel.AI.DeepSeek/)


```csharp
public class DeepSeekMessageProviderOptions
```


The `deepseek` provider options of a chat message. Maps to `DeepSeekMessageProviderOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [DeepSeekAssistantMessageProviderOptions](/api/Vercel.AI.DeepSeek/DeepSeekAssistantMessageProviderOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModelOptions.cs#L286)

## Properties

<a id="Vercel_AI_DeepSeek_DeepSeekMessageProviderOptions_Name_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekMessageProviderOptions_Name"></a>

### Name


```csharp
public string? Name { get; set; }
```


The name of the participant represented by the message. Supported on system, user, and assistant messages.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModelOptions.cs#L289)

## Methods

<a id="Vercel_AI_DeepSeek_DeepSeekMessageProviderOptions_ToProviderOptions_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekMessageProviderOptions_ToProviderOptions_System_String_"></a>

### ToProviderOptions(string)


```csharp
public IReadOnlyDictionary<string, JsonElement> ToProviderOptions(string providerOptionsName = "deepseek")
```


Returns the options as message provider options under &lt;code class="paramref"&gt;providerOptionsName&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptionsName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModelOptions.cs#L292)
