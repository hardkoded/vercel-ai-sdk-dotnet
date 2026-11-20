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
| [DeliveredSteeringOutcome](/api/Vercel.AI.GenerateText/DeliveredSteeringOutcome/) | The message was added to the run. |
| [DownloadException](/api/Vercel.AI.GenerateText/DownloadException/) | A download failed or the URL is not safe to fetch. Maps to DownloadError. |
| [DownloadUrls](/api/Vercel.AI.GenerateText/DownloadUrls/) | Rejects unsafe file URLs before a download starts. Maps to validateDownloadUrl. |
| [FingerprintTool](/api/Vercel.AI.GenerateText/FingerprintTool/) | A tool definition pinned by FingerprintTool%7d). |
| [GeneratedFileUrl](/api/Vercel.AI.GenerateText/GeneratedFileUrl/) | A generated file that still points at a URL. |
| [GenericToolApprovalOptions](/api/Vercel.AI.GenerateText/GenericToolApprovalOptions/) | The options a GenericToolApprovalFunction receives. Maps to the options of GenericToolApprovalFunction. |
| [InvalidToolApproval](/api/Vercel.AI.GenerateText/InvalidToolApproval/) | An approved tool call whose input failed validation. Maps to a collected approval with an error. |
| [InvalidToolApprovalException](/api/Vercel.AI.GenerateText/InvalidToolApprovalException/) | An approval response names an unknown approval id. |
| [JsonInstructions](/api/Vercel.AI.GenerateText/JsonInstructions/) | JSON output instructions. Maps to injectJsonInstruction. |
| [MessagePruning](/api/Vercel.AI.GenerateText/MessagePruning/) | Message pruning. Maps to pruneMessages. |
| [NoOutputGeneratedException](/api/Vercel.AI.GenerateText/NoOutputGeneratedException/) | No LLM output was generated, for example because a model stream ended without output. Maps to NoOutputGeneratedError. |
| [NotDeliveredSteeringOutcome](/api/Vercel.AI.GenerateText/NotDeliveredSteeringOutcome/) | The message was not added to the run. |
| [OutputStrategies](/api/Vercel.AI.GenerateText/OutputStrategies/) | Array output schema wrapping. Maps to the array branch of getOutputStrategy. |
| [ParsedToolCall](/api/Vercel.AI.GenerateText/ParsedToolCall/) | A tool call after its input was parsed and validated. Maps to a TypedToolCall. |
| [ParseToolCallOptions](/api/Vercel.AI.GenerateText/ParseToolCallOptions/) | Options of CancellationToken). Maps to the options of parseToolCall. |
| [PruneMessagesOptions](/api/Vercel.AI.GenerateText/PruneMessagesOptions/) | Options of PruneMessages(PruneMessagesOptions). |
| [PruneToolCallsRule](/api/Vercel.AI.GenerateText/PruneToolCallsRule/) | One rule for pruning tool calls, results, errors, and approvals. Maps to an entry of the toolCalls array, { type: 'all' \| 'before-last-message' \| 'before-last-N-messages', tools }. |
| [QueueOptions](/api/Vercel.AI.GenerateText/QueueOptions/) | Options of QueueAsync. Queued messages wait for the task, so they cannot interrupt. |
| [SingleToolApprovalOptions](/api/Vercel.AI.GenerateText/SingleToolApprovalOptions/) | The options a SingleToolApprovalFunction receives. Maps to the second argument of SingleToolApprovalFunction. |
| [SmoothStream](/api/Vercel.AI.GenerateText/SmoothStream/) | Smooths text and reasoning streaming output. Maps to smoothStream. |
| [SmoothStreamOptions](/api/Vercel.AI.GenerateText/SmoothStreamOptions/) | Options of Create(SmoothStreamOptions). Maps to the options of smoothStream. |
| [Steering](/api/Vercel.AI.GenerateText/Steering/) | Creates steering objects and starts their runs. Maps to steering.ts. |
| [SteeringOutcome](/api/Vercel.AI.GenerateText/SteeringOutcome/) | The outcome of a steering message. Maps to SteeringOutcome. |
| [SteerOptions](/api/Vercel.AI.GenerateText/SteerOptions/) | Options of SteerAsync. |
| [TokenCounts](/api/Vercel.AI.GenerateText/TokenCounts/) | Token-count helpers. Maps to sumTokenCounts. |
| [TokenRates](/api/Vercel.AI.GenerateText/TokenRates/) | Token-rate helpers. Maps to calculateTokensPerSecond. |
| [ToolApprovalCollection](/api/Vercel.AI.GenerateText/ToolApprovalCollection/) | Collects tool approvals from the last tool message. Maps to collectToolApprovals. |
| [ToolApprovalConfiguration](/api/Vercel.AI.GenerateText/ToolApprovalConfiguration/) | Configures whether individual tools require approval before they can run. Either use a generic function that is called for all tool calls, or set an approval status (or a function that produces one) for each tool. It takes precedence over the approval settings of the tools. Maps to ToolApprovalConfiguration. |
| [ToolApprovalEntry](/api/Vercel.AI.GenerateText/ToolApprovalEntry/) | The approval setting of one tool: a status or a function that produces a status at runtime. |
| [ToolApprovalResolution](/api/Vercel.AI.GenerateText/ToolApprovalResolution/) | Resolves the approval state of a tool call. Maps to resolveToolApproval. |
| [ToolApprovalSignatures](/api/Vercel.AI.GenerateText/ToolApprovalSignatures/) | HMAC approval signatures. Maps to signToolApproval and verifyToolApprovalSignature. |
| [ToolApprovalStatus](/api/Vercel.AI.GenerateText/ToolApprovalStatus/) | The approval status of a tool call: not-applicable (no approval needed), approved (approved automatically), denied (denied automatically), or user-approval (the user must approve). Maps to ToolApprovalStatus. For approved and denied, the reason is emitted on the approval response. For user-approval, it is emitted on the approval request so it can be shown to the approver. A function that returns <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> has the effect of not-applicable. |
| [ToolApprovalValidation](/api/Vercel.AI.GenerateText/ToolApprovalValidation/) | Validates approvals that come from message history. Maps to validateApprovedToolApprovals. |
| [ToolCallConcurrencyScheduler](/api/Vercel.AI.GenerateText/ToolCallConcurrencyScheduler/) | Tool call concurrency helpers. Maps to tool-call-concurrency.ts. |
| [ToolCallNotFoundForApprovalException](/api/Vercel.AI.GenerateText/ToolCallNotFoundForApprovalException/) | An approval request points at a tool call that is not in the history. |
| [ToolCallParser](/api/Vercel.AI.GenerateText/ToolCallParser/) | Parses the input of a tool call and validates it against the tool's input schema. Maps to parseToolCall. |
| [ToolCallRepairContext](/api/Vercel.AI.GenerateText/ToolCallRepairContext/) | Context passed to a ToolCallRepairFunction. Maps to the options of ToolCallRepairFunction. |
| [ToolChoiceViolationException](/api/Vercel.AI.GenerateText/ToolChoiceViolationException/) | The model response did not satisfy toolChoice. Maps to ToolChoiceViolationError. |
| [ToolContexts](/api/Vercel.AI.GenerateText/ToolContexts/) | Validates tool context. Maps to validateToolContext. |
| [ToolDrift](/api/Vercel.AI.GenerateText/ToolDrift/) | Added, removed, and changed tool names. |
| [ToolExecution](/api/Vercel.AI.GenerateText/ToolExecution/) | Finish-reason checks for the tool loop. Maps to isToolExecutionAllowedFinishReason. |
| [ToolFingerprints](/api/Vercel.AI.GenerateText/ToolFingerprints/) | Tool-definition fingerprints. Maps to fingerprintTools and detectToolDrift. |
| [ToolNeedsApprovalOptions](/api/Vercel.AI.GenerateText/ToolNeedsApprovalOptions/) | The options a tool's NeedsApprovalFunction receives. Maps to the second argument of the needsApproval function. |
| [ToolValidationContext](/api/Vercel.AI.GenerateText/ToolValidationContext/) | Where tool-context validation failed. |
| [TypeValidationException](/api/Vercel.AI.GenerateText/TypeValidationException/) | Tool context failed schema validation. Maps to TypeValidationError. |
| [ValidateApprovedToolApprovalsOptions](/api/Vercel.AI.GenerateText/ValidateApprovedToolApprovalsOptions/) | The options of CancellationToken). |
| [ValidatedToolApprovals](/api/Vercel.AI.GenerateText/ValidatedToolApprovals/) | The outcome of CancellationToken). |

## Structs

| Type | Summary |
| --- | --- |
| [JsonInstructionText](/api/Vercel.AI.GenerateText/JsonInstructionText/) | An optional string that can be omitted, null, or set. Distinguishes a missing argument from null. |

## Interfaces

| Type | Summary |
| --- | --- |
| [ISegmenter](/api/Vercel.AI.GenerateText/ISegmenter/) | Splits text into segments, like Intl.Segmenter. .NET has no built-in word segmenter, so you provide one (for example a wrapper over ICU) to chunk text with locale-aware word segmentation, which suits CJK languages. |
| [ISteering](/api/Vercel.AI.GenerateText/ISteering/) | Sends user messages to a running generation. Pass it to GenerateTextAsync, StreamText, or an agent call with the ExperimentalSteering option. Maps to Steering. |

## Enums

| Type | Summary |
| --- | --- |
| [PruneEmptyMessages](/api/Vercel.AI.GenerateText/PruneEmptyMessages/) | Whether PruneMessages(PruneMessagesOptions) keeps messages whose content is empty after pruning. |
| [PruneReasoning](/api/Vercel.AI.GenerateText/PruneReasoning/) | How PruneMessages(PruneMessagesOptions) removes reasoning from assistant messages. |
| [SmoothStreamChunking](/api/Vercel.AI.GenerateText/SmoothStreamChunking/) | How SmoothStream chunks the text. Maps to the 'word' \| 'line' values of chunking. |
| [SteeringNotDeliveredReason](/api/Vercel.AI.GenerateText/SteeringNotDeliveredReason/) | Why a steering message was not delivered. Maps to SteeringNotDeliveredReason. |

## Delegates

| Type | Summary |
| --- | --- |
| [ChunkDetector](/api/Vercel.AI.GenerateText/ChunkDetector/) | Detects the first chunk in a buffer. Maps to ChunkDetector. Returns the first detected chunk, or null when no chunk was detected. |
| [GenericToolApprovalFunction](/api/Vercel.AI.GenerateText/GenericToolApprovalFunction/) | Determines the approval status of any tool call. Returning <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> has the effect of not-applicable. Maps to GenericToolApprovalFunction. |
| [SingleToolApprovalFunction](/api/Vercel.AI.GenerateText/SingleToolApprovalFunction/) | Determines the approval status of a call to one tool, from the input of the call. Returning <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> has the effect of not-applicable. Maps to SingleToolApprovalFunction. |
| [ToolCallRepairFunction](/api/Vercel.AI.GenerateText/ToolCallRepairFunction/) | Attempts to repair a tool call that failed to parse. Returns the repaired call, or null to keep the error. Maps to ToolCallRepairFunction. |
