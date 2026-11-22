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
| [BinaryFieldPaths](/api/Vercel.AI.ProviderUtils/BinaryFieldPaths/) | The compiled paths, a trie that is walked while scanning. The edges of a node are the bytes of an object key (a key ends with its closing quote), the items of an array, and "a path ends here". |
| [ByteEncoding](/api/Vercel.AI.ProviderUtils/ByteEncoding/) | Base64 conversions for inline file bytes. |
| [ConnectToWebSocket](/api/Vercel.AI.ProviderUtils/ConnectToWebSocket/) | Opens a WebSocket for a provider model, owning the transport-generic layer: factory resolution, header hygiene, abort wiring, and message decoding. Maps to connectToWebSocket. |
| [ConnectToWebSocketOptions](/api/Vercel.AI.ProviderUtils/ConnectToWebSocketOptions/) | Options for Connect(ConnectToWebSocketOptions). |
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
| [JsonBinaryFieldScanner](/api/Vercel.AI.ProviderUtils/JsonBinaryFieldScanner/) | Streaming extraction of large base64 strings from a JSON response body. A JSON response that carries an image or a file is mostly one very long base64 string. The scanner reads the body chunk by chunk: strings at the configured paths are decoded to bytes while they stream in, and the text that is handed to the JSON parser (the "skeleton") holds a short placeholder in their place. A string is extracted only if it is canonical standard-alphabet base64 (length divisible by 4, = only as the final padding, zero trailing bits, no escape sequences or whitespace). Everything else stays a string. Maps to json-binary-field-scanner.ts. |
| [JsonBinaryFieldScanResult](/api/Vercel.AI.ProviderUtils/JsonBinaryFieldScanResult/) | The body with extracted strings replaced by placeholders, and the extracted bytes. |
| [JsonInstructions](/api/Vercel.AI.ProviderUtils/JsonInstructions/) | Appends a JSON schema instruction to a prompt or to the first system message. |
| [JsonParseException](/api/Vercel.AI.ProviderUtils/JsonParseException/) | JSON text could not be parsed. |
| [JsonParseResult](/api/Vercel.AI.ProviderUtils/JsonParseResult/) | Success or failure of a JSON parse, including the raw value when parsing succeeded. |
| [JsonParsing](/api/Vercel.AI.ProviderUtils/JsonParsing/) | Parses JSON text, optionally checking it against a JSON Schema document. |
| [JsonResponseHandlerResult&lt;T&gt;](/api/Vercel.AI.ProviderUtils/JsonResponseHandlerResult-1/) | A handled JSON value, the parsed body before validation, and the response headers. |
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
| [ProviderOptionsResolver](/api/Vercel.AI.ProviderUtils/ProviderOptionsResolver/) | Resolves options that a model accepts under more than one providerOptions namespace. |
| [ProviderReferences](/api/Vercel.AI.ProviderUtils/ProviderReferences/) | Helpers for provider references: maps from provider name to a provider-specific id. |
| [ProviderTextResponse](/api/Vercel.AI.ProviderUtils/ProviderTextResponse/) | A successful JSON response body and its headers. |
| [ProviderValues](/api/Vercel.AI.ProviderUtils/ProviderValues/) | Small collection, header, text, and media helpers shared by providers. |
| [ProviderValues.BatchRequestCounts](/api/Vercel.AI.ProviderUtils/ProviderValues.BatchRequestCounts/) | Complete batch request counts. |
| [ProviderValues.RuntimeProbe](/api/Vercel.AI.ProviderUtils/ProviderValues.RuntimeProbe/) | Inputs for GetRuntimeEnvironmentUserAgent(RuntimeProbe). |
| [ReasoningMap](/api/Vercel.AI.ProviderUtils/ReasoningMap/) | Maps reasoning levels onto provider effort and token budgets. |
| [Resolvable&lt;T&gt;](/api/Vercel.AI.ProviderUtils/Resolvable-1/) | A value, a task, or a synchronous or asynchronous callback that produces the value. Maps to the upstream Resolvable<T> of @ai-sdk/provider-utils. A callback is called every time the value is resolved, so a provider setting such as an API key is resolved per request. |
| [ResolvedProviderOptions&lt;T&gt;](/api/Vercel.AI.ProviderUtils/ResolvedProviderOptions-1/) | Result of Func%7b%60%600%2c%60%600%2c%60%600%7d). |
| [ResponseBodies](/api/Vercel.AI.ProviderUtils/ResponseBodies/) | Reads and releases HTTP response bodies. |
| [ResponseHandlerResult&lt;T&gt;](/api/Vercel.AI.ProviderUtils/ResponseHandlerResult-1/) | A handled response value with the response headers. |
| [ResponseHandlers](/api/Vercel.AI.ProviderUtils/ResponseHandlers/) | Response handlers for binary, event stream, and error responses. Maps to createBinaryResponseHandler, createBinaryStreamResponseHandler, createEventSourceResponseHandler, createJsonErrorResponseHandler, and createStatusCodeErrorResponseHandler. |
| [RetryException](/api/Vercel.AI.ProviderUtils/RetryException/) | Failure of ExponentialBackoff after at least one retry. |
| [RetryOptions](/api/Vercel.AI.ProviderUtils/RetryOptions/) | Options for ExponentialBackoff. |
| [RetryPolicy](/api/Vercel.AI.ProviderUtils/RetryPolicy/) | Retry policy aligned with the AI SDK defaults: 2 retries, 500–5000ms backoff, 25% jitter. Retryable statuses are 408, 429, and 500–599. Retry-After is honored up to 60 seconds. |
| [SandboxDeleteFileOptions](/api/Vercel.AI.ProviderUtils/SandboxDeleteFileOptions/) | Options of DeleteFileAsync(SandboxDeleteFileOptions). Maps to SandboxDeleteFileOptions. |
| [SandboxDirectoryEntry](/api/Vercel.AI.ProviderUtils/SandboxDirectoryEntry/) | Entry in a directory listing. Maps to SandboxDirectoryEntry. |
| [SandboxFileStat](/api/Vercel.AI.ProviderUtils/SandboxFileStat/) | Information about a file or directory in the sandbox. Symbolic links are followed. Maps to SandboxFileStat. |
| [SandboxListDirectoryOptions](/api/Vercel.AI.ProviderUtils/SandboxListDirectoryOptions/) | Options of ListDirectoryAsync(SandboxListDirectoryOptions). Maps to SandboxListDirectoryOptions. |
| [SandboxPathOptions](/api/Vercel.AI.ProviderUtils/SandboxPathOptions/) | Options for reading, inspecting, or deleting one path in the sandbox. Maps to PathOptions. |
| [SandboxProcessOptions](/api/Vercel.AI.ProviderUtils/SandboxProcessOptions/) | Options for executing a command in the sandbox with run or spawn. Maps to SandboxProcessOptions. |
| [SandboxReadBinaryFileOptions](/api/Vercel.AI.ProviderUtils/SandboxReadBinaryFileOptions/) | Options of ReadBinaryFileAsync(SandboxReadBinaryFileOptions). Maps to SandboxReadBinaryFileOptions. |
| [SandboxReadFileOptions](/api/Vercel.AI.ProviderUtils/SandboxReadFileOptions/) | Options of ReadFileAsync(SandboxReadFileOptions). Maps to SandboxReadFileOptions. |
| [SandboxReadTextFileOptions](/api/Vercel.AI.ProviderUtils/SandboxReadTextFileOptions/) | Options of ReadTextFileAsync(SandboxReadTextFileOptions). Maps to SandboxReadTextFileOptions. |
| [SandboxRunOptions](/api/Vercel.AI.ProviderUtils/SandboxRunOptions/) | Options of RunAsync(SandboxRunOptions). Maps to SandboxRunOptions. |
| [SandboxRunResult](/api/Vercel.AI.ProviderUtils/SandboxRunResult/) | The result of RunAsync(SandboxRunOptions). Maps to the result of run. |
| [SandboxSpawnOptions](/api/Vercel.AI.ProviderUtils/SandboxSpawnOptions/) | Options of SpawnAsync(SandboxSpawnOptions). Maps to SandboxSpawnOptions. |
| [SandboxStatOptions](/api/Vercel.AI.ProviderUtils/SandboxStatOptions/) | Options of StatAsync(SandboxStatOptions). Maps to SandboxStatOptions. |
| [SandboxWriteBinaryFileOptions](/api/Vercel.AI.ProviderUtils/SandboxWriteBinaryFileOptions/) | Options of WriteBinaryFileAsync(SandboxWriteBinaryFileOptions). Maps to SandboxWriteBinaryFileOptions. |
| [SandboxWriteFileOptions](/api/Vercel.AI.ProviderUtils/SandboxWriteFileOptions/) | Options of WriteFileAsync(SandboxWriteFileOptions). Maps to SandboxWriteFileOptions. |
| [SandboxWriteTextFileOptions](/api/Vercel.AI.ProviderUtils/SandboxWriteTextFileOptions/) | Options of WriteTextFileAsync(SandboxWriteTextFileOptions). Maps to SandboxWriteTextFileOptions. |
| [SecureJson](/api/Vercel.AI.ProviderUtils/SecureJson/) | Parses JSON and rejects proto and constructor.prototype payload keys. |
| [SerializationError](/api/Vercel.AI.ProviderUtils/SerializationError/) | A value could not be serialized. Maps to SerializationError. |
| [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/) | A serialized model id and config. Maps to the result of serializeModelOptions. |
| [SseParser](/api/Vercel.AI.ProviderUtils/SseParser/) | Reads Server-Sent Events data: lines. |
| [StringifyWithBigIntAndCyclesUtil](/api/Vercel.AI.ProviderUtils/StringifyWithBigIntAndCyclesUtil/) | Maps to stringify-with-bigint-and-cycles.ts. |
| [TelemetryRedactedValue](/api/Vercel.AI.ProviderUtils/TelemetryRedactedValue/) | A JSON value with the provider-owned telemetry view attached. The view is not part of the value, so the original is untouched. Maps to a value annotated by withTelemetryRedaction in @ai-sdk/provider-utils. |
| [TelemetryRedaction](/api/Vercel.AI.ProviderUtils/TelemetryRedaction/) | Provider-owned telemetry redaction. Maps to telemetry-redaction.ts in @ai-sdk/provider-utils. |
| [ToolExecution](/api/Vercel.AI.ProviderUtils/ToolExecution/) | Runs a tool delegate and reports preliminary and final outputs. |
| [ToolExecution.ToolStep&lt;T&gt;](/api/Vercel.AI.ProviderUtils/ToolExecution.ToolStep-1/) | One tool output. |
| [ToolNameMapping](/api/Vercel.AI.ProviderUtils/ToolNameMapping/) | Maps client tool names to provider tool names. Maps to ToolNameMapping. |
| [ToolNames](/api/Vercel.AI.ProviderUtils/ToolNames/) | Builds tool name maps. Maps to createToolNameMapping. |
| [TranscriptionStreamClientFrame](/api/Vercel.AI.ProviderUtils/TranscriptionStreamClientFrame/) | Server-side classification of a client text frame. Maps to TranscriptionStreamClientFrame. |
| [TranscriptionStreamEnvelope](/api/Vercel.AI.ProviderUtils/TranscriptionStreamEnvelope/) | Experimental transcription-stream WebSocket envelope (v1): the standard serialization of the transcription model doStream over a WebSocket. Maps to transcription-stream-envelope.ts. |
| [TranscriptionStreamStartFrame](/api/Vercel.AI.ProviderUtils/TranscriptionStreamStartFrame/) | The client's session start frame. Maps to TranscriptionStreamStartFrame. Optional keys are omitted when null. |
| [TypeValidationContext](/api/Vercel.AI.ProviderUtils/TypeValidationContext/) | Where a value failed type validation. Maps to TypeValidationContext. |
| [TypeValidationException](/api/Vercel.AI.ProviderUtils/TypeValidationException/) | A value did not match a JSON Schema. |
| [UrlSupport](/api/Vercel.AI.ProviderUtils/UrlSupport/) | Checks whether a model can read a URL natively. Maps to isUrlSupported. |
| [WebSocketCloseEvent](/api/Vercel.AI.ProviderUtils/WebSocketCloseEvent/) | Close diagnostics carried by OnClose. |
| [WebSocketConnection](/api/Vercel.AI.ProviderUtils/WebSocketConnection/) | A WebSocket opened by ConnectToWebSocket. |
| [WebSocketUtils](/api/Vercel.AI.ProviderUtils/WebSocketUtils/) | WebSocket helpers. Maps to provider-utils/src/websocket.ts. |

## Structs

| Type | Summary |
| --- | --- |
| [ProviderValues.OptionalHeader](/api/Vercel.AI.ProviderUtils/ProviderValues.OptionalHeader/) | Header values that may be omitted. |

## Interfaces

| Type | Summary |
| --- | --- |
| [ISandboxProcess](/api/Vercel.AI.ProviderUtils/ISandboxProcess/) | Handle to a long-running process started with SpawnAsync(SandboxSpawnOptions). Maps to SandboxProcess. |
| [ISandboxSession](/api/Vercel.AI.ProviderUtils/ISandboxSession/) | Sandbox session that can execute commands and read and write files. Relative paths in every method resolve against the same base directory: the directory that RunAsync(SandboxRunOptions) and SpawnAsync(SandboxSpawnOptions) start in when no working directory is given. Absolute paths are used as is. Maps to SandboxSession (ExperimentalSandboxSession). |
| [IWebSocketLike](/api/Vercel.AI.ProviderUtils/IWebSocketLike/) | The WebSocket surface used by providers. Maps to WebSocketLike. |

## Enums

| Type | Summary |
| --- | --- |
| [ProviderOptionsResolution](/api/Vercel.AI.ProviderUtils/ProviderOptionsResolution/) | How options found under several providerOptions namespaces are combined. Maps to ProviderOptionsResolution. |
| [RetryErrorReason](/api/Vercel.AI.ProviderUtils/RetryErrorReason/) | Why RetryException was thrown. |
| [SandboxDirectoryEntryType](/api/Vercel.AI.ProviderUtils/SandboxDirectoryEntryType/) | Type of an entry returned by ListDirectoryAsync(SandboxListDirectoryOptions). Maps to SandboxDirectoryEntry['type']. |
| [SandboxFileType](/api/Vercel.AI.ProviderUtils/SandboxFileType/) | Type of a path returned by StatAsync(SandboxStatOptions). Maps to SandboxFileStat['type']. |

## Delegates

| Type | Summary |
| --- | --- |
| [JsonResponseHandler&lt;T&gt;](/api/Vercel.AI.ProviderUtils/JsonResponseHandler-1/) | Handles a JSON response. Maps to the ResponseHandler that createJsonResponseHandler returns. |
| [WebSocketFactory](/api/Vercel.AI.ProviderUtils/WebSocketFactory/) | Creates a socket. Maps to WebSocketConstructor. |
