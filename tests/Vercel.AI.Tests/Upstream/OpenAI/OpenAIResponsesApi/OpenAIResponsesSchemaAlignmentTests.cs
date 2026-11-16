// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Tests.Upstream;
using A = Vercel.AI.OpenAI.OpenAIResponsesApi;

namespace Vercel.AI.Tests.OpenAIResponsesApi;

/// <summary>
/// Port of <c>openai-responses-api.test.ts</c> &gt; <c>openaiResponses schema alignment</c>.
/// Upstream compares inferred TypeScript types with <c>expectTypeOf</c>. Each test compares the C# types of the typed models in
/// <see cref="OpenAIResponsesApi"/> by reflection, and also feeds the same valid and invalid value into the chunk and the response rule set and requires the same verdict.
/// </summary>
public sealed class OpenAIResponsesSchemaAlignmentTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-api.test.ts::openaiResponses schema alignment::";

    private static Type PropertyType<T>(string name) =>
        typeof(T).GetProperty(name)?.PropertyType ?? throw new InvalidOperationException(typeof(T).Name + "." + name + " does not exist.");

    private static Type ElementType(Type list) => list.GetGenericArguments().Single();

    private static void AssertEqualTypes(Type expected, Type actual) => Assert.Equal(expected, actual);

    private static bool Chunk(string json) => OpenAIResponsesSchema.IsValidChunk(OpenAIUpstream.Json(json));

    private static bool Response(string json) => OpenAIResponsesSchema.IsValidResponse(OpenAIUpstream.Json(json));

    private static string Output(string item) => "{\"output\":[" + item + "]}";

    private static string Item(string type, string item) => "{\"type\":\"response.output_item." + type + "\",\"output_index\":0,\"item\":" + item + "}";

    private static string Message(string content, string phase = "null") =>
        "{\"type\":\"message\",\"role\":\"assistant\",\"id\":\"m\",\"phase\":" + phase + ",\"content\":" + content + "}";

    private static void AssertAligned(string[] valid, string[] invalid, Func<string, string> asChunk, Func<string, string> asResponse)
    {
        foreach (var value in valid)
        {
            Assert.True(Chunk(asChunk(value)), "chunk accepts " + value);
            Assert.True(Response(asResponse(value)), "response accepts " + value);
        }

        foreach (var value in invalid)
        {
            Assert.False(Chunk(asChunk(value)), "chunk rejects " + value);
            Assert.False(Response(asResponse(value)), "response rejects " + value);
        }
    }

    private static void Verdict_Matches_annotation_shape_between_chunk_and_response_schemas() => AssertAligned(
        [
            """{"type":"url_citation","start_index":0,"end_index":1,"url":"u","title":"t"}""",
            """{"type":"file_citation","file_id":"f","filename":"n","index":0}""",
            """{"type":"container_file_citation","container_id":"c","file_id":"f","filename":"n","start_index":0,"end_index":1}""",
            """{"type":"file_path","file_id":"f","index":0}""",
        ],
        [
            """{"type":"url_citation","start_index":"0","end_index":1,"url":"u","title":"t"}""",
            """{"type":"file_path","file_id":"f"}""",
            """{"type":"unknown"}""",
        ],
        annotation => "{\"type\":\"response.output_text.annotation.added\",\"annotation\":" + annotation + "}",
        annotation => Output(Message("[{\"type\":\"output_text\",\"text\":\"t\",\"annotations\":[" + annotation + "]}]")));

    private static void Verdict_Aligns_web_search_call_actions() => AssertAligned(
        [
            """{"type":"search","query":"q","sources":[{"type":"url","url":"u"},{"type":"api","name":"n"}]}""",
            """{"type":"open_page","url":"u"}""",
            """{"type":"find_in_page","url":"u","pattern":"p"}""",
            "null",
        ],
        [
            """{"type":"search","queries":"q"}""",
            """{"type":"open_page","url":1}""",
            """{"type":"click"}""",
        ],
        action => Item("done", "{\"type\":\"web_search_call\",\"id\":\"w\",\"status\":\"completed\",\"action\":" + action + "}"),
        action => Output("{\"type\":\"web_search_call\",\"id\":\"w\",\"status\":\"completed\",\"action\":" + action + "}"));

    private static void Verdict_Aligns_code_interpreter_outputs() => AssertAligned(
        [
            """[{"type":"logs","logs":"l"},{"type":"image","url":"u"}]""",
            "null",
        ],
        [
            """[{"type":"logs","logs":1}]""",
            """[{"type":"other"}]""",
            "\"x\"",
        ],
        outputs => Item("done", "{\"type\":\"code_interpreter_call\",\"id\":\"c\",\"code\":null,\"container_id\":\"k\",\"outputs\":" + outputs + "}"),
        outputs => Output("{\"type\":\"code_interpreter_call\",\"id\":\"c\",\"code\":null,\"container_id\":\"k\",\"outputs\":" + outputs + "}"));

    private static void Verdict_Aligns_file_search_call_results() => AssertAligned(
        [
            """[{"attributes":{"a":"s","b":1,"c":true},"file_id":"f","filename":"n","score":0.5,"text":"t"}]""",
            "null",
        ],
        [
            """[{"attributes":{"a":null},"file_id":"f","filename":"n","score":0.5,"text":"t"}]""",
            """[{"file_id":"f"}]""",
        ],
        results => Item("done", "{\"type\":\"file_search_call\",\"id\":\"s\",\"queries\":[],\"results\":" + results + "}"),
        results => Output("{\"type\":\"file_search_call\",\"id\":\"s\",\"queries\":[],\"results\":" + results + "}"));

    private static void Verdict_Aligns_message_phase_between_added_chunk_done_chunk_and_response_schemas()
    {
        foreach (var phase in new[] { "\"commentary\"", "\"final_answer\"", "null" })
        {
            Assert.True(Chunk(Item("added", "{\"type\":\"message\",\"id\":\"m\",\"phase\":" + phase + "}")));
            Assert.True(Chunk(Item("done", "{\"type\":\"message\",\"id\":\"m\",\"phase\":" + phase + "}")));
            Assert.True(Response(Output(Message("[]", phase))));
        }

        foreach (var phase in new[] { "\"other\"", "1" })
        {
            Assert.False(Chunk(Item("added", "{\"type\":\"message\",\"id\":\"m\",\"phase\":" + phase + "}")));
            Assert.False(Chunk(Item("done", "{\"type\":\"message\",\"id\":\"m\",\"phase\":" + phase + "}")));
            Assert.False(Response(Output(Message("[]", phase))));
        }
    }

    private static void Verdict_Aligns_async_mode_between_function_call_schemas()
    {
        const string Fields = "\"id\":\"f\",\"call_id\":\"c\",\"name\":\"n\",\"arguments\":\"{}\"";
        foreach (var async in new[] { "true", "false", "null" })
        {
            Assert.True(Chunk(Item("added", "{\"type\":\"function_call\"," + Fields + ",\"async\":" + async + "}")));
            Assert.True(Chunk(Item("done", "{\"type\":\"function_call\"," + Fields + ",\"status\":\"completed\",\"async\":" + async + "}")));
            Assert.True(Response(Output("{\"type\":\"function_call\"," + Fields + ",\"async\":" + async + "}")));
        }

        Assert.False(Chunk(Item("added", "{\"type\":\"function_call\"," + Fields + ",\"async\":\"yes\"}")));
        Assert.False(Chunk(Item("done", "{\"type\":\"function_call\"," + Fields + ",\"status\":\"completed\",\"async\":\"yes\"}")));
        Assert.False(Response(Output("{\"type\":\"function_call\"," + Fields + ",\"async\":\"yes\"}")));
    }

    private static void Verdict_Aligns_output_text_logprobs() => AssertAligned(
        [
            """[{"token":"a","logprob":-0.1,"top_logprobs":[{"token":"a","logprob":-0.1}]}]""",
            "null",
        ],
        [
            """[{"token":"a","logprob":"x","top_logprobs":[]}]""",
            """[{"token":"a","logprob":-0.1}]""",
        ],
        logprobs => "{\"type\":\"response.output_text.delta\",\"item_id\":\"m\",\"delta\":\"d\",\"logprobs\":" + logprobs + "}",
        logprobs => Output(Message("[{\"type\":\"output_text\",\"text\":\"t\",\"logprobs\":" + logprobs + ",\"annotations\":[]}]")));

    private static void Verdict_Aligns_local_shell_call_between_added_done_and_response_schemas()
    {
        string[] valid =
        [
            """{"type":"local_shell_call","id":"l","call_id":"c","action":{"type":"exec","command":["ls"]}}""",
            """{"type":"local_shell_call","id":"l","call_id":"c","action":{"type":"exec","command":["ls"],"timeout_ms":1,"user":"u","working_directory":"/","env":{"A":"b"}}}""",
        ];
        string[] invalid =
        [
            """{"type":"local_shell_call","id":"l","call_id":"c","action":{"type":"run","command":["ls"]}}""",
            """{"type":"local_shell_call","id":"l","call_id":"c","action":{"type":"exec","command":["ls"],"env":{"A":1}}}""",
            """{"type":"local_shell_call","id":"l","action":{"type":"exec","command":["ls"]}}""",
        ];

        AssertAligned(valid, invalid, item => Item("added", item), Output);
        AssertAligned(valid, invalid, item => Item("done", item), Output);
    }

    [Fact]
    [UpstreamTest(Prefix + "matches annotation shape between chunk and response schemas")]
    public void Matches_annotation_shape_between_chunk_and_response_schemas()
    {
        AssertEqualTypes(PropertyType<A.OutputTextAnnotationAdded>("Annotation"), ElementType(PropertyType<A.OutputText>("Annotations")));
        Verdict_Matches_annotation_shape_between_chunk_and_response_schemas();
    }

    [Fact]
    [UpstreamTest(Prefix + "aligns web_search_call actions")]
    public void Aligns_web_search_call_actions()
    {
        AssertEqualTypes(PropertyType<A.DoneWebSearchCall>("Action"), PropertyType<A.WebSearchCall>("Action"));
        Verdict_Aligns_web_search_call_actions();
    }

    [Fact]
    [UpstreamTest(Prefix + "aligns code_interpreter outputs")]
    public void Aligns_code_interpreter_outputs()
    {
        AssertEqualTypes(PropertyType<A.DoneCodeInterpreterCall>("Outputs"), PropertyType<A.CodeInterpreterCall>("Outputs"));
        Verdict_Aligns_code_interpreter_outputs();
    }

    [Fact]
    [UpstreamTest(Prefix + "aligns file_search_call results")]
    public void Aligns_file_search_call_results()
    {
        AssertEqualTypes(PropertyType<A.DoneFileSearchCall>("Results"), PropertyType<A.FileSearchCall>("Results"));
        Verdict_Aligns_file_search_call_results();
    }

    [Fact]
    [UpstreamTest(Prefix + "aligns message phase between added chunk, done chunk, and response schemas")]
    public void Aligns_message_phase_between_added_chunk_done_chunk_and_response_schemas()
    {
        AssertEqualTypes(PropertyType<A.AddedMessage>("Phase"), PropertyType<A.DoneMessage>("Phase"));
        AssertEqualTypes(PropertyType<A.DoneMessage>("Phase"), PropertyType<A.Message>("Phase"));
        Verdict_Aligns_message_phase_between_added_chunk_done_chunk_and_response_schemas();
    }

    [Fact]
    [UpstreamTest(Prefix + "aligns async mode between function call schemas")]
    public void Aligns_async_mode_between_function_call_schemas()
    {
        AssertEqualTypes(PropertyType<A.AddedFunctionCall>("Async"), PropertyType<A.DoneFunctionCall>("Async"));
        AssertEqualTypes(PropertyType<A.DoneFunctionCall>("Async"), PropertyType<A.FunctionCall>("Async"));
        Verdict_Aligns_async_mode_between_function_call_schemas();
    }

    [Fact]
    [UpstreamTest(Prefix + "aligns output_text logprobs")]
    public void Aligns_output_text_logprobs()
    {
        AssertEqualTypes(PropertyType<A.OutputTextDelta>("Logprobs"), PropertyType<A.OutputText>("Logprobs"));
        Verdict_Aligns_output_text_logprobs();
    }

    [Fact]
    [UpstreamTest(Prefix + "aligns local_shell_call between added, done, and response schemas")]
    public void Aligns_local_shell_call_between_added_done_and_response_schemas()
    {
        AssertEqualTypes(PropertyType<A.AddedLocalShellCall>("Call"), PropertyType<A.DoneLocalShellCall>("Call"));
        AssertEqualTypes(PropertyType<A.DoneLocalShellCall>("Call"), PropertyType<A.LocalShellCallOutput>("Call"));
        Verdict_Aligns_local_shell_call_between_added_done_and_response_schemas();
    }
}
