---
title: "OpenAICompatibleChat"
description: "Shared OpenAI-compatible chat request and response helpers."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICompatibleChat"
  hidden: true
---

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat"></a>

Namespace: [Vercel.AI.OpenAICompatible](/api/Vercel.AI.OpenAICompatible/)


```csharp
public static class OpenAICompatibleChat
```


Shared OpenAI-compatible chat request and response helpers.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L14)

## Methods

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_BaseProviderName_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_BaseProviderName_System_String_"></a>

### BaseProviderName(string?)


```csharp
public static string BaseProviderName(string? providerId)
```


First segment of a provider id. `anthropic.beta` is `anthropic`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L28)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_ConvertCompletionUsage_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_ConvertCompletionUsage_System_Nullable_System_Text_Json_JsonElement__"></a>

### ConvertCompletionUsage(JsonElement?)


```csharp
public static LanguageModelUsage ConvertCompletionUsage(JsonElement? usage)
```


Maps a Completions usage object. A missing object leaves token counts unset.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `usage` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L147)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_ConvertContent_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_ConvertContent_System_Text_Json_JsonElement_"></a>

### ConvertContent(JsonElement)


```csharp
public static List<OpenAICompatibleContentPart> ConvertContent(JsonElement content)
```


Converts text and thinking content. Unknown parts are ignored.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `content` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [List&lt;OpenAICompatibleContentPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L254)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_ConvertMessages_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_ConvertMessages_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__System_String_System_String_System_Boolean_"></a>

### `ConvertMessages(IReadOnlyList<ModelMessage>, string, string?, bool)`


```csharp
public static JsonArray ConvertMessages(IReadOnlyList<ModelMessage> prompt, string metadataKey, string? providerName = null, bool supportsMultiPartToolContent = false)
```


Converts prompt messages to Chat Completions messages.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `metadataKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `supportsMultiPartToolContent` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [JsonArray](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonarray)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L214)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_ConvertUsage_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_ConvertUsage_System_Nullable_System_Text_Json_JsonElement__"></a>

### ConvertUsage(JsonElement?)


```csharp
public static OpenAICompatibleUsage ConvertUsage(JsonElement? usage)
```


Maps a Chat Completions usage object. A missing object leaves token counts unset.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `usage` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [OpenAICompatibleUsage](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L119)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_HasOptions_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_HasOptions_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__System_String_"></a>

### `HasOptions(IReadOnlyDictionary<string, JsonElement>?, string)`


```csharp
public static bool HasOptions(IReadOnlyDictionary<string, JsonElement>? providerOptions, string key)
```


True when &lt;code class="paramref"&gt;key&lt;/code&gt; is present and not JSON null.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `key` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L108)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_IsCustomReasoning_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_IsCustomReasoning_System_String_"></a>

### IsCustomReasoning(string?)


```csharp
public static bool IsCustomReasoning(string? reasoning)
```


Top-level reasoning values other than `provider-default` are sent as `reasoning_effort`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `reasoning` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L208)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_ProviderMetadata_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_ProviderMetadata_System_String_System_Nullable_System_Int32__System_Nullable_System_Int32__"></a>

### ProviderMetadata(string, int?, int?)


```csharp
public static JsonElement ProviderMetadata(string metadataKey, int? acceptedPredictionTokens, int? rejectedPredictionTokens)
```


Builds provider metadata for accepted and rejected prediction tokens.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `metadataKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `acceptedPredictionTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `rejectedPredictionTokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L315)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_Qualify_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_Qualify_System_String_System_String_"></a>

### Qualify(string?, string)


```csharp
public static string Qualify(string? providerName, string kind)
```


Provider id for a model kind, such as `groq.chat`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `kind` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L17)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_ReadErrorMessage_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_ReadErrorMessage_System_String_"></a>

### ReadErrorMessage(string?)


```csharp
public static string? ReadErrorMessage(string? body)
```


Reads `error.message` or a string `error` without appending `param`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L166)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_ResolveProviderOptionsKey_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_ResolveProviderOptionsKey_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__"></a>

### `ResolveProviderOptionsKey(string, IReadOnlyDictionary<string, JsonElement>?)`


```csharp
public static string ResolveProviderOptionsKey(string rawName, IReadOnlyDictionary<string, JsonElement>? providerOptions)
```


Prefers the camelCase provider-options key when the caller supplied it.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `rawName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L82)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_ThoughtSignatureMetadata_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_ThoughtSignatureMetadata_System_String_System_String_"></a>

### ThoughtSignatureMetadata(string, string)


```csharp
public static JsonElement ThoughtSignatureMetadata(string metadataKey, string thoughtSignature)
```


Builds tool-call metadata that carries a Google thought signature.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `metadataKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `thoughtSignature` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L332)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_ToCamelCase_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_ToCamelCase_System_String_"></a>

### ToCamelCase(string)


```csharp
public static string ToCamelCase(string value)
```


Converts `provider-name` and `provider_name` to `providerName`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L40)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_Unsupported_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_Unsupported_System_String_"></a>

### Unsupported(string)


```csharp
public static AiSdkException Unsupported(string functionality)
```


Throws when a file part cannot be represented on this API.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `functionality` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L339)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_WarnIfDeprecated_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_WarnIfDeprecated_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__System_Collections_Generic_IList_Vercel_AI_Provider_CallWarning__"></a>

### `WarnIfDeprecated(string, IReadOnlyDictionary<string, JsonElement>?, IList<CallWarning>)`


```csharp
public static void WarnIfDeprecated(string rawName, IReadOnlyDictionary<string, JsonElement>? providerOptions, IList<CallWarning> warnings)
```


Adds a deprecation warning when &lt;code class="paramref"&gt;providerOptions&lt;/code&gt; uses a non-camelCase key.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `rawName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `warnings` | [IList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L96)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_WithMessage_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleChat_WithMessage_Vercel_AI_Provider_ApiException_System_String_"></a>

### WithMessage(ApiException, string)


```csharp
public static ApiException WithMessage(ApiException exception, string message)
```


Copies an API exception onto the same status type with a new message.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `exception` | [ApiException](/api/Vercel.AI.Provider/ApiException/) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ApiException](/api/Vercel.AI.Provider/ApiException/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleChat.cs#L202)
