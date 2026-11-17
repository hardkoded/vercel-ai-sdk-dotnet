---
title: "DeepSeekFilePartProviderOptions"
description: "The deepseek provider options of an image part. Maps to DeepSeekFilePartProviderOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekFilePartProviderOptions"
  hidden: true
---

<a id="Vercel_AI_DeepSeek_DeepSeekFilePartProviderOptions"></a>

Namespace: [Vercel.AI.DeepSeek](/api/Vercel.AI.DeepSeek/)


```csharp
public sealed class DeepSeekFilePartProviderOptions
```


The `deepseek` provider options of an image part. Maps to `DeepSeekFilePartProviderOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModelOptions.cs#L412)

## Properties

<a id="Vercel_AI_DeepSeek_DeepSeekFilePartProviderOptions_FileData_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekFilePartProviderOptions_FileData"></a>

### FileData


```csharp
public bool? FileData { get; set; }
```


Sends inline image data as a DeepSeek `file` part using `file_data` instead of an `image_url` data URL.
The file part's filename is preserved. It only applies to inline image data and cannot be combined with
[ImageDetail](#Vercel_AI_DeepSeek_DeepSeekFilePartProviderOptions_ImageDetail).

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModelOptions.cs#L424)

<a id="Vercel_AI_DeepSeek_DeepSeekFilePartProviderOptions_ImageDetail_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekFilePartProviderOptions_ImageDetail"></a>

### ImageDetail


```csharp
public string? ImageDetail { get; set; }
```


Controls how DeepSeek processes an image sent as an `image_url` part: `low`, `high`, `original` or `auto`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModelOptions.cs#L417)

## Methods

<a id="Vercel_AI_DeepSeek_DeepSeekFilePartProviderOptions_ToProviderOptions_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekFilePartProviderOptions_ToProviderOptions_System_String_"></a>

### ToProviderOptions(string)


```csharp
public IReadOnlyDictionary<string, JsonElement> ToProviderOptions(string providerOptionsName = "deepseek")
```


Returns the options as file part provider options under &lt;code class="paramref"&gt;providerOptionsName&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptionsName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekChatLanguageModelOptions.cs#L427)
