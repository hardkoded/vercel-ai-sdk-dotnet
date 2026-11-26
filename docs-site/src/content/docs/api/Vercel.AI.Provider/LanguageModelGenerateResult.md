---
title: "LanguageModelGenerateResult"
description: "Result of CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "LanguageModelGenerateResult"
  hidden: true
---

<a id="Vercel_AI_Provider_LanguageModelGenerateResult"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class LanguageModelGenerateResult
```


Result of `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2462)

## Constructors

<a id="Vercel_AI_Provider_LanguageModelGenerateResult__ctor_"></a>
<a id="Vercel_AI_Provider_LanguageModelGenerateResult__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_GeneratedContent__Vercel_AI_Provider_FinishReason_Vercel_AI_Provider_LanguageModelUsage_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_CallWarning__System_String_System_Nullable_System_Text_Json_JsonElement__System_String_System_String_System_Nullable_System_DateTimeOffset__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_String_Vercel_AI_Provider_LanguageModelStreamRequest_"></a>

### `LanguageModelGenerateResult(IReadOnlyList<GeneratedContent>, FinishReason, LanguageModelUsage, string?, IReadOnlyList<CallWarning>?, string?, JsonElement?, string?, string?, DateTimeOffset?, IReadOnlyDictionary<string, string>?, string?, LanguageModelStreamRequest?)`


```csharp
public LanguageModelGenerateResult(IReadOnlyList<GeneratedContent> content, FinishReason finishReason, LanguageModelUsage usage, string? rawFinishReason = null, IReadOnlyList<CallWarning>? warnings = null, string? responseId = null, JsonElement? providerMetadata = null, string? rawResponse = null, string? responseModelId = null, DateTimeOffset? responseTimestamp = null, IReadOnlyDictionary<string, string>? responseHeaders = null, string? structuredOutputText = null, LanguageModelStreamRequest? request = null)
```


Creates a generate result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `content` | [IReadOnlyList&lt;GeneratedContent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `finishReason` | [FinishReason](/api/Vercel.AI.Provider/FinishReason/) |  |
| `usage` | [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/) |  |
| `rawFinishReason` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `warnings` | [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `responseId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `rawResponse` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `responseModelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `responseTimestamp` | [DateTimeOffset?](https://learn.microsoft.com/dotnet/api/system.datetimeoffset) |  |
| `responseHeaders` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `structuredOutputText` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `request` | [LanguageModelStreamRequest](/api/Vercel.AI.Provider/LanguageModelStreamRequest/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2465)

## Properties

<a id="Vercel_AI_Provider_LanguageModelGenerateResult_Content_"></a>
<a id="Vercel_AI_Provider_LanguageModelGenerateResult_Content"></a>

### Content


```csharp
public IReadOnlyList<GeneratedContent> Content { get; }
```


Generated content parts.

**Value:** [IReadOnlyList&lt;GeneratedContent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2497)

<a id="Vercel_AI_Provider_LanguageModelGenerateResult_FinishReason_"></a>
<a id="Vercel_AI_Provider_LanguageModelGenerateResult_FinishReason"></a>

### FinishReason


```csharp
public FinishReason FinishReason { get; }
```


Normalized finish reason.

**Value:** [FinishReason](/api/Vercel.AI.Provider/FinishReason/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2607)

<a id="Vercel_AI_Provider_LanguageModelGenerateResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_LanguageModelGenerateResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this call, keyed by provider name.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2622)

<a id="Vercel_AI_Provider_LanguageModelGenerateResult_RawFinishReason_"></a>
<a id="Vercel_AI_Provider_LanguageModelGenerateResult_RawFinishReason"></a>

### RawFinishReason


```csharp
public string? RawFinishReason { get; }
```


Provider finish reason, when it differs from [FinishReason](#Vercel_AI_Provider_LanguageModelGenerateResult_FinishReason).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2610)

<a id="Vercel_AI_Provider_LanguageModelGenerateResult_RawResponse_"></a>
<a id="Vercel_AI_Provider_LanguageModelGenerateResult_RawResponse"></a>

### RawResponse


```csharp
public string? RawResponse { get; }
```


Raw provider response body, when the model captured it.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2625)

<a id="Vercel_AI_Provider_LanguageModelGenerateResult_RedactRawResponse_"></a>
<a id="Vercel_AI_Provider_LanguageModelGenerateResult_RedactRawResponse"></a>

### RedactRawResponse


```csharp
public Func<JsonElement, JsonElement>? RedactRawResponse { get; }
```


Provider-owned telemetry view of [RawResponse](#Vercel_AI_Provider_LanguageModelGenerateResult_RawResponse), which hides credentials. Maps to the annotation of `withTelemetryRedaction` on the response body.

**Value:** [Func&lt;JsonElement, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2628)

<a id="Vercel_AI_Provider_LanguageModelGenerateResult_Request_"></a>
<a id="Vercel_AI_Provider_LanguageModelGenerateResult_Request"></a>

### Request


```csharp
public LanguageModelStreamRequest? Request { get; }
```


Request info, when the model reports it. Maps to the `request` field of `doGenerate`.

**Value:** [LanguageModelStreamRequest](/api/Vercel.AI.Provider/LanguageModelStreamRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2604)

<a id="Vercel_AI_Provider_LanguageModelGenerateResult_ResponseHeaders_"></a>
<a id="Vercel_AI_Provider_LanguageModelGenerateResult_ResponseHeaders"></a>

### ResponseHeaders


```csharp
public IReadOnlyDictionary<string, string> ResponseHeaders { get; }
```


HTTP response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2662)

<a id="Vercel_AI_Provider_LanguageModelGenerateResult_ResponseId_"></a>
<a id="Vercel_AI_Provider_LanguageModelGenerateResult_ResponseId"></a>

### ResponseId


```csharp
public string? ResponseId { get; }
```


Provider response id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2619)

<a id="Vercel_AI_Provider_LanguageModelGenerateResult_ResponseModelId_"></a>
<a id="Vercel_AI_Provider_LanguageModelGenerateResult_ResponseModelId"></a>

### ResponseModelId


```csharp
public string? ResponseModelId { get; }
```


Model id reported by the provider.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2656)

<a id="Vercel_AI_Provider_LanguageModelGenerateResult_ResponseTimestamp_"></a>
<a id="Vercel_AI_Provider_LanguageModelGenerateResult_ResponseTimestamp"></a>

### ResponseTimestamp


```csharp
public DateTimeOffset? ResponseTimestamp { get; }
```


Provider timestamp for the response.

**Value:** [DateTimeOffset?](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2659)

<a id="Vercel_AI_Provider_LanguageModelGenerateResult_StructuredOutputText_"></a>
<a id="Vercel_AI_Provider_LanguageModelGenerateResult_StructuredOutputText"></a>

### StructuredOutputText


```csharp
public string? StructuredOutputText { get; }
```


Text to parse for structured JSON output when it differs from the concatenation of all text parts in
[Content](#Vercel_AI_Provider_LanguageModelGenerateResult_Content), such as drafts followed by a final answer. It does not replace or remove any content.
When null, consumers concatenate all text parts. An empty string is an explicit selection and does not fall
back to the text parts. Maps to `structuredOutputText`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2505)

<a id="Vercel_AI_Provider_LanguageModelGenerateResult_Text_"></a>
<a id="Vercel_AI_Provider_LanguageModelGenerateResult_Text"></a>

### Text


```csharp
public string Text { get; }
```


Concatenated text parts.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2665)

<a id="Vercel_AI_Provider_LanguageModelGenerateResult_Usage_"></a>
<a id="Vercel_AI_Provider_LanguageModelGenerateResult_Usage"></a>

### Usage


```csharp
public LanguageModelUsage Usage { get; }
```


Token usage.

**Value:** [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2613)

<a id="Vercel_AI_Provider_LanguageModelGenerateResult_Warnings_"></a>
<a id="Vercel_AI_Provider_LanguageModelGenerateResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<CallWarning> Warnings { get; }
```


Call warnings.

**Value:** [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2616)

## Methods

<a id="Vercel_AI_Provider_LanguageModelGenerateResult_RedactTelemetryRawResponse_"></a>
<a id="Vercel_AI_Provider_LanguageModelGenerateResult_RedactTelemetryRawResponse"></a>

### RedactTelemetryRawResponse()


```csharp
public JsonElement? RedactTelemetryRawResponse()
```


Applies [RedactRawResponse](#Vercel_AI_Provider_LanguageModelGenerateResult_RedactRawResponse) to [RawResponse](#Vercel_AI_Provider_LanguageModelGenerateResult_RawResponse) for telemetry, leaving [RawResponse](#Vercel_AI_Provider_LanguageModelGenerateResult_RawResponse) unchanged. Returns null when there is no JSON
body or the redactor throws. Maps to `redactTelemetryValue`.

**Returns:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2634)
