---
title: "Vercel.AI.GenerateText"
description: "Public types in the `Vercel.AI.GenerateText` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.GenerateText"
---

[API reference](/api/)

Public types in the `Vercel.AI.GenerateText` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [ActiveToolFilter](/api/Vercel.AI.GenerateText/ActiveToolFilter/) | Active-tool filtering. Maps to filterActiveTools. |
| [ApprovalHistoryMessage](/api/Vercel.AI.GenerateText/ApprovalHistoryMessage/) | A message in the approval history. |
| [ApprovalHistoryPart](/api/Vercel.AI.GenerateText/ApprovalHistoryPart/) | One part of an approval history message. |
| [ApprovalRequestPart](/api/Vercel.AI.GenerateText/ApprovalRequestPart/) | A request that a tool call be approved. |
| [ApprovalResponsePart](/api/Vercel.AI.GenerateText/ApprovalResponsePart/) | The caller's decision for an approval request. |
| [ApprovalToolCallPart](/api/Vercel.AI.GenerateText/ApprovalToolCallPart/) | A tool call stored in assistant history. |
| [ApprovalToolResultPart](/api/Vercel.AI.GenerateText/ApprovalToolResultPart/) | A tool result stored beside an approval response. |
| [CanonicalHash](/api/Vercel.AI.GenerateText/CanonicalHash/) | Canonical JSON and SHA-256 digests used by tool fingerprints and approval signatures. |
| [CollectedToolApproval](/api/Vercel.AI.GenerateText/CollectedToolApproval/) | One approval that still needs execution or denial handling. |
| [CollectedToolApprovalSet](/api/Vercel.AI.GenerateText/CollectedToolApprovalSet/) | Approved and denied tool approvals from the last tool message. |
| [ContentParts](/api/Vercel.AI.GenerateText/ContentParts/) | Reads text and reasoning out of generated content. Maps to extractTextContent and extractReasoningContent. |
| [DownloadException](/api/Vercel.AI.GenerateText/DownloadException/) | A download failed or the URL is not safe to fetch. Maps to DownloadError. |
| [DownloadUrls](/api/Vercel.AI.GenerateText/DownloadUrls/) | Rejects unsafe file URLs before a download starts. Maps to validateDownloadUrl. |
| [FingerprintTool](/api/Vercel.AI.GenerateText/FingerprintTool/) | A tool definition pinned by FingerprintTool%7d). |
| [GeneratedFileUrl](/api/Vercel.AI.GenerateText/GeneratedFileUrl/) | A generated file that still points at a URL. |
| [InvalidToolApprovalException](/api/Vercel.AI.GenerateText/InvalidToolApprovalException/) | An approval response names an unknown approval id. |
| [JsonInstructions](/api/Vercel.AI.GenerateText/JsonInstructions/) | JSON output instructions. Maps to injectJsonInstruction. |
| [MessagePruning](/api/Vercel.AI.GenerateText/MessagePruning/) | Message pruning. Maps to pruneMessages. |
| [NoOutputGeneratedException](/api/Vercel.AI.GenerateText/NoOutputGeneratedException/) | No LLM output was generated, for example because a model stream ended without output. Maps to NoOutputGeneratedError. |
| [OutputStrategies](/api/Vercel.AI.GenerateText/OutputStrategies/) | Array output schema wrapping. Maps to the array branch of getOutputStrategy. |
| [ParsedToolCall](/api/Vercel.AI.GenerateText/ParsedToolCall/) | A tool call after its input was parsed and validated. Maps to a TypedToolCall. |
| [ParseToolCallOptions](/api/Vercel.AI.GenerateText/ParseToolCallOptions/) | Options of CancellationToken). Maps to the options of parseToolCall. |
| [PruneMessagesOptions](/api/Vercel.AI.GenerateText/PruneMessagesOptions/) | Options of PruneMessages(PruneMessagesOptions). |
| [PruneToolCallsRule](/api/Vercel.AI.GenerateText/PruneToolCallsRule/) | One rule for pruning tool calls, results, errors, and approvals. Maps to an entry of the toolCalls array, { type: 'all' \| 'before-last-message' \| 'before-last-N-messages', tools }. |
| [TokenCounts](/api/Vercel.AI.GenerateText/TokenCounts/) | Token-count helpers. Maps to sumTokenCounts. |
| [TokenRates](/api/Vercel.AI.GenerateText/TokenRates/) | Token-rate helpers. Maps to calculateTokensPerSecond. |
| [ToolApprovalCollection](/api/Vercel.AI.GenerateText/ToolApprovalCollection/) | Collects tool approvals from the last tool message. Maps to collectToolApprovals. |
| [ToolApprovalSignatures](/api/Vercel.AI.GenerateText/ToolApprovalSignatures/) | HMAC approval signatures. Maps to signToolApproval and verifyToolApprovalSignature. |
| [ToolCallConcurrencyScheduler](/api/Vercel.AI.GenerateText/ToolCallConcurrencyScheduler/) | Tool call concurrency helpers. Maps to tool-call-concurrency.ts. |
| [ToolCallNotFoundForApprovalException](/api/Vercel.AI.GenerateText/ToolCallNotFoundForApprovalException/) | An approval request points at a tool call that is not in the history. |
| [ToolCallParser](/api/Vercel.AI.GenerateText/ToolCallParser/) | Parses the input of a tool call and validates it against the tool's input schema. Maps to parseToolCall. |
| [ToolCallRepairContext](/api/Vercel.AI.GenerateText/ToolCallRepairContext/) | Context passed to a ToolCallRepairFunction. Maps to the options of ToolCallRepairFunction. |
| [ToolChoiceViolationException](/api/Vercel.AI.GenerateText/ToolChoiceViolationException/) | The model response did not satisfy toolChoice. Maps to ToolChoiceViolationError. |
| [ToolContexts](/api/Vercel.AI.GenerateText/ToolContexts/) | Validates tool context. Maps to validateToolContext. |
| [ToolDrift](/api/Vercel.AI.GenerateText/ToolDrift/) | Added, removed, and changed tool names. |
| [ToolExecution](/api/Vercel.AI.GenerateText/ToolExecution/) | Finish-reason checks for the tool loop. Maps to isToolExecutionAllowedFinishReason. |
| [ToolFingerprints](/api/Vercel.AI.GenerateText/ToolFingerprints/) | Tool-definition fingerprints. Maps to fingerprintTools and detectToolDrift. |
| [ToolValidationContext](/api/Vercel.AI.GenerateText/ToolValidationContext/) | Where tool-context validation failed. |
| [TypeValidationException](/api/Vercel.AI.GenerateText/TypeValidationException/) | Tool context failed schema validation. Maps to TypeValidationError. |

## Structs

| Type | Summary |
| --- | --- |
| [JsonInstructionText](/api/Vercel.AI.GenerateText/JsonInstructionText/) | An optional string that can be omitted, null, or set. Distinguishes a missing argument from null. |

## Enums

| Type | Summary |
| --- | --- |
| [PruneEmptyMessages](/api/Vercel.AI.GenerateText/PruneEmptyMessages/) | Whether PruneMessages(PruneMessagesOptions) keeps messages whose content is empty after pruning. |
| [PruneReasoning](/api/Vercel.AI.GenerateText/PruneReasoning/) | How PruneMessages(PruneMessagesOptions) removes reasoning from assistant messages. |

## Delegates

| Type | Summary |
| --- | --- |
| [ToolCallRepairFunction](/api/Vercel.AI.GenerateText/ToolCallRepairFunction/) | Attempts to repair a tool call that failed to parse. Returns the repaired call, or null to keep the error. Maps to ToolCallRepairFunction. |
