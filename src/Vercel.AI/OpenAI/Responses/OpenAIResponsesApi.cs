// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.OpenAI;

/// <summary>
/// Typed models of the shapes that <c>openaiResponsesChunkSchema</c> and <c>openaiResponsesResponseSchema</c> share.
/// Upstream infers these types from the zod schemas (<c>InferSchema</c>) and its alignment tests compare them with <c>expectTypeOf</c>.
/// <see cref="OpenAIResponsesSchema"/> validates the JSON; these types describe what a valid value holds.
/// A shape that a chunk and a response both carry is one type here, so the two cannot drift apart.
/// </summary>
internal static class OpenAIResponsesApi
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
    public sealed record UrlCitation(int StartIndex, int EndIndex, string Url, string Title) : Annotation;

    /// <summary><c>file_citation</c>.</summary>
    public sealed record FileCitation(string FileId, string Filename, int Index) : Annotation;

    /// <summary><c>container_file_citation</c>.</summary>
    public sealed record ContainerFileCitation(string ContainerId, string FileId, string Filename, int StartIndex, int EndIndex) : Annotation;

    /// <summary><c>file_path</c>.</summary>
    public sealed record FilePath(string FileId, int Index) : Annotation;

    /// <summary>A log probability of an output token.</summary>
    public sealed record TopLogprob(string Token, double Logprob);

    /// <summary>A token with its log probability and the top alternatives.</summary>
    public sealed record TokenLogprob(string Token, double Logprob, IReadOnlyList<TopLogprob> TopLogprobs);

    /// <summary>A source of a web search.</summary>
    public abstract record WebSearchSource;

    /// <summary>A <c>url</c> source.</summary>
    public sealed record WebSearchUrlSource(string Url) : WebSearchSource;

    /// <summary>An <c>api</c> source.</summary>
    public sealed record WebSearchApiSource(string Name) : WebSearchSource;

    /// <summary>The action of a <c>web_search_call</c>.</summary>
    public abstract record WebSearchAction;

    /// <summary><c>search</c>.</summary>
    public sealed record WebSearchSearch(string? Query, IReadOnlyList<string>? Queries, IReadOnlyList<WebSearchSource>? Sources) : WebSearchAction;

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
    public sealed record FileSearchResult(IReadOnlyDictionary<string, object> Attributes, string FileId, string Filename, double Score, string Text);

    /// <summary>The <c>exec</c> action of a <c>local_shell_call</c>.</summary>
    public sealed record LocalShellAction(
        IReadOnlyList<string> Command,
        double? TimeoutMs,
        string? User,
        string? WorkingDirectory,
        IReadOnlyDictionary<string, string>? Env);

    /// <summary>A <c>local_shell_call</c> item. The added chunk, the done chunk, and the response carry the same shape.</summary>
    public sealed record LocalShellCall(string Id, string CallId, LocalShellAction Action);

    /// <summary>A content part of an assistant message.</summary>
    public abstract record MessageContent;

    /// <summary><c>output_text</c>.</summary>
    public sealed record OutputText(string Text, IReadOnlyList<TokenLogprob>? Logprobs, IReadOnlyList<Annotation> Annotations) : MessageContent;

    /// <summary><c>refusal</c>.</summary>
    public sealed record Refusal(string RefusalText) : MessageContent;

    /// <summary>An item of <c>output</c> in a response (<c>openaiResponsesResponseSchema</c>).</summary>
    public abstract record OutputItem;

    /// <summary>A <c>message</c> output item.</summary>
    public sealed record Message(string Id, MessagePhase? Phase, IReadOnlyList<MessageContent> Content) : OutputItem;

    /// <summary>A <c>web_search_call</c> output item.</summary>
    public sealed record WebSearchCall(string Id, string Status, WebSearchAction? Action) : OutputItem;

    /// <summary>A <c>code_interpreter_call</c> output item.</summary>
    public sealed record CodeInterpreterCall(string Id, string? Code, string ContainerId, IReadOnlyList<CodeInterpreterOutput>? Outputs) : OutputItem;

    /// <summary>A <c>file_search_call</c> output item.</summary>
    public sealed record FileSearchCall(string Id, IReadOnlyList<string> Queries, IReadOnlyList<FileSearchResult>? Results) : OutputItem;

    /// <summary>A <c>function_call</c> output item.</summary>
    public sealed record FunctionCall(string Id, string CallId, string Name, string Arguments, bool? Async) : OutputItem;

    /// <summary>A <c>local_shell_call</c> output item.</summary>
    public sealed record LocalShellCallOutput(LocalShellCall Call) : OutputItem;

    /// <summary>The part of <c>openaiResponsesResponseSchema</c> that these models cover.</summary>
    public sealed record Response(IReadOnlyList<OutputItem>? Output);

    /// <summary>A stream chunk (<c>openaiResponsesChunkSchema</c>).</summary>
    public abstract record Chunk;

    /// <summary><c>response.output_text.annotation.added</c>.</summary>
    public sealed record OutputTextAnnotationAdded(Annotation Annotation) : Chunk;

    /// <summary><c>response.output_text.delta</c>.</summary>
    public sealed record OutputTextDelta(string ItemId, string Delta, IReadOnlyList<TokenLogprob>? Logprobs) : Chunk;

    /// <summary>An item of <c>response.output_item.added</c>.</summary>
    public abstract record AddedItem;

    /// <summary>An item of <c>response.output_item.done</c>.</summary>
    public abstract record DoneItem;

    /// <summary>A <c>message</c> item of an added chunk.</summary>
    public sealed record AddedMessage(string Id, MessagePhase? Phase) : AddedItem;

    /// <summary>A <c>message</c> item of a done chunk.</summary>
    public sealed record DoneMessage(string Id, MessagePhase? Phase) : DoneItem;

    /// <summary>A <c>function_call</c> item of an added chunk.</summary>
    public sealed record AddedFunctionCall(string Id, string CallId, string Name, string Arguments, bool? Async) : AddedItem;

    /// <summary>A <c>function_call</c> item of a done chunk.</summary>
    public sealed record DoneFunctionCall(string Id, string CallId, string Name, string Arguments, bool? Async, string Status) : DoneItem;

    /// <summary>A <c>local_shell_call</c> item of an added chunk.</summary>
    public sealed record AddedLocalShellCall(LocalShellCall Call) : AddedItem;

    /// <summary>A <c>local_shell_call</c> item of a done chunk.</summary>
    public sealed record DoneLocalShellCall(LocalShellCall Call) : DoneItem;

    /// <summary>A <c>web_search_call</c> item of a done chunk.</summary>
    public sealed record DoneWebSearchCall(string Id, string Status, WebSearchAction? Action) : DoneItem;

    /// <summary>A <c>code_interpreter_call</c> item of a done chunk.</summary>
    public sealed record DoneCodeInterpreterCall(string Id, string? Code, string ContainerId, IReadOnlyList<CodeInterpreterOutput>? Outputs) : DoneItem;

    /// <summary>A <c>file_search_call</c> item of a done chunk.</summary>
    public sealed record DoneFileSearchCall(string Id, IReadOnlyList<string> Queries, IReadOnlyList<FileSearchResult>? Results) : DoneItem;
}
