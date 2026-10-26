---
title: "Vercel.AI.Util"
description: "Errors, downloads, warnings, and helpers."
editUrl: false
sidebar:
  label: "Vercel.AI.Util"
---

[API reference](/api/)

Errors, downloads, warnings, and helpers.

## Classes

| Type | Summary |
| --- | --- |
| [AbortController](/api/Vercel.AI.Util/AbortController/) | Controller for an AbortSignal. |
| [AbortSignal](/api/Vercel.AI.Util/AbortSignal/) | Abort signal that records the first reason. |
| [AiSdkError](/api/Vercel.AI.Util/AiSdkError/) | Base error for upstream AI SDK error types. Marker checks work without a shared assembly. |
| [ApiCallError](/api/Vercel.AI.Util/ApiCallError/) | A failed provider HTTP call, including retry metadata and response headers. |
| [AssetDownloadCall](/api/Vercel.AI.Util/AssetDownloadCall/) | Arguments passed to a single asset download. |
| [AsyncIterableStream&lt;T&gt;](/api/Vercel.AI.Util/AsyncIterableStream-1/) | A readable stream that can also be enumerated with await foreach. |
| [AsyncIterableStreams](/api/Vercel.AI.Util/AsyncIterableStreams/) | Wraps readable streams so they can be consumed with async iteration. |
| [Callbacks](/api/Vercel.AI.Util/Callbacks/) | Invokes callbacks and ignores individual failures. |
| [CanonicalHash](/api/Vercel.AI.Util/CanonicalHash/) | Canonical JSON and its SHA-256 base64url digest. |
| [Collections](/api/Vercel.AI.Util/Collections/) | Array and property helpers from the AI SDK util package. |
| [CompatibilityWarning](/api/Vercel.AI.Util/CompatibilityWarning/) | A feature running in compatibility mode. |
| [DataObject](/api/Vercel.AI.Util/DataObject/) | Object with an explicit constructor token and own properties. Used where JavaScript distinguishes prototypes and inherited names. |
| [DataUrl](/api/Vercel.AI.Util/DataUrl/) | Reads text from text/ data URLs. |
| [DecisionRefusalError](/api/Vercel.AI.Util/DecisionRefusalError/) | A decision model refused one or more questions. |
| [DecisionUnsupportedQuestionTypeError](/api/Vercel.AI.Util/DecisionUnsupportedQuestionTypeError/) | A decision model does not support the type of a requested question. |
| [DeepEqual](/api/Vercel.AI.Util/DeepEqual/) | Deep equality for JSON-like data, including dates and prototype identity. |
| [DefaultDownloadFunction](/api/Vercel.AI.Util/DefaultDownloadFunction/) | Default download function: downloads URLs the model cannot fetch itself. |
| [DeprecatedWarning](/api/Vercel.AI.Util/DeprecatedWarning/) | A deprecated setting. |
| [Deprecations](/api/Vercel.AI.Util/Deprecations/) | Stable codes for deprecation warnings. |
| [Download](/api/Vercel.AI.Util/Download/) | Downloads a URL with SSRF checks on every redirect hop. |
| [DownloadError](/api/Vercel.AI.Util/DownloadError/) | Failure while downloading a URL. |
| [DownloadRequest](/api/Vercel.AI.Util/DownloadRequest/) | Options passed to a download fetch implementation. |
| [DownloadRequestItem](/api/Vercel.AI.Util/DownloadRequestItem/) | One URL passed to DefaultDownloadFunction. |
| [DownloadResponse](/api/Vercel.AI.Util/DownloadResponse/) | One HTTP response seen by Download. |
| [DownloadResult](/api/Vercel.AI.Util/DownloadResult/) | Bytes and media type returned by Download.GetAsync. |
| [ErrorMarkerBag](/api/Vercel.AI.Util/ErrorMarkerBag/) | Plain marker bag used to recognize an error shape copied across packages. |
| [EvaluationUnsupportedQuestionTypeError](/api/Vercel.AI.Util/EvaluationUnsupportedQuestionTypeError/) | Deprecated alias of DecisionUnsupportedQuestionTypeError. It carries the decision error marker. |
| [FixJson](/api/Vercel.AI.Util/FixJson/) | Repairs truncated JSON by scanning once and closing open strings, literals, arrays, and objects. |
| [HeaderCollection](/api/Vercel.AI.Util/HeaderCollection/) | Case-insensitive header list that preserves repeated values such as Set-Cookie. |
| [IdMap&lt;T&gt;](/api/Vercel.AI.Util/IdMap-1/) | String-keyed map with no inherited members. Missing keys, including proto, return the default value. |
| [InvalidArgumentError](/api/Vercel.AI.Util/InvalidArgumentError/) | An argument failed validation. Message format matches the TypeScript SDK. |
| [InvalidToolInputError](/api/Vercel.AI.Util/InvalidToolInputError/) | The input of a tool call does not match the tool's input schema. Maps to InvalidToolInputError. |
| [JsUndefined](/api/Vercel.AI.Util/JsUndefined/) | Sentinel for a JavaScript undefined value. Distinct from null. |
| [LanguageModelCallOptionsInput](/api/Vercel.AI.Util/LanguageModelCallOptionsInput/) | Model call options validated by LanguageModelCallPreparation. |
| [LanguageModelCallPreparation](/api/Vercel.AI.Util/LanguageModelCallPreparation/) | Validates language-model call options. PrepareCallSettings(LanguageModelCallOptionsInput) is the deprecated alias. |
| [LogWarnings](/api/Vercel.AI.Util/LogWarnings/) | Logs provider warnings. Set Logger to false to suppress them, or to a delegate to replace the default emitter. |
| [LogWarningsOptions](/api/Vercel.AI.Util/LogWarningsOptions/) | Arguments passed to Log(LogWarningsOptions). |
| [MergeAbortSignals](/api/Vercel.AI.Util/MergeAbortSignals/) | Combines abort signals and timeout durations. |
| [ModelWarning](/api/Vercel.AI.Util/ModelWarning/) | A warning returned by a model provider. |
| [NoSuchModelError](/api/Vercel.AI.Util/NoSuchModelError/) | A provider has no model with the requested id. Maps to NoSuchModelError. |
| [NoSuchProviderError](/api/Vercel.AI.Util/NoSuchProviderError/) | A registry has no provider with the requested id. Maps to NoSuchProviderError. |
| [NoSuchToolError](/api/Vercel.AI.Util/NoSuchToolError/) | The model called a tool that is not available. Maps to NoSuchToolError. |
| [ObjectMerge](/api/Vercel.AI.Util/ObjectMerge/) | Deep-merges JSON objects. Arrays, dates, and regular expressions are replaced. |
| [OtherWarning](/api/Vercel.AI.Util/OtherWarning/) | A provider warning that is not one of the structured kinds. |
| [PartialJson](/api/Vercel.AI.Util/PartialJson/) | Parses complete JSON, or JSON repaired by FixJson. |
| [PartialJsonResult](/api/Vercel.AI.Util/PartialJsonResult/) | Outcome of Parse(string). |
| [PrepareHeaders](/api/Vercel.AI.Util/PrepareHeaders/) | Fills default headers without replacing headers the caller already set. |
| [PrepareRetries](/api/Vercel.AI.Util/PrepareRetries/) | Validates maxRetries and builds the default retry strategy. |
| [PrepareRetries.Result](/api/Vercel.AI.Util/PrepareRetries.Result/) | Result of Int32). |
| [ReadableStream&lt;T&gt;](/api/Vercel.AI.Util/ReadableStream-1/) | Pull-based readable stream. Supports enqueue, close, error, backpressure via pull, and a single reader. |
| [ReadableStreamController&lt;T&gt;](/api/Vercel.AI.Util/ReadableStreamController-1/) | Enqueues chunks into a ReadableStream%601. |
| [ReadableStreamReader&lt;T&gt;](/api/Vercel.AI.Util/ReadableStreamReader-1/) | Default reader for a ReadableStream%601. |
| [RetryError](/api/Vercel.AI.Util/RetryError/) | Retries were exhausted or stopped. |
| [RetryOptions](/api/Vercel.AI.Util/RetryOptions/) | Options for RetryWithExponentialBackoffRespectingRetryHeaders. |
| [RetryWithExponentialBackoffRespectingRetryHeaders](/api/Vercel.AI.Util/RetryWithExponentialBackoffRespectingRetryHeaders/) | Retries a failed operation with exponential backoff, honoring retry-after-ms and retry-after when the delay is between 0 and 60 seconds, or shorter than the backoff delay. |
| [SerialJobExecutor](/api/Vercel.AI.Util/SerialJobExecutor/) | Runs async jobs one at a time, in submission order. |
| [ServerResponse](/api/Vercel.AI.Util/ServerResponse/) | Byte sink with status, headers, and optional write backpressure. Mirrors the subset of Node ServerResponse used by the text-stream helpers. |
| [ServerResponseWriter](/api/Vercel.AI.Util/ServerResponseWriter/) | Writes a byte stream to a ServerResponse. |
| [SetAbortTimeout](/api/Vercel.AI.Util/SetAbortTimeout/) | Schedules an abort when a timeout elapses. |
| [SimulateReadableStream](/api/Vercel.AI.Util/SimulateReadableStream/) | Emits scripted chunks, optionally delaying each pull. |
| [StitchableStream&lt;T&gt;](/api/Vercel.AI.Util/StitchableStream-1/) | A stream that reads inner streams one at a time. |
| [StreamIterator&lt;T&gt;](/api/Vercel.AI.Util/StreamIterator-1/) | Async iterator over a ReadableStream%601. |
| [StreamProviderError](/api/Vercel.AI.Util/StreamProviderError/) | Error reported by a provider after a model response stream has started. |
| [TextStreamPart](/api/Vercel.AI.Util/TextStreamPart/) | One part of a model text stream. |
| [TextStreamResponse](/api/Vercel.AI.Util/TextStreamResponse/) | HTTP response whose body is a UTF-8 text stream. |
| [TextStreams](/api/Vercel.AI.Util/TextStreams/) | Text-stream helpers: keep text deltas, build a response, and write it to a server response. |
| [TimeoutError](/api/Vercel.AI.Util/TimeoutError/) | DOM TimeoutError analog used as an abort reason. |
| [ToolCallRepairError](/api/Vercel.AI.Util/ToolCallRepairError/) | The repair of a tool call failed. Maps to ToolCallRepairError. |
| [ToolInputSchemaValidator](/api/Vercel.AI.Util/ToolInputSchemaValidator/) | Validates a tool call input against the JSON Schema of a tool. Covers the keywords tool schemas use: type, enum, const, properties, required, additionalProperties, items, minItems, maxItems, minLength, maxLength, minimum, maximum, anyOf, oneOf and allOf. Other keywords are ignored. |
| [UndefinedStreamError](/api/Vercel.AI.Util/UndefinedStreamError/) | Rejection whose reason is JavaScript undefined. |
| [UnsupportedWarning](/api/Vercel.AI.Util/UnsupportedWarning/) | A feature the provider does not support. |

## Structs

| Type | Summary |
| --- | --- |
| [IteratorResult&lt;T&gt;](/api/Vercel.AI.Util/IteratorResult-1/) | Result of one async-iterator step. |
| [StreamRead&lt;T&gt;](/api/Vercel.AI.Util/StreamRead-1/) | One read from a ReadableStream%601. |

## Interfaces

| Type | Summary |
| --- | --- |
| [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/) | Reads error markers used to recognize AI SDK errors across assembly copies. Mirrors Symbol.for markers in the TypeScript SDK. |

## Enums

| Type | Summary |
| --- | --- |
| [RetryErrorReason](/api/Vercel.AI.Util/RetryErrorReason/) | Why RetryError stopped retrying. |

## Delegates

| Type | Summary |
| --- | --- |
| [DownloadFetch](/api/Vercel.AI.Util/DownloadFetch/) | Fetches one URL for Download. |
