---
title: "Vercel.AI.Anthropic"
description: "Anthropic Messages provider."
editUrl: false
sidebar:
  label: "Vercel.AI.Anthropic"
---

[API reference](/api/)

Anthropic Messages provider.

## Classes

| Type | Summary |
| --- | --- |
| [AnthropicAwsApiKeyFetch](/api/Vercel.AI.Anthropic/AnthropicAwsApiKeyFetch/) | API-key fetch wrapper. The transport is read when the call is sent, not when the wrapper is created. |
| [AnthropicAwsCredentials](/api/Vercel.AI.Anthropic/AnthropicAwsCredentials/) | AWS credentials used to sign Claude Platform requests. |
| [AnthropicAwsFetch](/api/Vercel.AI.Anthropic/AnthropicAwsFetch/) | Claude Platform on AWS fetch behavior. GET and POST requests without a body skip signing. POST bodies are signed for service aws-external-anthropic. |
| [AnthropicAwsPreparedCall](/api/Vercel.AI.Anthropic/AnthropicAwsPreparedCall/) | A request after the Claude Platform fetch wrapper has applied user-agent and SigV4. |
| [AnthropicAwsProvider](/api/Vercel.AI.Anthropic/AnthropicAwsProvider/) | Anthropic on AWS. Same Messages body, with regional hosts, workspace ids, and SigV4. |
| [AnthropicBashResult](/api/Vercel.AI.Anthropic/AnthropicBashResult/) | Output of a bash sandbox command. |
| [AnthropicBashTool](/api/Vercel.AI.Anthropic/AnthropicBashTool/) | Runs a bash provider tool through a caller-supplied sandbox. |
| [AnthropicBatch](/api/Vercel.AI.Anthropic/AnthropicBatch/) | Validation and status mapping for the Anthropic Message Batches API. |
| [AnthropicBatchApi](/api/Vercel.AI.Anthropic/AnthropicBatchApi/) | Anthropic Message Batches API. Downloads the JSONL results of a finished batch. |
| [AnthropicBatchCounts](/api/Vercel.AI.Anthropic/AnthropicBatchCounts/) | Request counts derived from an Anthropic message batch. |
| [AnthropicCacheControlValidator](/api/Vercel.AI.Anthropic/AnthropicCacheControlValidator/) | Reads cachecontrol and enforces Anthropic's four-breakpoint limit. |
| [AnthropicContainer](/api/Vercel.AI.Anthropic/AnthropicContainer/) | Copies the latest Anthropic container id into the next call's provider options. |
| [AnthropicCustomContent](/api/Vercel.AI.Anthropic/AnthropicCustomContent/) | A provider block the V4 content list has no dedicated type for. |
| [AnthropicErrorData](/api/Vercel.AI.Anthropic/AnthropicErrorData/) | Parsed Anthropic error body. Extra fields such as details are dropped. |
| [AnthropicEvaluationAnswer](/api/Vercel.AI.Anthropic/AnthropicEvaluationAnswer/) | One parsed answer. |
| [AnthropicEvaluationModel](/api/Vercel.AI.Anthropic/AnthropicEvaluationModel/) | Builds and scores Anthropic evaluation requests on the Messages API. |
| [AnthropicEvaluationQuestion](/api/Vercel.AI.Anthropic/AnthropicEvaluationQuestion/) | One evaluation question. |
| [AnthropicEvaluationResult](/api/Vercel.AI.Anthropic/AnthropicEvaluationResult/) | Evaluation call result. |
| [AnthropicFiles](/api/Vercel.AI.Anthropic/AnthropicFiles/) | Anthropic Files API upload. |
| [AnthropicJsonSchema](/api/Vercel.AI.Anthropic/AnthropicJsonSchema/) | Relaxes a JSON Schema so it can be sent as Anthropic outputconfig.format.schema. The caller's original schema is not modified. |
| [AnthropicLanguageModel](/api/Vercel.AI.Anthropic/AnthropicLanguageModel/) | Anthropic Messages language model. |
| [AnthropicMessagesRequest](/api/Vercel.AI.Anthropic/AnthropicMessagesRequest/) | Builds an Anthropic Messages request from SDK call options. |
| [AnthropicModelCapabilities](/api/Vercel.AI.Anthropic/AnthropicModelCapabilities/) | What a Claude model id accepts on the Messages API. |
| [AnthropicModelSettings](/api/Vercel.AI.Anthropic/AnthropicModelSettings/) | Optional settings that let a Messages-compatible provider adjust the shared Anthropic adapter. |
| [AnthropicOptions](/api/Vercel.AI.Anthropic/AnthropicOptions/) | Anthropic Messages API settings. |
| [AnthropicParseContext](/api/Vercel.AI.Anthropic/AnthropicParseContext/) | Settings for mapping an Anthropic Messages response onto SDK content. |
| [AnthropicPreparedRequest](/api/Vercel.AI.Anthropic/AnthropicPreparedRequest/) | A Messages request body plus the betas and warnings that go with it. |
| [AnthropicPreparedTools](/api/Vercel.AI.Anthropic/AnthropicPreparedTools/) | Tools and tool choice ready for a Messages request. |
| [AnthropicPrompt](/api/Vercel.AI.Anthropic/AnthropicPrompt/) | Converts SDK prompt messages into Anthropic message blocks. |
| [AnthropicPromptResult](/api/Vercel.AI.Anthropic/AnthropicPromptResult/) | Anthropic system and messages produced from an SDK prompt. |
| [AnthropicProvider](/api/Vercel.AI.Anthropic/AnthropicProvider/) | Anthropic Messages provider. |
| [AnthropicReasoning](/api/Vercel.AI.Anthropic/AnthropicReasoning/) | Maps SDK reasoning levels onto Anthropic thinking and effort. |
| [AnthropicReasoningConfig](/api/Vercel.AI.Anthropic/AnthropicReasoningConfig/) | Thinking and effort derived from a top-level reasoning level. |
| [AnthropicReasoningContent](/api/Vercel.AI.Anthropic/AnthropicReasoningContent/) | Reasoning content with a thinking signature or redacted payload. |
| [AnthropicResponse](/api/Vercel.AI.Anthropic/AnthropicResponse/) | Maps an Anthropic Messages JSON body onto a generate result. |
| [AnthropicServiceCollectionExtensions](/api/Vercel.AI.Anthropic/AnthropicServiceCollectionExtensions/) | Registers Anthropic. |
| [AnthropicSkillFile](/api/Vercel.AI.Anthropic/AnthropicSkillFile/) | A skill file sent as multipart files[]. |
| [AnthropicSkills](/api/Vercel.AI.Anthropic/AnthropicSkills/) | Anthropic Skills API. |
| [AnthropicSkillUpload](/api/Vercel.AI.Anthropic/AnthropicSkillUpload/) | Result of uploading a skill. |
| [AnthropicStopReason](/api/Vercel.AI.Anthropic/AnthropicStopReason/) | Maps an Anthropic stopreason onto the SDK finish reason. |
| [AnthropicStream](/api/Vercel.AI.Anthropic/AnthropicStream/) | Turns Anthropic SSE JSON events into SDK stream parts. |
| [AnthropicStreamError](/api/Vercel.AI.Anthropic/AnthropicStreamError/) | HTTP status implied by an Anthropic stream error type. |
| [AnthropicText](/api/Vercel.AI.Anthropic/AnthropicText/) | Generated text that also carries Anthropic provider metadata, such as citations. |
| [AnthropicToolDefinition](/api/Vercel.AI.Anthropic/AnthropicToolDefinition/) | A function or provider tool passed to AnthropicToolPreparer. |
| [AnthropicToolPreparer](/api/Vercel.AI.Anthropic/AnthropicToolPreparer/) | Builds the Anthropic tools and toolchoice fields. |
| [AnthropicToolResultContent](/api/Vercel.AI.Anthropic/AnthropicToolResultContent/) | A provider-executed tool result returned inside a Messages response. |
| [AnthropicUploadedFile](/api/Vercel.AI.Anthropic/AnthropicUploadedFile/) | Files API upload result. |
| [AnthropicUsage](/api/Vercel.AI.Anthropic/AnthropicUsage/) | Converts an Anthropic usage object into SDK token counts. |
| [AnthropicWarning](/api/Vercel.AI.Anthropic/AnthropicWarning/) | A warning produced while preparing an Anthropic request. |

## Delegates

| Type | Summary |
| --- | --- |
| [AnthropicMediaPartConverter](/api/Vercel.AI.Anthropic/AnthropicMediaPartConverter/) | Converts one file part for a provider that speaks the Messages protocol. Receives the part in the SDK message JSON shape. Returns the image or video block without cachecontrol, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to use the default conversion. |
