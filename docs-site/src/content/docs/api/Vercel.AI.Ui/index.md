---
title: "Vercel.AI.Ui"
description: "UI message conversion."
editUrl: false
sidebar:
  label: "Vercel.AI.Ui"
---

[API reference](/api/)

UI message conversion.

## Classes

| Type | Summary |
| --- | --- |
| [ConvertToModelMessagesOptions](/api/Vercel.AI.Ui/ConvertToModelMessagesOptions/) | Options of ConvertToModelMessagesOptions). |
| [CustomContentUIPart](/api/Vercel.AI.Ui/CustomContentUIPart/) | A provider-specific part. Maps to CustomContentUIPart. |
| [DataUIPart](/api/Vercel.AI.Ui/DataUIPart/) | A data part. Its type is data-{name}. Maps to DataUIPart. |
| [FileUIPart](/api/Vercel.AI.Ui/FileUIPart/) | A file part. Maps to FileUIPart. |
| [ReasoningFileUIPart](/api/Vercel.AI.Ui/ReasoningFileUIPart/) | A file produced as reasoning. Maps to ReasoningFileUIPart. |
| [ReasoningUIPart](/api/Vercel.AI.Ui/ReasoningUIPart/) | A reasoning part. Maps to ReasoningUIPart. |
| [SourceDocumentUIPart](/api/Vercel.AI.Ui/SourceDocumentUIPart/) | A source that is a document. Maps to SourceDocumentUIPart. |
| [SourceUrlUIPart](/api/Vercel.AI.Ui/SourceUrlUIPart/) | A source that is a URL. Maps to SourceUrlUIPart. |
| [StepStartUIPart](/api/Vercel.AI.Ui/StepStartUIPart/) | Marks the start of a step. Maps to StepStartUIPart. |
| [TextUIPart](/api/Vercel.AI.Ui/TextUIPart/) | A text part. Maps to TextUIPart. |
| [ToolUIApproval](/api/Vercel.AI.Ui/ToolUIApproval/) | The approval of a tool call in a ToolUIPart. |
| [ToolUIPart](/api/Vercel.AI.Ui/ToolUIPart/) | A tool invocation part. Its type is tool-{name}, or dynamic-tool for a tool whose name is not known up front. Maps to ToolUIPart and DynamicToolUIPart. |
| [UIMessage](/api/Vercel.AI.Ui/UIMessage/) | A message shown in a chat UI, made of parts. Maps to UIMessage. |
| [UIMessageConversion](/api/Vercel.AI.Ui/UIMessageConversion/) | Conversion of UI messages to model messages. Maps to convertToModelMessages. |
| [UIMessagePart](/api/Vercel.AI.Ui/UIMessagePart/) | A part of a UIMessage. Maps to UIMessagePart. |

## Enums

| Type | Summary |
| --- | --- |
| [ToolUIPartState](/api/Vercel.AI.Ui/ToolUIPartState/) | The state of a ToolUIPart. |
| [UIMessageRole](/api/Vercel.AI.Ui/UIMessageRole/) | The role of a UIMessage. |
