// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.OpenAI;

/// <summary>
/// Typed models of the shapes that <c>openaiResponsesChunkSchema</c> and <c>openaiResponsesResponseSchema</c> share.
/// Upstream infers these types from the zod schemas (<c>InferSchema</c>) and its alignment tests compare them with <c>expectTypeOf</c>.
/// <see cref="OpenAIResponsesSchema"/> validates the JSON; these types describe what a valid value holds.
/// A shape that a chunk and a response both carry is one type here, so the two cannot drift apart.
/// </summary>
internal static partial class OpenAIResponsesApi
{
    /// <summary>The phase of an assistant message.</summary>
    public enum MessagePhase
    {
        /// <summary><c>commentary</c>.</summary>
        Commentary,

        /// <summary><c>final_answer</c>.</summary>
        FinalAnswer,
    }

    /// <summary>An annotation of an output text. The <c>type</c> discriminator picks the variant.</summary>
    public abstract record Annotation;

    /// <summary><c>url_citation</c>.</summary>
    public sealed record UrlCitation(int StartIndex, int EndIndex, string Url, string Title)
        : Annotation;

    /// <summary><c>file_citation</c>.</summary>
    public sealed record FileCitation(string FileId, string Filename, int Index) : Annotation;

    /// <summary><c>container_file_citation</c>.</summary>
    public sealed record ContainerFileCitation(
        string ContainerId,
        string FileId,
        string Filename,
        int StartIndex,
        int EndIndex
    ) : Annotation;

    /// <summary><c>file_path</c>.</summary>
    public sealed record FilePath(string FileId, int Index) : Annotation;

    /// <summary>A log probability of an output token.</summary>
    public sealed record TopLogprob(string Token, double Logprob);

    /// <summary>A token with its log probability and the top alternatives.</summary>
    public sealed record TokenLogprob(
        string Token,
        double Logprob,
        IReadOnlyList<TopLogprob> TopLogprobs
    );

    /// <summary>A source of a web search.</summary>
    public abstract record WebSearchSource;

    /// <summary>A <c>url</c> source.</summary>
    public sealed record WebSearchUrlSource(string Url) : WebSearchSource;

    /// <summary>An <c>api</c> source.</summary>
    public sealed record WebSearchApiSource(string Name) : WebSearchSource;

    /// <summary>The action of a <c>web_search_call</c>.</summary>
    public abstract record WebSearchAction;

    /// <summary><c>search</c>.</summary>
    public sealed record WebSearchSearch(
        string? Query,
        IReadOnlyList<string>? Queries,
        IReadOnlyList<WebSearchSource>? Sources
    ) : WebSearchAction;

    /// <summary><c>open_page</c>.</summary>
    public sealed record WebSearchOpenPage(string? Url) : WebSearchAction;

    /// <summary><c>find_in_page</c>.</summary>
    public sealed record WebSearchFindInPage(string? Url, string? Pattern) : WebSearchAction;

    /// <summary>An output of a <c>code_interpreter_call</c>.</summary>
    public abstract record CodeInterpreterOutput;

    /// <summary><c>logs</c>.</summary>
    public sealed record CodeInterpreterLogs(string Logs) : CodeInterpreterOutput;

    /// <summary><c>image</c>.</summary>
    public sealed record CodeInterpreterImage(string Url) : CodeInterpreterOutput;

    /// <summary>A result of a <c>file_search_call</c>. An attribute holds a string, a number, or a boolean.</summary>
    public sealed record FileSearchResult(
        IReadOnlyDictionary<string, object> Attributes,
        string FileId,
        string Filename,
        double Score,
        string Text
    );

    /// <summary>The <c>exec</c> action of a <c>local_shell_call</c>.</summary>
    public sealed record LocalShellAction(
        IReadOnlyList<string> Command,
        double? TimeoutMs,
        string? User,
        string? WorkingDirectory,
        IReadOnlyDictionary<string, string>? Env
    );

    /// <summary>A <c>local_shell_call</c> item. The added chunk, the done chunk, and the response carry the same shape.</summary>
    public sealed record LocalShellCall(string Id, string CallId, LocalShellAction Action);

    /// <summary>A content part of an assistant message.</summary>
    public abstract record MessageContent;

    /// <summary><c>output_text</c>.</summary>
    public sealed record OutputText(
        string Text,
        IReadOnlyList<TokenLogprob>? Logprobs,
        IReadOnlyList<Annotation> Annotations
    ) : MessageContent;

    /// <summary><c>refusal</c>.</summary>
    public sealed record Refusal(string RefusalText) : MessageContent;

    /// <summary>An item of <c>output</c> in a response (<c>openaiResponsesResponseSchema</c>).</summary>
    public interface IOutputItem;

    /// <summary>A <c>message</c> output item.</summary>
    public sealed record Message(
        string Id,
        MessagePhase? Phase,
        IReadOnlyList<MessageContent> Content
    ) : IOutputItem;

    /// <summary>A <c>web_search_call</c> output item.</summary>
    public sealed record WebSearchCall(string Id, string Status, WebSearchAction? Action)
        : IOutputItem;

    /// <summary>A <c>code_interpreter_call</c> output item.</summary>
    public sealed record CodeInterpreterCall(
        string Id,
        string? Code,
        string ContainerId,
        IReadOnlyList<CodeInterpreterOutput>? Outputs
    ) : IOutputItem;

    /// <summary>A <c>file_search_call</c> output item.</summary>
    public sealed record FileSearchCall(
        string Id,
        IReadOnlyList<string> Queries,
        IReadOnlyList<FileSearchResult>? Results
    ) : IOutputItem;

    /// <summary>A <c>function_call</c> output item.</summary>
    public sealed record FunctionCall(
        string Id,
        string CallId,
        string Name,
        string Arguments,
        bool? Async,
        string? Namespace = null,
        ToolCaller? Caller = null
    ) : IOutputItem;

    /// <summary>A <c>local_shell_call</c> output item.</summary>
    public sealed record LocalShellCallOutput(LocalShellCall Call) : IOutputItem;

    /// <summary>The part of <c>openaiResponsesResponseSchema</c> that these models cover.</summary>
    public sealed record Response(
        IReadOnlyList<IOutputItem>? Output,
        string? Id = null,
        double? CreatedAt = null,
        ResponseError? Error = null,
        string? Model = null,
        string? ServiceTier = null,
        string? ReasoningContext = null,
        string? IncompleteReason = null,
        Usage? Usage = null,
        PromptCacheDiagnostics? PromptCacheDiagnostics = null
    );

    /// <summary>A stream chunk (<c>openaiResponsesChunkSchema</c>).</summary>
    public abstract record Chunk;

    /// <summary><c>response.output_text.annotation.added</c>.</summary>
    public sealed record OutputTextAnnotationAdded(Annotation Annotation) : Chunk;

    /// <summary><c>response.output_text.delta</c>.</summary>
    public sealed record OutputTextDelta(
        string ItemId,
        double? OutputIndex,
        string Delta,
        IReadOnlyList<TokenLogprob>? Logprobs
    ) : Chunk;

    /// <summary>An item of <c>response.output_item.added</c>.</summary>
    public interface IAddedItem;

    /// <summary>An item of <c>response.output_item.done</c>.</summary>
    public interface IDoneItem;

    /// <summary>A <c>message</c> item of an added chunk.</summary>
    public sealed record AddedMessage(string Id, MessagePhase? Phase) : IAddedItem;

    /// <summary>A <c>message</c> item of a done chunk.</summary>
    public sealed record DoneMessage(string Id, MessagePhase? Phase) : IDoneItem;

    /// <summary>A <c>function_call</c> item of an added chunk.</summary>
    public sealed record AddedFunctionCall(
        string Id,
        string CallId,
        string Name,
        string Arguments,
        bool? Async,
        string? Namespace = null,
        ToolCaller? Caller = null
    ) : IAddedItem;

    /// <summary>A <c>function_call</c> item of a done chunk.</summary>
    public sealed record DoneFunctionCall(
        string Id,
        string CallId,
        string Name,
        string Arguments,
        bool? Async,
        string Status,
        string? Namespace = null,
        ToolCaller? Caller = null
    ) : IDoneItem;

    /// <summary>A <c>local_shell_call</c> item of an added chunk.</summary>
    public sealed record AddedLocalShellCall(LocalShellCall Call) : IAddedItem;

    /// <summary>A <c>local_shell_call</c> item of a done chunk.</summary>
    public sealed record DoneLocalShellCall(LocalShellCall Call) : IDoneItem;

    /// <summary>A <c>web_search_call</c> item of a done chunk.</summary>
    public sealed record DoneWebSearchCall(string Id, string Status, WebSearchAction? Action)
        : IDoneItem;

    /// <summary>A <c>code_interpreter_call</c> item of a done chunk.</summary>
    public sealed record DoneCodeInterpreterCall(
        string Id,
        string? Code,
        string ContainerId,
        IReadOnlyList<CodeInterpreterOutput>? Outputs
    ) : IDoneItem;

    /// <summary>A <c>file_search_call</c> item of a done chunk.</summary>
    public sealed record DoneFileSearchCall(
        string Id,
        IReadOnlyList<string> Queries,
        IReadOnlyList<FileSearchResult>? Results
    ) : IDoneItem;

    /// <summary>The <c>caller</c> of a function call.</summary>
    public abstract record ToolCaller;

    /// <summary><c>direct</c>.</summary>
    public sealed record DirectToolCaller : ToolCaller;

    /// <summary><c>program</c>.</summary>
    public sealed record ProgramToolCaller(string CallerId) : ToolCaller;

    /// <summary>The <c>error</c> of an mcp item: a string, or an object with optional fields.</summary>
    public abstract record ErrorValue;

    /// <summary>An error given as a string.</summary>
    public sealed record ErrorText(string Value) : ErrorValue;

    /// <summary>An error given as an object. A numeric <c>code</c> keeps its JSON text.</summary>
    public sealed record ErrorObject(string? Type, string? Code, string? Message) : ErrorValue;

    /// <summary>A point of a <c>drag</c> path.</summary>
    public sealed record ComputerPoint(double X, double Y);

    /// <summary>An action of a <c>computer_call</c>.</summary>
    public abstract record ComputerAction;

    /// <summary><c>click</c>.</summary>
    public sealed record ClickAction(string Button, double X, double Y, IReadOnlyList<string>? Keys)
        : ComputerAction;

    /// <summary><c>double_click</c>.</summary>
    public sealed record DoubleClickAction(double X, double Y, IReadOnlyList<string>? Keys)
        : ComputerAction;

    /// <summary><c>drag</c>.</summary>
    public sealed record DragAction(IReadOnlyList<ComputerPoint> Path, IReadOnlyList<string>? Keys)
        : ComputerAction;

    /// <summary><c>keypress</c>.</summary>
    public sealed record KeypressAction(IReadOnlyList<string> Keys) : ComputerAction;

    /// <summary><c>move</c>.</summary>
    public sealed record MoveAction(double X, double Y, IReadOnlyList<string>? Keys)
        : ComputerAction;

    /// <summary><c>screenshot</c>.</summary>
    public sealed record ScreenshotAction : ComputerAction;

    /// <summary><c>scroll</c>.</summary>
    public sealed record ScrollAction(
        double X,
        double Y,
        double ScrollX,
        double ScrollY,
        IReadOnlyList<string>? Keys
    ) : ComputerAction;

    /// <summary><c>type</c>.</summary>
    public sealed record TypeAction(string Text) : ComputerAction;

    /// <summary><c>wait</c>.</summary>
    public sealed record WaitAction : ComputerAction;

    /// <summary>An item of <c>pending_safety_checks</c>.</summary>
    public sealed record PendingSafetyCheck(string Id, string? Code, string? Message);

    /// <summary>The <c>operation</c> of an <c>apply_patch_call</c>.</summary>
    public abstract record ApplyPatchOperation;

    /// <summary><c>create_file</c>.</summary>
    public sealed record CreateFileOperation(string Path, string Diff) : ApplyPatchOperation;

    /// <summary><c>delete_file</c>.</summary>
    public sealed record DeleteFileOperation(string Path) : ApplyPatchOperation;

    /// <summary><c>update_file</c>.</summary>
    public sealed record UpdateFileOperation(string Path, string Diff) : ApplyPatchOperation;

    /// <summary>The <c>action</c> of a <c>shell_call</c>.</summary>
    public sealed record ShellCallAction(
        IReadOnlyList<string> Commands,
        double? TimeoutMs,
        double? MaxOutputLength
    );

    /// <summary>The <c>outcome</c> of a shell call output entry.</summary>
    public abstract record ShellOutcome;

    /// <summary><c>timeout</c>.</summary>
    public sealed record TimeoutOutcome : ShellOutcome;

    /// <summary><c>exit</c>.</summary>
    public sealed record ExitOutcome(double ExitCode) : ShellOutcome;

    /// <summary>An entry of the <c>output</c> of a <c>shell_call_output</c>.</summary>
    public sealed record ShellCallOutputEntry(string Stdout, string Stderr, ShellOutcome Outcome);

    /// <summary>A tool of an <c>mcp_list_tools</c> item. <c>input_schema</c> and <c>annotations</c> keep their JSON.</summary>
    public sealed record McpTool(
        string Name,
        string? Description,
        JsonElement? InputSchema,
        JsonElement? Annotations
    );

    /// <summary>A <c>program</c> item. The added chunk, the done chunk, and the response carry the same shape.</summary>
    public sealed record Program(string Id, string CallId, string Code, string Fingerprint)
        : IOutputItem,
            IAddedItem,
            IDoneItem;

    /// <summary>A <c>program_output</c> item. The added chunk, the done chunk, and the response carry the same shape.</summary>
    public sealed record ProgramOutput(string Id, string CallId, string Result, string Status)
        : IOutputItem,
            IAddedItem,
            IDoneItem;

    /// <summary>A <c>computer_call</c> item. The added chunk, the done chunk, and the response carry the same shape.</summary>
    public sealed record ComputerCall(
        string Id,
        string? CallId,
        string Status,
        ComputerAction? Action,
        IReadOnlyList<ComputerAction>? Actions,
        IReadOnlyList<PendingSafetyCheck>? PendingSafetyChecks
    ) : IOutputItem, IAddedItem, IDoneItem;

    /// <summary>An <c>apply_patch_call</c> item. The added chunk, the done chunk, and the response carry the same shape.</summary>
    public sealed record ApplyPatchCall(
        string Id,
        string CallId,
        string Status,
        ApplyPatchOperation Operation
    ) : IOutputItem, IAddedItem, IDoneItem;

    /// <summary>A <c>shell_call</c> item. The added chunk, the done chunk, and the response carry the same shape.</summary>
    public sealed record ShellCall(string Id, string CallId, string Status, ShellCallAction Action)
        : IOutputItem,
            IAddedItem,
            IDoneItem;

    /// <summary>A <c>shell_call_output</c> item. The added chunk, the done chunk, and the response carry the same shape.</summary>
    public sealed record ShellCallOutput(
        string Id,
        string CallId,
        string Status,
        double? MaxOutputLength,
        IReadOnlyList<ShellCallOutputEntry> Output
    ) : IOutputItem, IAddedItem, IDoneItem;

    /// <summary>A <c>tool_search_call</c> item. The added chunk, the done chunk, and the response carry the same shape.</summary>
    public sealed record ToolSearchCall(
        string Id,
        string Execution,
        string? CallId,
        string Status,
        JsonElement? Arguments
    ) : IOutputItem, IAddedItem, IDoneItem;

    /// <summary>A <c>tool_search_output</c> item. The added chunk, the done chunk, and the response carry the same shape.</summary>
    public sealed record ToolSearchOutput(
        string Id,
        string Execution,
        string? CallId,
        string Status,
        IReadOnlyList<JsonElement> Tools
    ) : IOutputItem, IAddedItem, IDoneItem;

    /// <summary>A <c>mcp_call</c> item. The done chunk and the response carry the same shape.</summary>
    public sealed record McpCall(
        string Id,
        string Status,
        string Arguments,
        string Name,
        string ServerLabel,
        string? Output,
        ErrorValue? Error,
        string? ApprovalRequestId
    ) : IOutputItem, IDoneItem;

    /// <summary>A <c>mcp_list_tools</c> item. The done chunk and the response carry the same shape.</summary>
    public sealed record McpListTools(
        string Id,
        string ServerLabel,
        IReadOnlyList<McpTool> Tools,
        ErrorValue? Error
    ) : IOutputItem, IDoneItem;

    /// <summary>A <c>mcp_approval_request</c> item. The done chunk and the response carry the same shape.</summary>
    public sealed record McpApprovalRequest(
        string Id,
        string ServerLabel,
        string Name,
        string Arguments,
        string? ApprovalRequestId
    ) : IOutputItem, IDoneItem;

    /// <summary>A <c>compaction</c> item. The done chunk and the response carry the same shape.</summary>
    public sealed record Compaction(string Id, string EncryptedContent) : IOutputItem, IDoneItem;

    /// <summary>An <c>image_generation_call</c> item. The done chunk and the response carry the same shape.</summary>
    public sealed record ImageGenerationCall(string Id, string Result) : IOutputItem, IDoneItem;

    /// <summary>A <c>custom_tool_call</c> item of a response or of an added chunk.</summary>
    public sealed record CustomToolCall(
        string Id,
        string CallId,
        string Name,
        string Input,
        bool? Async
    ) : IOutputItem, IAddedItem;

    /// <summary>A <c>custom_tool_call</c> item of a done chunk.</summary>
    public sealed record DoneCustomToolCall(
        string Id,
        string CallId,
        string Name,
        string Input,
        bool? Async,
        string Status
    ) : IDoneItem;

    /// <summary>A <c>reasoning</c> item of a response.</summary>
    public sealed record Reasoning(
        string Id,
        string? EncryptedContent,
        IReadOnlyList<string> Summary
    ) : IOutputItem;

    /// <summary>A <c>reasoning</c> item of an added or a done chunk.</summary>
    public sealed record ReasoningItem(string Id, string? EncryptedContent) : IAddedItem, IDoneItem;

    /// <summary>A <c>web_search_call</c> item of an added chunk.</summary>
    public sealed record AddedWebSearchCall(string Id, string Status) : IAddedItem;

    /// <summary>A <c>file_search_call</c> item of an added chunk.</summary>
    public sealed record AddedFileSearchCall(string Id) : IAddedItem;

    /// <summary>An <c>image_generation_call</c> item of an added chunk.</summary>
    public sealed record AddedImageGenerationCall(string Id) : IAddedItem;

    /// <summary>A <c>code_interpreter_call</c> item of an added chunk.</summary>
    public sealed record AddedCodeInterpreterCall(
        string Id,
        string ContainerId,
        string? Code,
        IReadOnlyList<CodeInterpreterOutput>? Outputs,
        string Status
    ) : IAddedItem;

    /// <summary>A <c>mcp_call</c> item of an added chunk.</summary>
    public sealed record AddedMcpCall(string Id, string Status, string? ApprovalRequestId)
        : IAddedItem;

    /// <summary>A <c>mcp_list_tools</c> item of an added chunk.</summary>
    public sealed record AddedMcpListTools(string Id) : IAddedItem;

    /// <summary>A <c>mcp_approval_request</c> item of an added chunk.</summary>
    public sealed record AddedMcpApprovalRequest(string Id) : IAddedItem;

    /// <summary>A <c>compaction</c> item of an added chunk.</summary>
    public sealed record AddedCompaction(string Id, string? EncryptedContent) : IAddedItem;

    /// <summary>The token usage of a response.</summary>
    public sealed record Usage(
        double InputTokens,
        InputTokensDetails? InputTokensDetails,
        double OutputTokens,
        OutputTokensDetails? OutputTokensDetails,
        double? TotalTokens,
        JsonElement? Raw = null
    );

    /// <summary>The <c>input_tokens_details</c> of a usage.</summary>
    public sealed record InputTokensDetails(
        double? CachedTokens,
        double? CacheWriteTokens,
        double? Ttl30mTokens,
        double? Ttl12hTokens,
        double? OrchestrationInputTokens,
        double? OrchestrationInputCachedTokens
    );

    /// <summary>The <c>output_tokens_details</c> of a usage.</summary>
    public sealed record OutputTokensDetails(
        double? ReasoningTokens,
        double? OrchestrationOutputTokens
    );

    /// <summary>The <c>error</c> of a response body.</summary>
    public sealed record ResponseError(string Message, string Type, string? Param, string Code);

    /// <summary>The <c>prompt_cache_diagnostics</c> of a response.</summary>
    public sealed record PromptCacheDiagnostics(
        string Type,
        string? Reason,
        double? CacheMissedTokens,
        double? ComparisonReusableTokens
    );

    /// <summary>The <c>response</c> of <c>response.created</c> and <c>response.in_progress</c>.</summary>
    public sealed record ResponseHeader(
        string Id,
        double CreatedAt,
        string Model,
        string? ServiceTier
    );

    /// <summary>The <c>response</c> of <c>response.completed</c>, <c>response.incomplete</c> and <c>response.failed</c>.</summary>
    public sealed record FinishedResponse(
        PromptCacheDiagnostics? PromptCacheDiagnostics,
        string? IncompleteReason,
        Usage? Usage,
        string? ReasoningContext,
        string? ServiceTier,
        FailedResponseError? Error
    );

    /// <summary>The <c>error</c> of the response of a <c>response.failed</c> chunk.</summary>
    public sealed record FailedResponseError(string? Code, string Message);

    /// <summary><c>response.created</c>.</summary>
    public sealed record ResponseCreated(ResponseHeader Response) : Chunk;

    /// <summary><c>response.in_progress</c>.</summary>
    public sealed record ResponseInProgress(ResponseHeader Response) : Chunk;

    /// <summary><c>response.completed</c>.</summary>
    public sealed record ResponseCompleted(FinishedResponse Response) : Chunk;

    /// <summary><c>response.incomplete</c>.</summary>
    public sealed record ResponseIncomplete(FinishedResponse Response) : Chunk;

    /// <summary><c>response.failed</c>.</summary>
    public sealed record ResponseFailed(double SequenceNumber, FinishedResponse Response) : Chunk;

    /// <summary><c>response.output_item.added</c>.</summary>
    public sealed record OutputItemAdded(double OutputIndex, IAddedItem Item) : Chunk;

    /// <summary><c>response.output_item.done</c>.</summary>
    public sealed record OutputItemDone(double OutputIndex, IDoneItem Item) : Chunk;

    /// <summary><c>response.refusal.delta</c>.</summary>
    public sealed record RefusalDelta(
        string ItemId,
        double? OutputIndex,
        double? ContentIndex,
        string Delta
    ) : Chunk;

    /// <summary><c>response.refusal.done</c>.</summary>
    public sealed record RefusalDone(
        string ItemId,
        double? OutputIndex,
        double? ContentIndex,
        string Refusal
    ) : Chunk;

    /// <summary><c>response.function_call_arguments.delta</c>.</summary>
    public sealed record FunctionCallArgumentsDelta(string ItemId, double OutputIndex, string Delta)
        : Chunk;

    /// <summary><c>response.function_call_arguments.done</c>. Upstream omits <c>name</c>.</summary>
    public sealed record FunctionCallArgumentsDone(
        string ItemId,
        double OutputIndex,
        string Arguments
    ) : Chunk;

    /// <summary><c>response.custom_tool_call_input.delta</c>.</summary>
    public sealed record CustomToolCallInputDelta(string ItemId, double OutputIndex, string Delta)
        : Chunk;

    /// <summary><c>response.image_generation_call.partial_image</c>.</summary>
    public sealed record ImageGenerationCallPartialImage(
        string ItemId,
        double OutputIndex,
        string PartialImageB64
    ) : Chunk;

    /// <summary><c>response.code_interpreter_call_code.delta</c>.</summary>
    public sealed record CodeInterpreterCallCodeDelta(
        string ItemId,
        double OutputIndex,
        string Delta
    ) : Chunk;

    /// <summary><c>response.code_interpreter_call_code.done</c>.</summary>
    public sealed record CodeInterpreterCallCodeDone(string ItemId, double OutputIndex, string Code)
        : Chunk;

    /// <summary><c>response.reasoning_summary_part.added</c>.</summary>
    public sealed record ReasoningSummaryPartAdded(
        string ItemId,
        double? OutputIndex,
        double SummaryIndex
    ) : Chunk;

    /// <summary><c>response.reasoning_summary_text.delta</c>.</summary>
    public sealed record ReasoningSummaryTextDelta(
        string ItemId,
        double? OutputIndex,
        double SummaryIndex,
        string Delta
    ) : Chunk;

    /// <summary><c>response.reasoning_summary_part.done</c>.</summary>
    public sealed record ReasoningSummaryPartDone(
        string ItemId,
        double? OutputIndex,
        double SummaryIndex
    ) : Chunk;

    /// <summary><c>response.apply_patch_call_operation_diff.delta</c>.</summary>
    public sealed record ApplyPatchCallOperationDiffDelta(
        string ItemId,
        double OutputIndex,
        string Delta,
        string? Obfuscation
    ) : Chunk;

    /// <summary><c>response.apply_patch_call_operation_diff.done</c>.</summary>
    public sealed record ApplyPatchCallOperationDiffDone(
        string ItemId,
        double OutputIndex,
        string Diff
    ) : Chunk;

    /// <summary><c>error</c>. The nested and the flat shape of the chunk both land here.</summary>
    public sealed record ErrorChunk(
        double SequenceNumber,
        string? Type,
        string? Code,
        string Message,
        string? Param
    ) : Chunk;

    /// <summary>The <c>unknown_chunk</c> fallback of <c>openaiResponsesChunkSchema</c>: a chunk type, or an output item type, that is not modeled.</summary>
    public sealed record UnknownChunk(JsonElement Value) : Chunk;
}
