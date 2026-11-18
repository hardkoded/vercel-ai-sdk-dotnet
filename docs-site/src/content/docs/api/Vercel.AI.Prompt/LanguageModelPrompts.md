---
title: "LanguageModelPrompts"
description: "Prompt conversion before a model call. Maps to convert-to-language-model-prompt.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "LanguageModelPrompts"
  hidden: true
---

<a id="Vercel_AI_Prompt_LanguageModelPrompts"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public static class LanguageModelPrompts
```


Prompt conversion before a model call. Maps to `convert-to-language-model-prompt.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/ConvertToLanguageModelPrompt.cs#L12)

## Methods

<a id="Vercel_AI_Prompt_LanguageModelPrompts_ConvertToLanguageModelMessage_"></a>
<a id="Vercel_AI_Prompt_LanguageModelPrompts_ConvertToLanguageModelMessage_Vercel_AI_Provider_ModelMessage_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Util_DownloadResult__System_String_"></a>

### `ConvertToLanguageModelMessage(ModelMessage, IReadOnlyDictionary<string, DownloadResult>, string?)`


```csharp
public static ModelMessage ConvertToLanguageModelMessage(ModelMessage message, IReadOnlyDictionary<string, DownloadResult> downloadedAssets, string? provider = null)
```


Converts a message to its language model form. Maps to `convertToLanguageModelMessage`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [ModelMessage](/api/Vercel.AI.Provider/ModelMessage/) | The message to convert. |
| `downloadedAssets` | [IReadOnlyDictionary&lt;string, DownloadResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) | Downloaded data by URL. Only available if the model does not support URLs. |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Provider of the model. It is only used to convert the deprecated `file-id` and `image-file-id` tool result parts with a string file id. |

**Returns:** [ModelMessage](/api/Vercel.AI.Provider/ModelMessage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/ConvertToLanguageModelPrompt.cs#L57)

<a id="Vercel_AI_Prompt_LanguageModelPrompts_ConvertToLanguageModelPromptAsync_"></a>
<a id="Vercel_AI_Prompt_LanguageModelPrompts_ConvertToLanguageModelPromptAsync_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Func_System_Collections_Generic_IReadOnlyList_Vercel_AI_Util_DownloadRequestItem__System_Threading_Tasks_Task_System_Collections_Generic_IReadOnlyList_Vercel_AI_Util_DownloadResult____System_Threading_CancellationToken_System_String_"></a>

### `ConvertToLanguageModelPromptAsync(IReadOnlyList<ModelMessage>, IReadOnlyDictionary<string, string>, Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>>?, CancellationToken, string?)`


```csharp
public static Task<IReadOnlyList<ModelMessage>> ConvertToLanguageModelPromptAsync(IReadOnlyList<ModelMessage> messages, IReadOnlyDictionary<string, string> supportedUrls, Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>>? download, CancellationToken abortSignal = default, string? provider = null)
```


Downloads the files whose URLs the model cannot read itself and inlines their bytes. Maps to `convertToLanguageModelPrompt`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) | The prompt messages. |
| `supportedUrls` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) | The model's [SupportedUrls](/api/Vercel.AI.Provider/ILanguageModel/#Vercel_AI_Provider_ILanguageModel_SupportedUrls). |
| `download` | [Func&lt;IReadOnlyList&lt;DownloadRequestItem&gt;, Task&lt;IReadOnlyList&lt;DownloadResult&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) | Custom download function. When it is null, URLs are downloaded with [DefaultDownloadFunction](/api/Vercel.AI.Util/DefaultDownloadFunction/). |
| `abortSignal` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) | Cancellation for the downloads. |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Provider of the model. It is only used to convert the deprecated `file-id` and `image-file-id` tool result parts with a string file id. |

**Returns:** [Task&lt;IReadOnlyList&lt;ModelMessage&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/ConvertToLanguageModelPrompt.cs#L22)

<a id="Vercel_AI_Prompt_LanguageModelPrompts_DownloadAssetsAsync_"></a>
<a id="Vercel_AI_Prompt_LanguageModelPrompts_DownloadAssetsAsync_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__System_Func_System_Collections_Generic_IReadOnlyList_Vercel_AI_Util_DownloadRequestItem__System_Threading_Tasks_Task_System_Collections_Generic_IReadOnlyList_Vercel_AI_Util_DownloadResult____System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `DownloadAssetsAsync(IReadOnlyList<ModelMessage>, Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>>, IReadOnlyDictionary<string, string>, CancellationToken)`


```csharp
public static Task<IReadOnlyDictionary<string, DownloadResult>> DownloadAssetsAsync(IReadOnlyList<ModelMessage> messages, Func<IReadOnlyList<DownloadRequestItem>, Task<IReadOnlyList<DownloadResult>>> download, IReadOnlyDictionary<string, string> supportedUrls, CancellationToken abortSignal = default)
```


Downloads files from URLs in the user messages. Maps to `downloadAssets`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `download` | [Func&lt;IReadOnlyList&lt;DownloadRequestItem&gt;, Task&lt;IReadOnlyList&lt;DownloadResult&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `supportedUrls` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `abortSignal` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IReadOnlyDictionary&lt;string, DownloadResult&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1) — Downloaded data by URL. Only URLs the download function returned data for.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/ConvertToLanguageModelPrompt.cs#L86)
