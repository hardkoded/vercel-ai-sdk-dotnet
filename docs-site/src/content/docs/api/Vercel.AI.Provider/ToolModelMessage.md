---
title: "ToolModelMessage"
description: "A tool result fed back to the model."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolModelMessage"
  hidden: true
---

<a id="Vercel_AI_Provider_ToolModelMessage"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ToolModelMessage : ModelMessage
```


A tool result fed back to the model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ModelMessage](/api/Vercel.AI.Provider/ModelMessage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1191)

## Constructors

<a id="Vercel_AI_Provider_ToolModelMessage__ctor_"></a>
<a id="Vercel_AI_Provider_ToolModelMessage__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ToolModelMessage__"></a>

### `ToolModelMessage(IReadOnlyList<ToolModelMessage>)`


```csharp
public ToolModelMessage(IReadOnlyList<ToolModelMessage> results)
```


Creates one tool message that carries several tool results, as upstream does for the calls of one step.
Its single-result members describe the first result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `results` | [IReadOnlyList&lt;ToolModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1248)

<a id="Vercel_AI_Provider_ToolModelMessage__ctor_System_String_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_UserContentPart__System_Nullable_System_Text_Json_JsonElement__"></a>

### `ToolModelMessage(string, string, IReadOnlyList<UserContentPart>, JsonElement?)`


```csharp
public ToolModelMessage(string toolCallId, string toolName, IReadOnlyList<UserContentPart> content, JsonElement? providerMetadata = null)
```


Creates a tool result whose output is a list of text and file parts, as upstream `output: { type: 'content' }`.
[OutputType](#Vercel_AI_Provider_ToolModelMessage_OutputType) is `json` and [OutputJson](#Vercel_AI_Provider_ToolModelMessage_OutputJson) is the JSON text of the parts, so a provider that
does not read [OutputContent](#Vercel_AI_Provider_ToolModelMessage_OutputContent) sends the stringified parts, as upstream does by default.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `content` | [IReadOnlyList&lt;UserContentPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1233)

<a id="Vercel_AI_Provider_ToolModelMessage__ctor_System_String_System_String_System_String_System_Boolean_System_Nullable_System_Text_Json_JsonElement__"></a>

### ToolModelMessage(string, string, string, bool, JsonElement?)


```csharp
public ToolModelMessage(string toolCallId, string toolName, string outputJson, bool isError, JsonElement? providerMetadata = null)
```


Creates a tool result message. The output type comes from &lt;code class="paramref"&gt;isError&lt;/code&gt; and from whether &lt;code class="paramref"&gt;outputJson&lt;/code&gt; is valid JSON.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `outputJson` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `isError` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1196)

<a id="Vercel_AI_Provider_ToolModelMessage__ctor_System_String_System_String_System_String_System_Boolean_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### ToolModelMessage(string, string, string?, bool, string?, JsonElement?)


```csharp
public ToolModelMessage(string toolCallId, string toolName, string? outputJson, bool isError, string? outputType, JsonElement? providerMetadata = null)
```


Creates a tool result message. &lt;code class="paramref"&gt;outputType&lt;/code&gt; is `text`, `json`, `error-text`, `error-json`,
or `execution-denied`. When it is null, the type comes from &lt;code class="paramref"&gt;isError&lt;/code&gt; and from whether
&lt;code class="paramref"&gt;outputJson&lt;/code&gt; is valid JSON. When it is set, it decides [IsError](#Vercel_AI_Provider_ToolModelMessage_IsError) and &lt;code class="paramref"&gt;isError&lt;/code&gt; is ignored.
For `execution-denied`, &lt;code class="paramref"&gt;outputJson&lt;/code&gt; is the reason, or null for none.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `outputJson` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `isError` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `outputType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1207)

## Properties

<a id="Vercel_AI_Provider_ToolModelMessage_IsError_"></a>
<a id="Vercel_AI_Provider_ToolModelMessage_IsError"></a>

### IsError


```csharp
public bool IsError { get; }
```


Whether the tool failed.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1327)

<a id="Vercel_AI_Provider_ToolModelMessage_MessageProviderOptions_"></a>
<a id="Vercel_AI_Provider_ToolModelMessage_MessageProviderOptions"></a>

### MessageProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? MessageProviderOptions { get; init; }
```


Provider options of the whole tool message, such as an Amazon Bedrock `cachePoint` added after all its results.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1342)

<a id="Vercel_AI_Provider_ToolModelMessage_OutputContent_"></a>
<a id="Vercel_AI_Provider_ToolModelMessage_OutputContent"></a>

### OutputContent


```csharp
public IReadOnlyList<UserContentPart>? OutputContent { get; }
```


Text and file parts of a multi-part output, or null for a single value. [OutputJson](#Vercel_AI_Provider_ToolModelMessage_OutputJson) holds the same parts as JSON text.

**Value:** [IReadOnlyList&lt;UserContentPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1324)

<a id="Vercel_AI_Provider_ToolModelMessage_OutputJson_"></a>
<a id="Vercel_AI_Provider_ToolModelMessage_OutputJson"></a>

### OutputJson


```csharp
public string OutputJson { get; }
```


JSON result. For `text` and `error-text` it is the text itself. For `execution-denied` it is the reason, or empty for none.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1321)

<a id="Vercel_AI_Provider_ToolModelMessage_OutputProviderOptions_"></a>
<a id="Vercel_AI_Provider_ToolModelMessage_OutputProviderOptions"></a>

### OutputProviderOptions


```csharp
public JsonElement? OutputProviderOptions { get; init; }
```


Provider options set on the tool output itself rather than on the tool result, such as an OpenAI `promptCacheBreakpoint`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1339)

<a id="Vercel_AI_Provider_ToolModelMessage_OutputType_"></a>
<a id="Vercel_AI_Provider_ToolModelMessage_OutputType"></a>

### OutputType


```csharp
public string OutputType { get; }
```


Output type: `text`, `json`, `error-text`, `error-json`, or `execution-denied`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1330)

<a id="Vercel_AI_Provider_ToolModelMessage_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_ToolModelMessage_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata carried on this tool result, such as a thought signature.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1333)

<a id="Vercel_AI_Provider_ToolModelMessage_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_ToolModelMessage_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
```


Provider options set on this tool result, such as an Amazon Bedrock `cachePoint`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1336)

<a id="Vercel_AI_Provider_ToolModelMessage_Results_"></a>
<a id="Vercel_AI_Provider_ToolModelMessage_Results"></a>

### Results


```csharp
public IReadOnlyList<ToolModelMessage> Results { get; }
```


The tool results this message carries, one per executed call. A single-result message lists itself.

**Value:** [IReadOnlyList&lt;ToolModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1278)

<a id="Vercel_AI_Provider_ToolModelMessage_ToolCallId_"></a>
<a id="Vercel_AI_Provider_ToolModelMessage_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Matching tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1315)

<a id="Vercel_AI_Provider_ToolModelMessage_ToolName_"></a>
<a id="Vercel_AI_Provider_ToolModelMessage_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1318)

## Methods

<a id="Vercel_AI_Provider_ToolModelMessage_Flatten_"></a>
<a id="Vercel_AI_Provider_ToolModelMessage_Flatten_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__"></a>

### `Flatten(IReadOnlyList<ModelMessage>)`


```csharp
public static IReadOnlyList<ModelMessage> Flatten(IReadOnlyList<ModelMessage> messages)
```


Splits grouped tool messages into one message per result, in order. Other messages pass through.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1281)
