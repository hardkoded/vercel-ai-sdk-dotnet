---
title: "Vercel.AI.ProviderUtils"
description: "Public types in the `Vercel.AI.ProviderUtils` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.ProviderUtils"
---

[API reference](/api/)

Public types in the `Vercel.AI.ProviderUtils` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [AbortErrors](/api/Vercel.AI.ProviderUtils/AbortErrors/) | Recognizes abort and timeout failures that must not be retried. |
| [ApiKeys](/api/Vercel.AI.ProviderUtils/ApiKeys/) | Resolves an API key from an explicit value or an environment variable. |
| [BatchResultDownloads](/api/Vercel.AI.ProviderUtils/BatchResultDownloads/) | Settings for downloading JSON Lines batch results. |
| [BatchResultsApiBase](/api/Vercel.AI.ProviderUtils/BatchResultsApiBase/) | Batch API that downloads results. Start, status, cancel, and list throw unless a provider overrides them. |
| [ByteEncoding](/api/Vercel.AI.ProviderUtils/ByteEncoding/) | Base64 conversions for inline file bytes. |
| [Delay](/api/Vercel.AI.ProviderUtils/Delay/) | Waits for a duration, or completes immediately when the duration is null. Negative durations wait zero. |
| [DelayAbortedException](/api/Vercel.AI.ProviderUtils/DelayAbortedException/) | Raised when Delay is cancelled. Name is AbortError. |
| [DelayedPromise&lt;T&gt;](/api/Vercel.AI.ProviderUtils/DelayedPromise-1/) | A task that is completed from outside. Maps to DelayedPromise. |
| [EventSourceMessage](/api/Vercel.AI.ProviderUtils/EventSourceMessage/) | One Server-Sent Event. Field names are compared without case folding. |
| [EventSourceParser](/api/Vercel.AI.ProviderUtils/EventSourceParser/) | Incremental Server-Sent Events parser. A UTF-8 BOM is ignored at the start of the stream. A trailing carriage return is held until the next chunk so it can form a \r\n line ending. [DONE] is ordinary data. |
| [ExponentialBackoff](/api/Vercel.AI.ProviderUtils/ExponentialBackoff/) | Retries a failed operation with exponential backoff. Abort errors are rethrown. The first non-retryable error is rethrown without wrapping. |
| [FetchErrors](/api/Vercel.AI.ProviderUtils/FetchErrors/) | Turns transport failures into retryable ApiCallErrors. Maps to handleFetchError. |
| [FileDataConversions](/api/Vercel.AI.ProviderUtils/FileDataConversions/) | Converts file payloads to the forms providers send. |
| [HostnameParts](/api/Vercel.AI.ProviderUtils/HostnameParts/) | Checks a value before it is concatenated into a generated host. |
| [IdGenerator](/api/Vercel.AI.ProviderUtils/IdGenerator/) | Random id generator. The id is not cryptographically secure. A prefix is joined with separator. The separator must not appear in the alphabet. |
| [JsonInstructions](/api/Vercel.AI.ProviderUtils/JsonInstructions/) | Appends a JSON schema instruction to a prompt or to the first system message. |
| [JsonParseException](/api/Vercel.AI.ProviderUtils/JsonParseException/) | JSON text could not be parsed. |
| [JsonParseResult](/api/Vercel.AI.ProviderUtils/JsonParseResult/) | Success or failure of a JSON parse, including the raw value when parsing succeeded. |
| [JsonParsing](/api/Vercel.AI.ProviderUtils/JsonParsing/) | Parses JSON text, optionally checking it against a JSON Schema document. |
| [JsonResponseTools](/api/Vercel.AI.ProviderUtils/JsonResponseTools/) | Helpers for providers that cannot combine tools with native structured output. They add an internal function tool whose arguments are the structured output, then turn that tool call back into text. |
| [JsonSchemas](/api/Vercel.AI.ProviderUtils/JsonSchemas/) | Builds JSON Schema documents from explicit shapes. This is not a Zod converter. |
| [JsonSchemaValidator](/api/Vercel.AI.ProviderUtils/JsonSchemaValidator/) | Applies a JSON Schema document. Unknown object properties are dropped. |
| [JsonStreams](/api/Vercel.AI.ProviderUtils/JsonStreams/) | Reads JSON and Server-Sent Event bodies, including response headers. |
| [JsonStreams.JsonBody](/api/Vercel.AI.ProviderUtils/JsonStreams.JsonBody/) | A parsed JSON body and the headers that came with it. |
| [JsonValues](/api/Vercel.AI.ProviderUtils/JsonValues/) | Small JSON helpers shared by providers. |
| [MappedTool](/api/Vercel.AI.ProviderUtils/MappedTool/) | A tool passed to a language model when building a name map. |
| [MediaTypes](/api/Vercel.AI.ProviderUtils/MediaTypes/) | Detects IANA media types from file signatures. Maps to detectMediaType. |
| [ModelOptionsSerialization](/api/Vercel.AI.ProviderUtils/ModelOptionsSerialization/) | Serializes model options for a workflow boundary. Maps to serializeModelOptions. |
| [NamedException](/api/Vercel.AI.ProviderUtils/NamedException/) | An exception identified by a name such as AbortError or ResponseAborted. |
| [NoSuchProviderReferenceError](/api/Vercel.AI.ProviderUtils/NoSuchProviderReferenceError/) | A provider reference has no entry for the requested provider. Maps to NoSuchProviderReferenceError. |
| [PathSegments](/api/Vercel.AI.ProviderUtils/PathSegments/) | Encodes provider-returned identifiers for URL paths. |
| [ProviderHttp](/api/Vercel.AI.ProviderUtils/ProviderHttp/) | Sends JSON and SSE requests with the SDK retry policy. |
| [ProviderReferences](/api/Vercel.AI.ProviderUtils/ProviderReferences/) | Helpers for provider references: maps from provider name to a provider-specific id. |
| [ProviderTextResponse](/api/Vercel.AI.ProviderUtils/ProviderTextResponse/) | A successful JSON response body and its headers. |
| [ProviderValues](/api/Vercel.AI.ProviderUtils/ProviderValues/) | Small collection, header, text, and media helpers shared by providers. |
| [ProviderValues.BatchRequestCounts](/api/Vercel.AI.ProviderUtils/ProviderValues.BatchRequestCounts/) | Complete batch request counts. |
| [ProviderValues.RuntimeProbe](/api/Vercel.AI.ProviderUtils/ProviderValues.RuntimeProbe/) | Inputs for GetRuntimeEnvironmentUserAgent(RuntimeProbe). |
| [ReasoningMap](/api/Vercel.AI.ProviderUtils/ReasoningMap/) | Maps reasoning levels onto provider effort and token budgets. |
| [ResponseBodies](/api/Vercel.AI.ProviderUtils/ResponseBodies/) | Reads and releases HTTP response bodies. |
| [ResponseHandlerResult&lt;T&gt;](/api/Vercel.AI.ProviderUtils/ResponseHandlerResult-1/) | A handled response value with the response headers. |
| [ResponseHandlers](/api/Vercel.AI.ProviderUtils/ResponseHandlers/) | Response handlers for binary, event stream, and error responses. Maps to createBinaryResponseHandler, createBinaryStreamResponseHandler, createEventSourceResponseHandler, createJsonErrorResponseHandler, and createStatusCodeErrorResponseHandler. |
| [RetryException](/api/Vercel.AI.ProviderUtils/RetryException/) | Failure of ExponentialBackoff after at least one retry. |
| [RetryOptions](/api/Vercel.AI.ProviderUtils/RetryOptions/) | Options for ExponentialBackoff. |
| [RetryPolicy](/api/Vercel.AI.ProviderUtils/RetryPolicy/) | Retry policy aligned with the AI SDK defaults: 2 retries, 500–5000ms backoff, 25% jitter. Retryable statuses are 408, 429, and 500–599. Retry-After is honored up to 60 seconds. |
| [SecureJson](/api/Vercel.AI.ProviderUtils/SecureJson/) | Parses JSON and rejects proto and constructor.prototype payload keys. |
| [SerializationError](/api/Vercel.AI.ProviderUtils/SerializationError/) | A value could not be serialized. Maps to SerializationError. |
| [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/) | A serialized model id and config. Maps to the result of serializeModelOptions. |
| [SseParser](/api/Vercel.AI.ProviderUtils/SseParser/) | Reads Server-Sent Events data: lines. |
| [ToolExecution](/api/Vercel.AI.ProviderUtils/ToolExecution/) | Runs a tool delegate and reports preliminary and final outputs. |
| [ToolExecution.ToolStep&lt;T&gt;](/api/Vercel.AI.ProviderUtils/ToolExecution.ToolStep-1/) | One tool output. |
| [ToolNameMapping](/api/Vercel.AI.ProviderUtils/ToolNameMapping/) | Maps client tool names to provider tool names. Maps to ToolNameMapping. |
| [ToolNames](/api/Vercel.AI.ProviderUtils/ToolNames/) | Builds tool name maps. Maps to createToolNameMapping. |
| [TypeValidationException](/api/Vercel.AI.ProviderUtils/TypeValidationException/) | A value did not match a JSON Schema. |
| [UrlSupport](/api/Vercel.AI.ProviderUtils/UrlSupport/) | Checks whether a model can read a URL natively. Maps to isUrlSupported. |

## Structs

| Type | Summary |
| --- | --- |
| [ProviderValues.OptionalHeader](/api/Vercel.AI.ProviderUtils/ProviderValues.OptionalHeader/) | Header values that may be omitted. |

## Enums

| Type | Summary |
| --- | --- |
| [RetryErrorReason](/api/Vercel.AI.ProviderUtils/RetryErrorReason/) | Why RetryException was thrown. |
