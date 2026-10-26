---
title: "Vercel.AI.Prompt"
description: "Public types in the `Vercel.AI.Prompt` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.Prompt"
---

[API reference](/api/)

Public types in the `Vercel.AI.Prompt` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [ContentToolModelOutput](/api/Vercel.AI.Prompt/ContentToolModelOutput/) | Multi-part tool output. |
| [DataUrlParts](/api/Vercel.AI.Prompt/DataUrlParts/) | Media type and base64 payload from a data URL. Maps to splitDataUrl. |
| [DataUrls](/api/Vercel.AI.Prompt/DataUrls/) | Data URL parsing. Maps to splitDataUrl. |
| [ErrorJsonToolModelOutput](/api/Vercel.AI.Prompt/ErrorJsonToolModelOutput/) | Error JSON tool output. |
| [ErrorTextToolModelOutput](/api/Vercel.AI.Prompt/ErrorTextToolModelOutput/) | Error text tool output. |
| [FilePartInput](/api/Vercel.AI.Prompt/FilePartInput/) | A file-part value before it is converted to the v4 provider shape. |
| [FileParts](/api/Vercel.AI.Prompt/FileParts/) | File-part conversion. Maps to convertToLanguageModelV4FilePart. |
| [Instructions](/api/Vercel.AI.Prompt/Instructions/) | Instructions for a call: a string, one system message, or a list of system messages. Maps to Instructions. A single system message is kept as a one-item Messages list. |
| [InvalidDataContentException](/api/Vercel.AI.Prompt/InvalidDataContentException/) | File content could not be converted. Maps to InvalidDataContentError. |
| [InvalidPromptException](/api/Vercel.AI.Prompt/InvalidPromptException/) | The prompt input is invalid. Maps to InvalidPromptError. |
| [JsonToolModelOutput](/api/Vercel.AI.Prompt/JsonToolModelOutput/) | JSON tool output. |
| [LanguageModelFilePart](/api/Vercel.AI.Prompt/LanguageModelFilePart/) | A file part in the v4 provider prompt. Maps to the result of convertToLanguageModelV4FilePart. |
| [PreparedTool](/api/Vercel.AI.Prompt/PreparedTool/) | A tool after String). |
| [PreparedToolChoice](/api/Vercel.AI.Prompt/PreparedToolChoice/) | A tool choice sent to a language model. Maps to the result of prepareToolChoice. |
| [PreservedStreamError](/api/Vercel.AI.Prompt/PreservedStreamError/) | An error-shaped value that must be returned unchanged. |
| [PromptInput](/api/Vercel.AI.Prompt/PromptInput/) | The prompt of a call: a user string or a list of messages. Maps to prompt: string \| ModelMessage[]. |
| [Prompts](/api/Vercel.AI.Prompt/Prompts/) | Prompt normalization. Maps to standardizePrompt. |
| [PromptTool](/api/Vercel.AI.Prompt/PromptTool/) | A tool passed to String). |
| [PromptTools](/api/Vercel.AI.Prompt/PromptTools/) | Tool preparation. Maps to prepareTools. |
| [ProviderStreamError](/api/Vercel.AI.Prompt/ProviderStreamError/) | A provider stream payload that already carries status metadata. |
| [StandardizedPrompt](/api/Vercel.AI.Prompt/StandardizedPrompt/) | A standardized prompt. Maps to the result of standardizePrompt. |
| [StandardizePromptInput](/api/Vercel.AI.Prompt/StandardizePromptInput/) | Input for StandardizePrompt(StandardizePromptInput). |
| [StreamErrors](/api/Vercel.AI.Prompt/StreamErrors/) | Normalizes provider stream failures. Maps to normalizeStreamProviderError. |
| [StreamProviderError](/api/Vercel.AI.Prompt/StreamProviderError/) | A normalized stream provider error. Maps to StreamProviderError. |
| [TextToolModelOutput](/api/Vercel.AI.Prompt/TextToolModelOutput/) | Text tool output. |
| [TimeoutConfiguration](/api/Vercel.AI.Prompt/TimeoutConfiguration/) | Timeout configuration for GenerateTextAsync and StreamTextAsync. Maps to TimeoutConfiguration. Upstream takes a number (the total timeout in milliseconds) or an object. A TimeSpan converts to the total timeout, as the number form does upstream. An expired timeout aborts the call with a TimeoutError reason. |
| [ToolChoices](/api/Vercel.AI.Prompt/ToolChoices/) | Tool-choice normalization. Maps to prepareToolChoice. |
| [ToolDescriptionContext](/api/Vercel.AI.Prompt/ToolDescriptionContext/) | Context passed to a function-valued tool description. |
| [ToolModelContentPart](/api/Vercel.AI.Prompt/ToolModelContentPart/) | One content part inside a content tool output. |
| [ToolModelOutput](/api/Vercel.AI.Prompt/ToolModelOutput/) | A tool result sent back to the model. Maps to ToolResultOutput. |
| [ToolModelOutputCall](/api/Vercel.AI.Prompt/ToolModelOutputCall/) | Arguments passed to toModelOutput. |
| [ToolOutputs](/api/Vercel.AI.Prompt/ToolOutputs/) | Builds the model-facing tool result. Maps to createToolModelOutput. |
| [ToolOutputs.Undefined](/api/Vercel.AI.Prompt/ToolOutputs.Undefined/) | Sentinel for an undefined tool output. |

## Interfaces

| Type | Summary |
| --- | --- |
| [IToolJsonValue](/api/Vercel.AI.Prompt/IToolJsonValue/) | A value that supplies its own JSON form, matching JavaScript toJSON. |
