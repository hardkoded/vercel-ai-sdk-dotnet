---
title: "OpenAICompatibleOptions"
description: "Settings for an OpenAI Chat Completions compatible provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICompatibleOptions"
  hidden: true
---

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions"></a>

Namespace: [Vercel.AI.OpenAICompatible](/api/Vercel.AI.OpenAICompatible/)


```csharp
public class OpenAICompatibleOptions
```


Settings for an OpenAI Chat Completions compatible provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [AssemblyAIOptions](/api/Vercel.AI.AssemblyAI/AssemblyAIOptions/), [BasetenOptions](/api/Vercel.AI.Baseten/BasetenOptions/), [DeepgramOptions](/api/Vercel.AI.Deepgram/DeepgramOptions/), [ElevenLabsOptions](/api/Vercel.AI.ElevenLabs/ElevenLabsOptions/), [GladiaOptions](/api/Vercel.AI.Gladia/GladiaOptions/), [HumeOptions](/api/Vercel.AI.Hume/HumeOptions/), [KlingAIOptions](/api/Vercel.AI.KlingAI/KlingAIOptions/), [OpenAIOptions](/api/Vercel.AI.OpenAI/OpenAIOptions/), [RevAIOptions](/api/Vercel.AI.RevAI/RevAIOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L34)

## Properties

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_AdditionalApiKeyEnvironmentVariables_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_AdditionalApiKeyEnvironmentVariables"></a>

### AdditionalApiKeyEnvironmentVariables


```csharp
public string[] AdditionalApiKeyEnvironmentVariables { get; set; }
```


Extra environment variables accepted as a fallback.

**Value:** [string\[\]](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L49)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ApiKey_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ApiKey"></a>

### ApiKey


```csharp
public string? ApiKey { get; set; }
```


Explicit API key. Wins over the environment variable.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L43)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ApiKeyEnvironmentVariable_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ApiKeyEnvironmentVariable"></a>

### ApiKeyEnvironmentVariable


```csharp
public string ApiKeyEnvironmentVariable { get; set; }
```


Environment variable read when [ApiKey](#Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ApiKey) is empty.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L46)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ApiKeyHeaderName_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ApiKeyHeaderName"></a>

### ApiKeyHeaderName


```csharp
public string ApiKeyHeaderName { get; set; }
```


Header name for [ApiKeyHeader](/api/Vercel.AI.OpenAICompatible/ApiKeyStyle/#Vercel_AI_OpenAICompatible_ApiKeyStyle_ApiKeyHeader) and [CustomHeader](/api/Vercel.AI.OpenAICompatible/ApiKeyStyle/#Vercel_AI_OpenAICompatible_ApiKeyStyle_CustomHeader).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L55)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ApiKeyStyle_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ApiKeyStyle"></a>

### ApiKeyStyle


```csharp
public ApiKeyStyle ApiKeyStyle { get; set; }
```


How to send the key.

**Value:** [ApiKeyStyle](/api/Vercel.AI.OpenAICompatible/ApiKeyStyle/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L52)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_AzureApiVersion_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_AzureApiVersion"></a>

### AzureApiVersion


```csharp
public string? AzureApiVersion { get; set; }
```


When set, chat, completion, embedding, and Responses URLs use the Azure routes and this API version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L73)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_AzureUseDeploymentBasedUrls_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_AzureUseDeploymentBasedUrls"></a>

### AzureUseDeploymentBasedUrls


```csharp
public bool AzureUseDeploymentBasedUrls { get; set; }
```


With [AzureApiVersion](#Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_AzureApiVersion), uses `/openai/deployments/{id}` routes instead of `/openai/v1`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L76)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_BaseUrl_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_BaseUrl"></a>

### BaseUrl


```csharp
public string BaseUrl { get; set; }
```


API origin, without a trailing slash. Chat calls append [ChatCompletionsPath](#Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ChatCompletionsPath).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L40)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ChatCompletionsPath_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ChatCompletionsPath"></a>

### ChatCompletionsPath


```csharp
public string ChatCompletionsPath { get; set; }
```


Relative chat path.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L58)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_CompletionsPath_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_CompletionsPath"></a>

### CompletionsPath


```csharp
public string CompletionsPath { get; set; }
```


Relative completions path.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L157)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ConvertUsage_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ConvertUsage"></a>

### ConvertUsage


```csharp
public Func<JsonElement, LanguageModelUsage>? ConvertUsage { get; set; }
```


Converts the raw chat usage object. Null uses `JsonElement%7d)`.

**Value:** [Func&lt;JsonElement, LanguageModelUsage&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L151)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_DownloadHttpClient_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_DownloadHttpClient"></a>

### DownloadHttpClient


```csharp
public HttpClient? DownloadHttpClient { get; set; }
```


Client for hops off the provider origin when a URL from a response body is downloaded (Fal, Replicate, Luma, Prodia).
It must not follow redirects: each hop is validated before it is requested. Null uses a shared client that does not follow them.

**Value:** [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L172)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_EmbeddingsPath_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_EmbeddingsPath"></a>

### EmbeddingsPath


```csharp
public string EmbeddingsPath { get; set; }
```


Relative embeddings path.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L61)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_Headers_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_Headers"></a>

### Headers


```csharp
public Dictionary<string, string> Headers { get; }
```


Extra headers sent on every call. These override the API key header.

**Value:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L79)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ImageBaseUrl_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ImageBaseUrl"></a>

### ImageBaseUrl


```csharp
public string? ImageBaseUrl { get; set; }
```


Origin for image calls when it differs from [BaseUrl](#Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_BaseUrl).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L166)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ImagesPath_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ImagesPath"></a>

### ImagesPath


```csharp
public string ImagesPath { get; set; }
```


Relative images path.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L64)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_IncludeUsage_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_IncludeUsage"></a>

### IncludeUsage


```csharp
public bool IncludeUsage { get; set; }
```


When true, streaming requests send `stream_options.include_usage`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L85)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_JsonTextOverridesToolCalls_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_JsonTextOverridesToolCalls"></a>

### JsonTextOverridesToolCalls


```csharp
public bool JsonTextOverridesToolCalls { get; set; }
```


When true, a JSON-mode response that has text drops its tool calls, and a `tool_calls` finish becomes stop.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L94)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_MapFinishReason_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_MapFinishReason"></a>

### MapFinishReason


```csharp
public Func<string?, FinishReason?>? MapFinishReason { get; set; }
```


Maps a raw finish reason before the shared mapping. Null falls through.

**Value:** [Func&lt;string, FinishReason?&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L154)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_MaxEmbeddingsPerCall_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_MaxEmbeddingsPerCall"></a>

### MaxEmbeddingsPerCall


```csharp
public int MaxEmbeddingsPerCall { get; set; }
```


Maximum embedding inputs in one request.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L163)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ProviderName_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ProviderName"></a>

### ProviderName


```csharp
public string ProviderName { get; set; }
```


Provider id recorded on models.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L37)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_QueryParameters_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_QueryParameters"></a>

### QueryParameters


```csharp
public Dictionary<string, string> QueryParameters { get; }
```


Query parameters appended to every request URL.

**Value:** [Dictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L82)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_RequireApiKey_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_RequireApiKey"></a>

### RequireApiKey


```csharp
public bool RequireApiKey { get; set; }
```


When true, a missing API key fails the call. The generic provider leaves this unset.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L115)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_RerankPath_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_RerankPath"></a>

### RerankPath


```csharp
public string RerankPath { get; set; }
```


Relative rerank path.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L160)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ResolveReasoningEffort_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_ResolveReasoningEffort"></a>

### ResolveReasoningEffort


```csharp
public Func<string, string?, string?, IList<CallWarning>, string?>? ResolveReasoningEffort { get; set; }
```


Resolves `reasoning_effort` for a model. Arguments are the model id, the portable `Reasoning` level,
the provider option effort, and the call warnings. Null removes the field. Null here keeps the default behavior.

**Value:** [Func&lt;string, string, string, IList&lt;CallWarning&gt;, string&gt;](https://learn.microsoft.com/dotnet/api/system.func-5)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L148)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_SelectErrorMessage_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_SelectErrorMessage"></a>

### SelectErrorMessage


```csharp
public Func<string?, string?>? SelectErrorMessage { get; set; }
```


Rewrites a provider error body into the exception message.

**Value:** [Func&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L121)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_SupportsEmbeddings_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_SupportsEmbeddings"></a>

### SupportsEmbeddings


```csharp
public bool SupportsEmbeddings { get; set; }
```


Whether embedding calls are supported.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L67)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_SupportsImages_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_SupportsImages"></a>

### SupportsImages


```csharp
public bool SupportsImages { get; set; }
```


Whether [ImageModel(string)](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/#Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_ImageModel_System_String_) is supported.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L70)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_SupportsMultiPartToolContent_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_SupportsMultiPartToolContent"></a>

### SupportsMultiPartToolContent


```csharp
public bool SupportsMultiPartToolContent { get; set; }
```


When true, a tool result with [OutputContent](/api/Vercel.AI.Provider/ToolModelMessage/#Vercel_AI_Provider_ToolModelMessage_OutputContent) is sent as an array of content parts. Otherwise the parts are sent as a JSON string.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L91)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_SupportsStructuredOutputs_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_SupportsStructuredOutputs"></a>

### SupportsStructuredOutputs


```csharp
public bool SupportsStructuredOutputs { get; set; }
```


When true, a JSON schema is sent as `json_schema` instead of `json_object`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L88)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_TransformRequestBody_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_TransformRequestBody"></a>

### TransformRequestBody


```csharp
public Func<JsonObject, IList<CallWarning>, JsonObject>? TransformRequestBody { get; set; }
```


Rewrites the JSON body before it is sent. Warnings raised here are returned with the call.

**Value:** [Func&lt;JsonObject, IList&lt;CallWarning&gt;, JsonObject&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L124)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_UserAgent_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_UserAgent"></a>

### UserAgent


```csharp
public string? UserAgent { get; set; }
```


User-agent suffix. Empty uses [UserAgent(string)](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleInfo/#Vercel_AI_OpenAICompatible_OpenAICompatibleInfo_UserAgent_System_String_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L118)

## Methods

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_Copy_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_Copy"></a>

### Copy()


```csharp
public OpenAICompatibleOptions Copy()
```


Returns a copy. Providers change the copy, so the options the caller passed in are left as they were.

**Returns:** [OpenAICompatibleOptions](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleOptions.cs#L177)
