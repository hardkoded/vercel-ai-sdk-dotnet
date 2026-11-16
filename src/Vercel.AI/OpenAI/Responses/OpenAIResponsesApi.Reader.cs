// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;

namespace Vercel.AI.OpenAI;

/// <summary>
/// Reads a JSON value that <see cref="OpenAIResponsesSchema"/> accepted into the typed models, like the value that zod returns from <c>safeParse</c>.
/// A value the schema rejects is never read: <see cref="OpenAIResponsesSchema.ParseChunk"/> and <see cref="OpenAIResponsesSchema.ParseResponse"/> validate first.
/// </summary>
internal static partial class OpenAIResponsesApi
{
    /// <summary>Reads a valid response body. The caller validated it.</summary>
    public static Response ReadResponse(JsonElement body) =>
        new(
            body.Opt("output") is { ValueKind: JsonValueKind.Array } output
                ? [.. output.EnumerateArray().Select(ReadOutputItem)]
                : null,
            body.Str("id"),
            body.Num("created_at"),
            body.Obj("error") is { } error
                ? new ResponseError(
                    error.Req("message"),
                    error.Req("type"),
                    error.Str("param"),
                    error.Req("code")
                )
                : null,
            body.Str("model"),
            body.Str("service_tier"),
            body.Obj("reasoning")?.Str("context"),
            body.Obj("incomplete_details")?.Req("reason"),
            ReadUsage(body.Obj("usage"))
        );

    /// <summary>Reads a chunk that matched a modeled variant of <c>openaiResponsesChunkSchema</c>. The caller validated it, and handles <c>unknown_chunk</c> itself.</summary>
    public static Chunk ReadChunk(JsonElement chunk)
    {
        var type = chunk.Str("type");

        switch (type)
        {
            case "response.output_text.delta":
                return new OutputTextDelta(
                    chunk.Req("item_id"),
                    chunk.Req("delta"),
                    ReadLogprobs(chunk.Opt("logprobs"))
                );
            case "response.refusal.delta":
                return new RefusalDelta(
                    chunk.Req("item_id"),
                    chunk.Num("output_index"),
                    chunk.Num("content_index"),
                    chunk.Req("delta")
                );
            case "response.refusal.done":
                return new RefusalDone(
                    chunk.Req("item_id"),
                    chunk.Num("output_index"),
                    chunk.Num("content_index"),
                    chunk.Req("refusal")
                );
            case "response.completed":
                return new ResponseCompleted(ReadFinished(chunk.Prop("response")));
            case "response.incomplete":
                return new ResponseIncomplete(ReadFinished(chunk.Prop("response")));
            case "response.failed":
                return new ResponseFailed(
                    chunk.Num("sequence_number") ?? 0,
                    ReadFinished(chunk.Prop("response"))
                );
            case "response.created":
                return new ResponseCreated(ReadHeader(chunk.Prop("response")));
            case "response.in_progress":
                return new ResponseInProgress(ReadHeader(chunk.Prop("response")));
            case "response.output_item.added":
                return new OutputItemAdded(
                    chunk.Num("output_index") ?? 0,
                    ReadAddedItem(chunk.Prop("item"))
                );
            case "response.output_item.done":
                return new OutputItemDone(
                    chunk.Num("output_index") ?? 0,
                    ReadDoneItem(chunk.Prop("item"))
                );
            case "response.function_call_arguments.delta":
                return new FunctionCallArgumentsDelta(
                    chunk.Req("item_id"),
                    chunk.Num("output_index") ?? 0,
                    chunk.Req("delta")
                );
            case "response.function_call_arguments.done":
                return new FunctionCallArgumentsDone(
                    chunk.Req("item_id"),
                    chunk.Num("output_index") ?? 0,
                    chunk.Req("arguments")
                );
            case "response.custom_tool_call_input.delta":
                return new CustomToolCallInputDelta(
                    chunk.Req("item_id"),
                    chunk.Num("output_index") ?? 0,
                    chunk.Req("delta")
                );
            case "response.image_generation_call.partial_image":
                return new ImageGenerationCallPartialImage(
                    chunk.Req("item_id"),
                    chunk.Num("output_index") ?? 0,
                    chunk.Req("partial_image_b64")
                );
            case "response.code_interpreter_call_code.delta":
                return new CodeInterpreterCallCodeDelta(
                    chunk.Req("item_id"),
                    chunk.Num("output_index") ?? 0,
                    chunk.Req("delta")
                );
            case "response.code_interpreter_call_code.done":
                return new CodeInterpreterCallCodeDone(
                    chunk.Req("item_id"),
                    chunk.Num("output_index") ?? 0,
                    chunk.Req("code")
                );
            case "response.output_text.annotation.added":
                return new OutputTextAnnotationAdded(ReadAnnotation(chunk.Prop("annotation")));
            case "response.reasoning_summary_part.added":
                return new ReasoningSummaryPartAdded(
                    chunk.Req("item_id"),
                    chunk.Num("output_index"),
                    chunk.Num("summary_index") ?? 0
                );
            case "response.reasoning_summary_text.delta":
                return new ReasoningSummaryTextDelta(
                    chunk.Req("item_id"),
                    chunk.Num("output_index"),
                    chunk.Num("summary_index") ?? 0,
                    chunk.Req("delta")
                );
            case "response.reasoning_summary_part.done":
                return new ReasoningSummaryPartDone(
                    chunk.Req("item_id"),
                    chunk.Num("output_index"),
                    chunk.Num("summary_index") ?? 0
                );
            case "response.apply_patch_call_operation_diff.delta":
                return new ApplyPatchCallOperationDiffDelta(
                    chunk.Req("item_id"),
                    chunk.Num("output_index") ?? 0,
                    chunk.Req("delta"),
                    chunk.Str("obfuscation")
                );
            case "response.apply_patch_call_operation_diff.done":
                return new ApplyPatchCallOperationDiffDone(
                    chunk.Req("item_id"),
                    chunk.Num("output_index") ?? 0,
                    chunk.Req("diff")
                );
            case "error":
                // The nested shape wins, like the first member of the upstream union.
                return chunk.Obj("error") is { } nested
                    ? new ErrorChunk(
                        chunk.Num("sequence_number") ?? 0,
                        nested.Str("type"),
                        nested.Str("code"),
                        nested.Req("message"),
                        nested.Str("param")
                    )
                    : new ErrorChunk(
                        chunk.Num("sequence_number") ?? 0,
                        null,
                        chunk.Str("code"),
                        chunk.Req("message"),
                        chunk.Str("param")
                    );
            default:
                throw new InvalidOperationException("Chunk type " + type + " is not modeled.");
        }
    }

    private static IOutputItem ReadOutputItem(JsonElement item) =>
        item.Req("type") switch
        {
            "message" => new Message(
                item.Req("id"),
                ReadPhase(item),
                [.. item.Prop("content").EnumerateArray().Select(ReadMessageContent)]
            ),
            "web_search_call" => new WebSearchCall(
                item.Req("id"),
                item.Req("status"),
                ReadWebSearchAction(item.Obj("action"))
            ),
            "file_search_call" => new FileSearchCall(
                item.Req("id"),
                ReadStrings(item.Prop("queries")),
                ReadFileSearchResults(item.Opt("results"))
            ),
            "code_interpreter_call" => new CodeInterpreterCall(
                item.Req("id"),
                item.Str("code"),
                item.Req("container_id"),
                ReadCodeInterpreterOutputs(item.Opt("outputs"))
            ),
            "image_generation_call" => new ImageGenerationCall(item.Req("id"), item.Req("result")),
            "local_shell_call" => new LocalShellCallOutput(ReadLocalShellCall(item)),
            "function_call" => new FunctionCall(
                item.Req("id"),
                item.Req("call_id"),
                item.Req("name"),
                item.Req("arguments"),
                item.Bool("async"),
                item.Str("namespace"),
                ReadToolCaller(item.Obj("caller"))
            ),
            "program" => ReadProgram(item),
            "program_output" => ReadProgramOutput(item),
            "custom_tool_call" => ReadCustomToolCall(item),
            "computer_call" => ReadComputerCall(item),
            "reasoning" => new Reasoning(
                item.Req("id"),
                item.Str("encrypted_content"),
                [.. item.Prop("summary").EnumerateArray().Select(summary => summary.Req("text"))]
            ),
            "mcp_call" => ReadMcpCall(item),
            "mcp_list_tools" => ReadMcpListTools(item),
            "mcp_approval_request" => ReadMcpApprovalRequest(item),
            "apply_patch_call" => ReadApplyPatchCall(item),
            "shell_call" => ReadShellCall(item),
            "compaction" => new Compaction(item.Req("id"), item.Req("encrypted_content")),
            "shell_call_output" => ReadShellCallOutput(item),
            "tool_search_call" => ReadToolSearchCall(item),
            "tool_search_output" => ReadToolSearchOutput(item),
            var other => throw new InvalidOperationException(
                "Unknown output item type " + other + "."
            ),
        };

    private static IAddedItem ReadAddedItem(JsonElement item) =>
        item.Req("type") switch
        {
            "message" => new AddedMessage(item.Req("id"), ReadPhase(item)),
            "reasoning" => new ReasoningItem(item.Req("id"), item.Str("encrypted_content")),
            "function_call" => new AddedFunctionCall(
                item.Req("id"),
                item.Req("call_id"),
                item.Req("name"),
                item.Req("arguments"),
                item.Bool("async"),
                item.Str("namespace"),
                ReadToolCaller(item.Obj("caller"))
            ),
            "program" => ReadProgram(item),
            "program_output" => ReadProgramOutput(item),
            "web_search_call" => new AddedWebSearchCall(item.Req("id"), item.Req("status")),
            "computer_call" => ReadComputerCall(item),
            "file_search_call" => new AddedFileSearchCall(item.Req("id")),
            "local_shell_call" => new AddedLocalShellCall(ReadLocalShellCall(item)),
            "image_generation_call" => new AddedImageGenerationCall(item.Req("id")),
            "code_interpreter_call" => new AddedCodeInterpreterCall(
                item.Req("id"),
                item.Req("container_id"),
                item.Str("code"),
                ReadCodeInterpreterOutputs(item.Opt("outputs")),
                item.Req("status")
            ),
            "mcp_call" => new AddedMcpCall(
                item.Req("id"),
                item.Req("status"),
                item.Str("approval_request_id")
            ),
            "mcp_list_tools" => new AddedMcpListTools(item.Req("id")),
            "mcp_approval_request" => new AddedMcpApprovalRequest(item.Req("id")),
            "apply_patch_call" => ReadApplyPatchCall(item),
            "custom_tool_call" => ReadCustomToolCall(item),
            "shell_call" => ReadShellCall(item),
            "compaction" => new AddedCompaction(item.Req("id"), item.Str("encrypted_content")),
            "shell_call_output" => ReadShellCallOutput(item),
            "tool_search_call" => ReadToolSearchCall(item),
            "tool_search_output" => ReadToolSearchOutput(item),
            var other => throw new InvalidOperationException(
                "Unknown output item type " + other + "."
            ),
        };

    private static IDoneItem ReadDoneItem(JsonElement item) =>
        item.Req("type") switch
        {
            "message" => new DoneMessage(item.Req("id"), ReadPhase(item)),
            "reasoning" => new ReasoningItem(item.Req("id"), item.Str("encrypted_content")),
            "function_call" => new DoneFunctionCall(
                item.Req("id"),
                item.Req("call_id"),
                item.Req("name"),
                item.Req("arguments"),
                item.Bool("async"),
                item.Req("status"),
                item.Str("namespace"),
                ReadToolCaller(item.Obj("caller"))
            ),
            "program" => ReadProgram(item),
            "program_output" => ReadProgramOutput(item),
            "custom_tool_call" => new DoneCustomToolCall(
                item.Req("id"),
                item.Req("call_id"),
                item.Req("name"),
                item.Req("input"),
                item.Bool("async"),
                item.Req("status")
            ),
            "code_interpreter_call" => new DoneCodeInterpreterCall(
                item.Req("id"),
                item.Str("code"),
                item.Req("container_id"),
                ReadCodeInterpreterOutputs(item.Opt("outputs"))
            ),
            "image_generation_call" => new ImageGenerationCall(item.Req("id"), item.Req("result")),
            "web_search_call" => new DoneWebSearchCall(
                item.Req("id"),
                item.Req("status"),
                ReadWebSearchAction(item.Obj("action"))
            ),
            "file_search_call" => new DoneFileSearchCall(
                item.Req("id"),
                ReadStrings(item.Prop("queries")),
                ReadFileSearchResults(item.Opt("results"))
            ),
            "local_shell_call" => new DoneLocalShellCall(ReadLocalShellCall(item)),
            "computer_call" => ReadComputerCall(item),
            "mcp_call" => ReadMcpCall(item),
            "mcp_list_tools" => ReadMcpListTools(item),
            "mcp_approval_request" => ReadMcpApprovalRequest(item),
            "apply_patch_call" => ReadApplyPatchCall(item),
            "shell_call" => ReadShellCall(item),
            "compaction" => new Compaction(item.Req("id"), item.Req("encrypted_content")),
            "shell_call_output" => ReadShellCallOutput(item),
            "tool_search_call" => ReadToolSearchCall(item),
            "tool_search_output" => ReadToolSearchOutput(item),
            var other => throw new InvalidOperationException(
                "Unknown output item type " + other + "."
            ),
        };

    private static MessagePhase? ReadPhase(JsonElement item) =>
        item.Str("phase") switch
        {
            "commentary" => MessagePhase.Commentary,
            "final_answer" => MessagePhase.FinalAnswer,
            _ => null,
        };

    private static MessageContent ReadMessageContent(JsonElement part) =>
        part.Req("type") == "refusal"
            ? new Refusal(part.Req("refusal"))
            : new OutputText(
                part.Req("text"),
                ReadLogprobs(part.Opt("logprobs")),
                [.. part.Prop("annotations").EnumerateArray().Select(ReadAnnotation)]
            );

    private static Annotation ReadAnnotation(JsonElement annotation) =>
        annotation.Req("type") switch
        {
            "url_citation" => new UrlCitation(
                annotation.Int("start_index"),
                annotation.Int("end_index"),
                annotation.Req("url"),
                annotation.Req("title")
            ),
            "file_citation" => new FileCitation(
                annotation.Req("file_id"),
                annotation.Req("filename"),
                annotation.Int("index")
            ),
            "container_file_citation" => new ContainerFileCitation(
                annotation.Req("container_id"),
                annotation.Req("file_id"),
                annotation.Req("filename"),
                annotation.Int("start_index"),
                annotation.Int("end_index")
            ),
            _ => new FilePath(annotation.Req("file_id"), annotation.Int("index")),
        };

    private static IReadOnlyList<TokenLogprob>? ReadLogprobs(JsonElement? logprobs) =>
        logprobs is { ValueKind: JsonValueKind.Array } list
            ?
            [
                .. list.EnumerateArray()
                    .Select(entry => new TokenLogprob(
                        entry.Req("token"),
                        entry.Num("logprob") ?? 0,
                        [
                            .. entry
                                .Prop("top_logprobs")
                                .EnumerateArray()
                                .Select(top => new TopLogprob(
                                    top.Req("token"),
                                    top.Num("logprob") ?? 0
                                )),
                        ]
                    )),
            ]
            : null;

    private static WebSearchAction? ReadWebSearchAction(JsonElement? action)
    {
        if (action is not { } value)
        {
            return null;
        }

        return value.Req("type") switch
        {
            "search" => new WebSearchSearch(
                value.Str("query"),
                value.Opt("queries") is { ValueKind: JsonValueKind.Array } queries
                    ? ReadStrings(queries)
                    : null,
                value.Opt("sources") is { ValueKind: JsonValueKind.Array } sources
                    ?
                    [
                        .. sources
                            .EnumerateArray()
                            .Select(source =>
                                source.Req("type") == "url"
                                    ? (WebSearchSource)new WebSearchUrlSource(source.Req("url"))
                                    : new WebSearchApiSource(source.Req("name"))
                            ),
                    ]
                    : null
            ),
            "open_page" => new WebSearchOpenPage(value.Str("url")),
            _ => new WebSearchFindInPage(value.Str("url"), value.Str("pattern")),
        };
    }

    private static IReadOnlyList<CodeInterpreterOutput>? ReadCodeInterpreterOutputs(
        JsonElement? outputs
    ) =>
        outputs is { ValueKind: JsonValueKind.Array } list
            ?
            [
                .. list.EnumerateArray()
                    .Select(output =>
                        output.Req("type") == "logs"
                            ? (CodeInterpreterOutput)new CodeInterpreterLogs(output.Req("logs"))
                            : new CodeInterpreterImage(output.Req("url"))
                    ),
            ]
            : null;

    private static IReadOnlyList<FileSearchResult>? ReadFileSearchResults(JsonElement? results) =>
        results is { ValueKind: JsonValueKind.Array } list
            ?
            [
                .. list.EnumerateArray()
                    .Select(result => new FileSearchResult(
                        result
                            .Prop("attributes")
                            .EnumerateObject()
                            .ToDictionary(
                                property => property.Name,
                                property => ReadAttribute(property.Value)
                            ),
                        result.Req("file_id"),
                        result.Req("filename"),
                        result.Num("score") ?? 0,
                        result.Req("text")
                    )),
            ]
            : null;

    private static object ReadAttribute(JsonElement value) =>
        value.ValueKind switch
        {
            JsonValueKind.String => Text(value),
            JsonValueKind.Number => value.GetDouble(),
            JsonValueKind.True => true,
            _ => false,
        };

    private static LocalShellCall ReadLocalShellCall(JsonElement item)
    {
        var action = item.Prop("action");
        return new LocalShellCall(
            item.Req("id"),
            item.Req("call_id"),
            new LocalShellAction(
                ReadStrings(action.Prop("command")),
                action.Num("timeout_ms"),
                action.Str("user"),
                action.Str("working_directory"),
                action.Obj("env") is { } env
                    ? env.EnumerateObject()
                        .ToDictionary(property => property.Name, property => Text(property.Value))
                    : null
            )
        );
    }

    private static ToolCaller? ReadToolCaller(JsonElement? caller) =>
        caller is { } value
            ? value.Req("type") == "program"
                ? new ProgramToolCaller(value.Req("caller_id"))
                : new DirectToolCaller()
            : null;

    private static Program ReadProgram(JsonElement item) =>
        new(item.Req("id"), item.Req("call_id"), item.Req("code"), item.Req("fingerprint"));

    private static ProgramOutput ReadProgramOutput(JsonElement item) =>
        new(item.Req("id"), item.Req("call_id"), item.Req("result"), item.Req("status"));

    private static CustomToolCall ReadCustomToolCall(JsonElement item) =>
        new(
            item.Req("id"),
            item.Req("call_id"),
            item.Req("name"),
            item.Req("input"),
            item.Bool("async")
        );

    private static ComputerCall ReadComputerCall(JsonElement item) =>
        new(
            item.Req("id"),
            item.Str("call_id"),
            item.Req("status"),
            item.Obj("action") is { } action ? ReadComputerAction(action) : null,
            item.Opt("actions") is { ValueKind: JsonValueKind.Array } actions
                ? [.. actions.EnumerateArray().Select(ReadComputerAction)]
                : null,
            item.Opt("pending_safety_checks") is { ValueKind: JsonValueKind.Array } checks
                ?
                [
                    .. checks
                        .EnumerateArray()
                        .Select(check => new PendingSafetyCheck(
                            check.Req("id"),
                            check.Str("code"),
                            check.Str("message")
                        )),
                ]
                : null
        );

    private static ComputerAction ReadComputerAction(JsonElement action) =>
        action.Req("type") switch
        {
            "click" => new ClickAction(
                action.Req("button"),
                action.Num("x") ?? 0,
                action.Num("y") ?? 0,
                ReadOptionalStrings(action.Opt("keys"))
            ),
            "double_click" => new DoubleClickAction(
                action.Num("x") ?? 0,
                action.Num("y") ?? 0,
                ReadOptionalStrings(action.Opt("keys"))
            ),
            "drag" => new DragAction(
                [
                    .. action
                        .Prop("path")
                        .EnumerateArray()
                        .Select(point => new ComputerPoint(
                            point.Num("x") ?? 0,
                            point.Num("y") ?? 0
                        )),
                ],
                ReadOptionalStrings(action.Opt("keys"))
            ),
            "keypress" => new KeypressAction(ReadStrings(action.Prop("keys"))),
            "move" => new MoveAction(
                action.Num("x") ?? 0,
                action.Num("y") ?? 0,
                ReadOptionalStrings(action.Opt("keys"))
            ),
            "screenshot" => new ScreenshotAction(),
            "scroll" => new ScrollAction(
                action.Num("x") ?? 0,
                action.Num("y") ?? 0,
                action.Num("scroll_x") ?? 0,
                action.Num("scroll_y") ?? 0,
                ReadOptionalStrings(action.Opt("keys"))
            ),
            "type" => new TypeAction(action.Req("text")),
            _ => new WaitAction(),
        };

    private static McpCall ReadMcpCall(JsonElement item) =>
        new(
            item.Req("id"),
            item.Req("status"),
            item.Req("arguments"),
            item.Req("name"),
            item.Req("server_label"),
            item.Str("output"),
            ReadErrorValue(item.Opt("error")),
            item.Str("approval_request_id")
        );

    private static McpListTools ReadMcpListTools(JsonElement item) =>
        new(
            item.Req("id"),
            item.Req("server_label"),
            [
                .. item.Prop("tools")
                    .EnumerateArray()
                    .Select(tool => new McpTool(
                        tool.Req("name"),
                        tool.Str("description"),
                        tool.Opt("input_schema")?.Clone(),
                        tool.Opt("annotations")?.Clone()
                    )),
            ],
            ReadErrorValue(item.Opt("error"))
        );

    private static McpApprovalRequest ReadMcpApprovalRequest(JsonElement item) =>
        new(
            item.Req("id"),
            item.Req("server_label"),
            item.Req("name"),
            item.Req("arguments"),
            item.Str("approval_request_id")
        );

    private static ErrorValue? ReadErrorValue(JsonElement? error) =>
        error switch
        {
            { ValueKind: JsonValueKind.String } text => new ErrorText(text.GetString()!),
            { ValueKind: JsonValueKind.Object } value => new ErrorObject(
                value.Str("type"),
                value.Opt("code") is { } code
                    ? (code.ValueKind == JsonValueKind.String ? Text(code) : code.GetRawText())
                    : null,
                value.Str("message")
            ),
            _ => null,
        };

    private static ApplyPatchCall ReadApplyPatchCall(JsonElement item)
    {
        var operation = item.Prop("operation");
        ApplyPatchOperation read = operation.Req("type") switch
        {
            "create_file" => new CreateFileOperation(operation.Req("path"), operation.Req("diff")),
            "delete_file" => new DeleteFileOperation(operation.Req("path")),
            _ => new UpdateFileOperation(operation.Req("path"), operation.Req("diff")),
        };
        return new ApplyPatchCall(item.Req("id"), item.Req("call_id"), item.Req("status"), read);
    }

    private static ShellCall ReadShellCall(JsonElement item)
    {
        var action = item.Prop("action");
        return new ShellCall(
            item.Req("id"),
            item.Req("call_id"),
            item.Req("status"),
            new ShellCallAction(
                ReadStrings(action.Prop("commands")),
                action.Num("timeout_ms"),
                action.Num("max_output_length")
            )
        );
    }

    private static ShellCallOutput ReadShellCallOutput(JsonElement item) =>
        new(
            item.Req("id"),
            item.Req("call_id"),
            item.Req("status"),
            item.Num("max_output_length"),
            [
                .. item.Prop("output")
                    .EnumerateArray()
                    .Select(entry => new ShellCallOutputEntry(
                        entry.Req("stdout"),
                        entry.Req("stderr"),
                        entry.Prop("outcome").Req("type") == "exit"
                            ? new ExitOutcome(entry.Prop("outcome").Num("exit_code") ?? 0)
                            : new TimeoutOutcome()
                    )),
            ]
        );

    private static ToolSearchCall ReadToolSearchCall(JsonElement item) =>
        new(
            item.Req("id"),
            item.Req("execution"),
            item.Str("call_id"),
            item.Req("status"),
            item.Opt("arguments")?.Clone()
        );

    private static ToolSearchOutput ReadToolSearchOutput(JsonElement item) =>
        new(
            item.Req("id"),
            item.Req("execution"),
            item.Str("call_id"),
            item.Req("status"),
            [.. item.Prop("tools").EnumerateArray().Select(tool => tool.Clone())]
        );

    private static Usage? ReadUsage(JsonElement? usage)
    {
        if (usage is not { } value)
        {
            return null;
        }

        var input = value.Obj("input_tokens_details");
        var output = value.Obj("output_tokens_details");
        return new Usage(
            value.Num("input_tokens") ?? 0,
            input is { } i
                ? new InputTokensDetails(
                    i.Num("cached_tokens"),
                    i.Num("cache_write_tokens"),
                    i.Obj("cache_write_tokens_details")?.Num("ttl_30m_tokens"),
                    i.Obj("cache_write_tokens_details")?.Num("ttl_12h_tokens"),
                    i.Num("orchestration_input_tokens"),
                    i.Num("orchestration_input_cached_tokens")
                )
                : null,
            value.Num("output_tokens") ?? 0,
            output is { } o
                ? new OutputTokensDetails(
                    o.Num("reasoning_tokens"),
                    o.Num("orchestration_output_tokens")
                )
                : null,
            value.Num("total_tokens")
        );
    }

    private static ResponseHeader ReadHeader(JsonElement response) =>
        new(
            response.Req("id"),
            response.Num("created_at") ?? 0,
            response.Req("model"),
            response.Str("service_tier")
        );

    private static FinishedResponse ReadFinished(JsonElement response)
    {
        var diagnostics = response.Obj("prompt_cache_diagnostics");
        var error = response.Obj("error");
        return new FinishedResponse(
            diagnostics is { } d
                ? new PromptCacheDiagnostics(
                    d.Req("type"),
                    d.Str("reason"),
                    d.Num("cache_missed_tokens"),
                    d.Num("comparison_reusable_tokens")
                )
                : null,
            response.Obj("incomplete_details")?.Req("reason"),
            ReadUsage(response.Obj("usage")),
            response.Obj("reasoning")?.Str("context"),
            response.Str("service_tier"),
            error is { } e ? new FailedResponseError(e.Str("code"), e.Req("message")) : null
        );
    }

    private static IReadOnlyList<string> ReadStrings(JsonElement array) =>
        [.. array.EnumerateArray().Select(Text)];

    private static IReadOnlyList<string>? ReadOptionalStrings(JsonElement? array) =>
        array is { ValueKind: JsonValueKind.Array } list ? ReadStrings(list) : null;

    private static JsonElement Prop(this JsonElement element, string name) =>
        element.GetProperty(name);

    /// <summary>The property, or <see langword="null"/> when it is absent or <c>null</c>.</summary>
    private static JsonElement? Opt(this JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(name, out var value)
        && value.ValueKind != JsonValueKind.Null
            ? value
            : null;

    private static JsonElement? Obj(this JsonElement element, string name) =>
        element.Opt(name) is { ValueKind: JsonValueKind.Object } value ? value : null;

    private static string? Str(this JsonElement element, string name) =>
        element.Opt(name) is { ValueKind: JsonValueKind.String } value ? Text(value) : null;

    private static string Req(this JsonElement element, string name) => Text(element.Prop(name));

    /// <summary>
    /// The string value. A JavaScript string can hold a lone surrogate (<c>"\ud83d"</c>) and the API relays it, but <see cref="JsonElement.GetString"/> throws for it.
    /// This reads those escapes one by one, so the value survives untouched.
    /// </summary>
    private static string Text(JsonElement value)
    {
        try
        {
            return value.GetString()!;
        }
        catch (InvalidOperationException)
        {
            var raw = value.GetRawText();
            var text = new System.Text.StringBuilder(raw.Length);
            for (var i = 1; i < raw.Length - 1; i++)
            {
                var c = raw[i];
                if (c != '\\')
                {
                    text.Append(c);
                    continue;
                }

                c = raw[++i];
                switch (c)
                {
                    case 'u':
                        text.Append(
                            (char)
                                int.Parse(
                                    raw.Substring(i + 1, Math.Min(4, raw.Length - i - 2)),
                                    NumberStyles.HexNumber,
                                    CultureInfo.InvariantCulture
                                )
                        );
                        i += 4;
                        break;
                    case 'b':
                        text.Append('\b');
                        break;
                    case 'f':
                        text.Append('\f');
                        break;
                    case 'n':
                        text.Append('\n');
                        break;
                    case 'r':
                        text.Append('\r');
                        break;
                    case 't':
                        text.Append('\t');
                        break;
                    default:
                        text.Append(c);
                        break;
                }
            }

            return text.ToString();
        }
    }

    private static double? Num(this JsonElement element, string name) =>
        element.Opt(name) is { ValueKind: JsonValueKind.Number } value ? value.GetDouble() : null;

    private static int Int(this JsonElement element, string name) => (int)(element.Num(name) ?? 0);

    private static bool? Bool(this JsonElement element, string name) =>
        element.Opt(name) is { } value
        && value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : null;
}
