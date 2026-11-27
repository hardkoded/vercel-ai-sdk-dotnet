// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.ConvertToAnthropicPrompt.PromptRunner;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>assistant messages</c>.</summary>
public sealed class AssistantMessagesTests
{
    private const string Prefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::assistant messages::";

    [Fact]
    [UpstreamTest(
        Prefix + "should ignore message-start accounting parts when replaying assistant content",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_ignore_message_start_accounting_parts_when_replaying_assistant_content()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"type":"custom","kind":"anthropic.message_start","providerOptions":{"anthropic":{"id":"msg_usage","model":"claude-haiku-4-5","usage":{"input_tokens":13,"output_tokens":1}}}},
              {"type":"text","text":"Hi!"}]}]
            """,
            sendReasoning: false
        );

        AssertJson(
            """[{"role":"assistant","content":[{"type":"text","text":"Hi!"}]}]""",
            result.Messages
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve fallback boundaries between reasoning blocks",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_fallback_boundaries_between_reasoning_blocks()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"type":"reasoning","text":"Primary model thinking","providerOptions":{"anthropic":{"signature":"primary-signature"}}},
              {"type":"custom","kind":"anthropic.fallback","providerOptions":{"anthropic":{"type":"fallback","from":{"model":"claude-opus-5-5"},"to":{"model":"claude-opus-4-8"}}}},
              {"type":"reasoning","text":"Fallback model thinking","providerOptions":{"anthropic":{"signature":"fallback-signature"}}}]}]
            """,
            sendReasoning: true
        );

        AssertJson(
            """
            [{"role":"assistant","content":[
              {"type":"thinking","thinking":"Primary model thinking","signature":"primary-signature"},
              {"type":"fallback","from":{"model":"claude-opus-5-5"},"to":{"model":"claude-opus-4-8"}},
              {"type":"thinking","thinking":"Fallback model thinking","signature":"fallback-signature"}]}]
            """,
            result.Messages
        );
        Assert.Contains("server-side-fallback-2026-06-01", result.Betas);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn and omit fallback boundaries with invalid metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_warn_and_omit_fallback_boundaries_with_invalid_metadata()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"type":"custom","kind":"anthropic.fallback","providerOptions":{"anthropic":{"type":"fallback","from":{"model":"claude-opus-5-5"}}}}]}]
            """,
            sendReasoning: true
        );

        AssertJson("[]", result.Messages);
        AssertWarnings(result, "anthropic fallback metadata must include from.model and to.model");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit an assistant message that only contains an empty compaction block",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_omit_an_assistant_message_that_only_contains_an_empty_compaction_block()
    {
        var result = Run(
            """
            [{"role":"user","content":[{"type":"text","text":"first user message"}]},
             {"role":"assistant","content":[{"type":"text","text":"","providerOptions":{"anthropic":{"type":"compaction"}}}]},
             {"role":"user","content":[{"type":"text","text":"second user message"}]}]
            """,
            sendReasoning: true
        );

        AssertJson(
            """
            [{"role":"user","content":[{"type":"text","text":"first user message"}]},
             {"role":"user","content":[{"type":"text","text":"second user message"}]}]
            """,
            result.Messages
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve citations on assistant text",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_citations_on_assistant_text()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[{"type":"text","text":"The Federal Reserve held rates steady.","providerOptions":{"anthropic":{"citations":[{"type":"web_search_result_location","cited_text":"The Committee decided to maintain the rate.","url":"https://example.com/fed-decision","title":"Federal Reserve decision","encrypted_index":"encrypted-index"}]}}}]},
             {"role":"user","content":[{"type":"text","text":"What happened before that?"}]}]
            """,
            sendReasoning: true
        );

        AssertJson(
            """
            [{"content":[{"citations":[{"cited_text":"The Committee decided to maintain the rate.","encrypted_index":"encrypted-index","title":"Federal Reserve decision","type":"web_search_result_location","url":"https://example.com/fed-decision"}],"text":"The Federal Reserve held rates steady.","type":"text"}],"role":"assistant"},
             {"content":[{"text":"What happened before that?","type":"text"}],"role":"user"}]
            """,
            result.Messages
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should remove trailing whitespace from last assistant message with multi-part content when there is no further user message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_remove_trailing_whitespace_from_last_assistant_message_with_multi_part_content_when_there_is_no_further_user_message()
    {
        var result = Run(
            """
            [{"role":"user","content":[{"type":"text","text":"user content"}]},
             {"role":"assistant","content":[{"type":"text","text":"assistant "},{"type":"text","text":"content  "}]}]
            """,
            sendReasoning: true
        );

        AssertJson(
            """
            [{"role":"user","content":[{"type":"text","text":"user content"}]},
             {"role":"assistant","content":[{"type":"text","text":"assistant "},{"type":"text","text":"content"}]}]
            """,
            result.Messages
        );
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should keep trailing whitespace from assistant message when there is a further user message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_keep_trailing_whitespace_from_assistant_message_when_there_is_a_further_user_message()
    {
        var result = Run(
            """
            [{"role":"user","content":[{"type":"text","text":"user content"}]},
             {"role":"assistant","content":[{"type":"text","text":"assistant content  "}]},
             {"role":"user","content":[{"type":"text","text":"user content 2"}]}]
            """,
            sendReasoning: true
        );

        AssertJson(
            """
            [{"role":"user","content":[{"type":"text","text":"user content"}]},
             {"role":"assistant","content":[{"type":"text","text":"assistant content  "}]},
             {"role":"user","content":[{"type":"text","text":"user content 2"}]}]
            """,
            result.Messages
        );
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should combine multiple sequential assistant messages into a single message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_combine_multiple_sequential_assistant_messages_into_a_single_message()
    {
        var result = Run(
            """
            [{"role":"user","content":[{"type":"text","text":"Hi!"}]},
             {"role":"assistant","content":[{"type":"text","text":"Hello"}]},
             {"role":"assistant","content":[{"type":"text","text":"World"}]},
             {"role":"assistant","content":[{"type":"text","text":"!"}]}]
            """,
            sendReasoning: true
        );

        AssertJson(
            """
            [{"role":"user","content":[{"type":"text","text":"Hi!"}]},
             {"role":"assistant","content":[{"type":"text","text":"Hello"},{"type":"text","text":"World"},{"type":"text","text":"!"}]}]
            """,
            result.Messages
        );
        Assert.Empty(result.Betas);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should convert assistant message reasoning parts with signature into thinking parts when sendReasoning is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_assistant_message_reasoning_parts_with_signature_into_thinking_parts_when_sendReasoning_is_true()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"type":"reasoning","text":"I need to count the number of \"r\"s in the word \"strawberry\".","providerOptions":{"anthropic":{"signature":"test-signature"}}},
              {"type":"text","text":"The word \"strawberry\" has 2 \"r\"s."}]}]
            """,
            sendReasoning: true
        );

        AssertJson(
            """
            [{"role":"assistant","content":[
              {"type":"thinking","thinking":"I need to count the number of \"r\"s in the word \"strawberry\".","signature":"test-signature"},
              {"type":"text","text":"The word \"strawberry\" has 2 \"r\"s."}]}]
            """,
            result.Messages
        );
        Assert.Empty(result.Betas);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should ignore reasoning parts without signature into thinking parts when sendReasoning is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_ignore_reasoning_parts_without_signature_into_thinking_parts_when_sendReasoning_is_true()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"type":"reasoning","text":"I need to count the number of \"r\"s in the word \"strawberry\"."},
              {"type":"text","text":"The word \"strawberry\" has 2 \"r\"s."}]}]
            """,
            sendReasoning: true
        );

        AssertJson(
            """
            [{"role":"assistant","content":[{"type":"text","text":"The word \"strawberry\" has 2 \"r\"s."}]}]
            """,
            result.Messages
        );
        Assert.Null(result.System);
        Assert.Empty(result.Betas);
        AssertWarnings(result, "unsupported reasoning metadata");
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should omit assistant message reasoning parts with signature when sendReasoning is false",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_omit_assistant_message_reasoning_parts_with_signature_when_sendReasoning_is_false()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"type":"reasoning","text":"I need to count the number of \"r\"s in the word \"strawberry\".","providerOptions":{"anthropic":{"signature":"test-signature"}}},
              {"type":"text","text":"The word \"strawberry\" has 2 \"r\"s."}]}]
            """,
            sendReasoning: false
        );

        AssertJson(
            """
            [{"role":"assistant","content":[{"type":"text","text":"The word \"strawberry\" has 2 \"r\"s."}]}]
            """,
            result.Messages
        );
        Assert.Empty(result.Betas);
        AssertWarnings(result, "sending reasoning content is disabled for this model");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit reasoning parts without signature when sendReasoning is false",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_omit_reasoning_parts_without_signature_when_sendReasoning_is_false()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"type":"reasoning","text":"I need to count the number of \"r\"s in the word \"strawberry\"."},
              {"type":"text","text":"The word \"strawberry\" has 2 \"r\"s."}]}]
            """,
            sendReasoning: false
        );

        AssertJson(
            """
            [{"role":"assistant","content":[{"type":"text","text":"The word \"strawberry\" has 2 \"r\"s."}]}]
            """,
            result.Messages
        );
        Assert.Empty(result.Betas);
        AssertWarnings(result, "sending reasoning content is disabled for this model");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert anthropic web_search tool call and result parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_anthropic_web_search_tool_call_and_result_parts()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"input":{"query":"San Francisco major news events June 22 2025"},"providerExecuted":true,"toolCallId":"srvtoolu_011cNtbtzFARKPcAcp7w4nh9","toolName":"web_search","type":"tool-call"},
              {"output":{"type":"json","value":[{"url":"https://patch.com/california/san-francisco/calendar","title":"San Francisco Calendar","pageAge":null,"encryptedContent":"encrypted-content","type":"web_search_result"}]},"toolCallId":"srvtoolu_011cNtbtzFARKPcAcp7w4nh9","toolName":"web_search","type":"tool-result"}]}]
            """,
            sendReasoning: false
        );

        AssertJson(
            """
            [{"content":[
              {"id":"srvtoolu_011cNtbtzFARKPcAcp7w4nh9","input":{"query":"San Francisco major news events June 22 2025"},"name":"web_search","type":"server_tool_use"},
              {"content":[{"encrypted_content":"encrypted-content","page_age":null,"title":"San Francisco Calendar","type":"web_search_result","url":"https://patch.com/california/san-francisco/calendar"}],"tool_use_id":"srvtoolu_011cNtbtzFARKPcAcp7w4nh9","type":"web_search_tool_result"}],
              "role":"assistant"}]
            """,
            result.Messages
        );
        Assert.Null(result.System);
        Assert.Empty(result.Betas);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should move regular tool_use blocks after provider-executed web_search results",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_move_regular_tool_use_blocks_after_provider_executed_web_search_results()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"type":"text","text":"I will save a note and search the web."},
              {"type":"tool-call","toolCallId":"toolu_regular","toolName":"saveNote","input":{"note":"Searching for basketball news"}},
              {"type":"tool-call","toolCallId":"srvtoolu_web_search","toolName":"web_search","providerExecuted":true,"input":{"query":"basketball news today"}},
              {"type":"tool-result","toolCallId":"srvtoolu_web_search","toolName":"web_search","output":{"type":"json","value":[{"url":"https://www.nba.com/news","title":"NBA News","pageAge":"1 hour ago","encryptedContent":"encrypted-content","type":"web_search_result"}]}}]},
             {"role":"tool","content":[{"type":"tool-result","toolCallId":"toolu_regular","toolName":"saveNote","output":{"type":"json","value":{"success":true}}}]}]
            """,
            sendReasoning: false
        );

        AssertJson(
            """
            [{"role":"assistant","content":[
               {"type":"text","text":"I will save a note and search the web."},
               {"type":"server_tool_use","id":"srvtoolu_web_search","name":"web_search","input":{"query":"basketball news today"}},
               {"type":"web_search_tool_result","tool_use_id":"srvtoolu_web_search","content":[{"url":"https://www.nba.com/news","title":"NBA News","page_age":"1 hour ago","encrypted_content":"encrypted-content","type":"web_search_result"}]},
               {"type":"tool_use","id":"toolu_regular","name":"saveNote","input":{"note":"Searching for basketball news"}}]},
             {"role":"user","content":[{"type":"tool_result","tool_use_id":"toolu_regular","content":"{\"success\":true}"}]}]
            """,
            result.Messages
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not move regular tool_use blocks across thinking blocks",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_move_regular_tool_use_blocks_across_thinking_blocks()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"type":"reasoning","text":"Think before the initial note.","providerOptions":{"anthropic":{"signature":"test-signature-1"}}},
              {"type":"tool-call","toolCallId":"toolu_initial","toolName":"saveNote","input":{"note":"phase 1: initial plan"}},
              {"type":"reasoning","text":"Think before the revised note.","providerOptions":{"anthropic":{"signature":"test-signature-2"}}},
              {"type":"tool-call","toolCallId":"toolu_revised","toolName":"saveNote","input":{"note":"phase 2: revised plan"}}]},
             {"role":"tool","content":[
              {"type":"tool-result","toolCallId":"toolu_initial","toolName":"saveNote","output":{"type":"json","value":{"success":true}}},
              {"type":"tool-result","toolCallId":"toolu_revised","toolName":"saveNote","output":{"type":"json","value":{"success":true}}}]}]
            """,
            sendReasoning: true
        );

        AssertJson(
            """
            [{"role":"assistant","content":[
               {"type":"thinking","thinking":"Think before the initial note.","signature":"test-signature-1"},
               {"type":"tool_use","id":"toolu_initial","name":"saveNote","input":{"note":"phase 1: initial plan"}},
               {"type":"thinking","thinking":"Think before the revised note.","signature":"test-signature-2"},
               {"type":"tool_use","id":"toolu_revised","name":"saveNote","input":{"note":"phase 2: revised plan"}}]},
             {"role":"user","content":[
               {"type":"tool_result","tool_use_id":"toolu_initial","content":"{\"success\":true}"},
               {"type":"tool_result","tool_use_id":"toolu_revised","content":"{\"success\":true}"}]}]
            """,
            result.Messages
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should convert anthropic web_search tool call with error result (error-json string)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_anthropic_web_search_tool_call_with_error_result_error_json_string()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"input":{"query":"test query"},"providerExecuted":true,"toolCallId":"srvtoolu_error1","toolName":"web_search","type":"tool-call"},
              {"output":{"type":"error-json","value":"{\"type\":\"web_search_tool_result_error\",\"errorCode\":\"invalid_tool_input\"}"},"toolCallId":"srvtoolu_error1","toolName":"web_search","type":"tool-result"}]}]
            """,
            sendReasoning: false
        );

        AssertJson(
            """
            [{"content":[
              {"id":"srvtoolu_error1","input":{"query":"test query"},"name":"web_search","type":"server_tool_use"},
              {"content":{"error_code":"invalid_tool_input","type":"web_search_tool_result_error"},"tool_use_id":"srvtoolu_error1","type":"web_search_tool_result"}],
              "role":"assistant"}]
            """,
            result.Messages
        );
        Assert.Empty(result.Betas);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should convert anthropic web_search tool call with error result (error-json object)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_anthropic_web_search_tool_call_with_error_result_error_json_object()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"input":{"query":"test query"},"providerExecuted":true,"toolCallId":"srvtoolu_error2","toolName":"web_search","type":"tool-call"},
              {"output":{"type":"error-json","value":{"type":"web_search_tool_result_error","errorCode":"max_uses_exceeded"}},"toolCallId":"srvtoolu_error2","toolName":"web_search","type":"tool-result"}]}]
            """,
            sendReasoning: false
        );

        AssertJson(
            """
            [{"content":[
              {"id":"srvtoolu_error2","input":{"query":"test query"},"name":"web_search","type":"server_tool_use"},
              {"content":{"error_code":"max_uses_exceeded","type":"web_search_tool_result_error"},"tool_use_id":"srvtoolu_error2","type":"web_search_tool_result"}],
              "role":"assistant"}]
            """,
            result.Messages
        );
        Assert.Empty(result.Betas);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert anthropic web_fetch tool call and result parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_anthropic_web_fetch_tool_call_and_result_parts()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"input":{"url":"https://raw.githubusercontent.com/vercel/ai/blob/main/examples/ai-functions/data/ai.pdf"},"providerExecuted":true,"toolCallId":"srvtoolu_011cNtbtzFARKPcAcp7w4nh9","toolName":"web_fetch","type":"tool-call"},
              {"output":{"type":"json","value":{"type":"web_fetch_result","url":"https://raw.githubusercontent.com/vercel/ai/blob/main/examples/ai-functions/data/ai.pdf","retrievedAt":"2025-01-01T00:00:00.000Z","content":{"type":"document","title":"AI.pdf","citations":{"enabled":true},"source":{"type":"text","mediaType":"text/plain","data":"The PDF says about AI."}}}},"toolCallId":"srvtoolu_011cNtbtzFARKPcAcp7w4nh9","toolName":"web_fetch","type":"tool-result"}]}]
            """,
            sendReasoning: false
        );

        AssertJson(
            """
            [{"content":[
              {"id":"srvtoolu_011cNtbtzFARKPcAcp7w4nh9","input":{"url":"https://raw.githubusercontent.com/vercel/ai/blob/main/examples/ai-functions/data/ai.pdf"},"name":"web_fetch","type":"server_tool_use"},
              {"content":{"content":{"citations":{"enabled":true},"source":{"data":"The PDF says about AI.","media_type":"text/plain","type":"text"},"title":"AI.pdf","type":"document"},"retrieved_at":"2025-01-01T00:00:00.000Z","type":"web_fetch_result","url":"https://raw.githubusercontent.com/vercel/ai/blob/main/examples/ai-functions/data/ai.pdf"},"tool_use_id":"srvtoolu_011cNtbtzFARKPcAcp7w4nh9","type":"web_fetch_tool_result"}],
              "role":"assistant"}]
            """,
            result.Messages
        );
        Assert.Empty(result.Betas);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert anthropic web_fetch tool call with error result",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_anthropic_web_fetch_tool_call_with_error_result()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"input":{"url":"https://httpbin.org/status/500"},"providerExecuted":true,"toolCallId":"srvtoolu_016yTvwN6L1sDdjdPUzPbZRV","toolName":"web_fetch","type":"tool-call"},
              {"output":{"type":"error-json","value":"{\"type\":\"web_fetch_tool_result_error\",\"errorCode\":\"url_not_accessible\"}"},"toolCallId":"srvtoolu_016yTvwN6L1sDdjdPUzPbZRV","toolName":"web_fetch","type":"tool-result"}]}]
            """,
            sendReasoning: false
        );

        AssertJson(
            """
            [{"content":[
              {"id":"srvtoolu_016yTvwN6L1sDdjdPUzPbZRV","input":{"url":"https://httpbin.org/status/500"},"name":"web_fetch","type":"server_tool_use"},
              {"content":{"error_code":"url_not_accessible","type":"web_fetch_tool_result_error"},"tool_use_id":"srvtoolu_016yTvwN6L1sDdjdPUzPbZRV","type":"web_fetch_tool_result"}],
              "role":"assistant"}]
            """,
            result.Messages
        );
        Assert.Empty(result.Betas);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert anthropic web_fetch tool call with error result as object",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_anthropic_web_fetch_tool_call_with_error_result_as_object()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"input":{"url":"https://www.fotball.no/fotballdata/turnering/hjem/?fiksId=193156"},"providerExecuted":true,"toolCallId":"srvtoolu_01JteKo9VRHDKZ1rdMXywnwD","toolName":"web_fetch","type":"tool-call"},
              {"output":{"type":"error-json","value":{"type":"web_fetch_tool_result_error","errorCode":"url_not_allowed"}},"toolCallId":"srvtoolu_01JteKo9VRHDKZ1rdMXywnwD","toolName":"web_fetch","type":"tool-result"}]}]
            """,
            sendReasoning: false
        );

        AssertJson(
            """
            [{"content":[
              {"id":"srvtoolu_01JteKo9VRHDKZ1rdMXywnwD","input":{"url":"https://www.fotball.no/fotballdata/turnering/hjem/?fiksId=193156"},"name":"web_fetch","type":"server_tool_use"},
              {"content":{"error_code":"url_not_allowed","type":"web_fetch_tool_result_error"},"tool_use_id":"srvtoolu_01JteKo9VRHDKZ1rdMXywnwD","type":"web_fetch_tool_result"}],
              "role":"assistant"}]
            """,
            result.Messages
        );
        Assert.Empty(result.Betas);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should convert anthropic web_fetch tool call with error result as malformed string",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_anthropic_web_fetch_tool_call_with_error_result_as_malformed_string()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"input":{"url":"https://example.com"},"providerExecuted":true,"toolCallId":"srvtoolu_test123","toolName":"web_fetch","type":"tool-call"},
              {"output":{"type":"error-json","value":"not valid json at all"},"toolCallId":"srvtoolu_test123","toolName":"web_fetch","type":"tool-result"}]}]
            """,
            sendReasoning: false
        );

        AssertJson(
            """
            {"content":{"error_code":"unavailable","type":"web_fetch_tool_result_error"},"tool_use_id":"srvtoolu_test123","type":"web_fetch_tool_result"}
            """,
            result.Messages[0]!["content"]![1]
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert anthropic tool_search_tool_regex tool call and result parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_anthropic_tool_search_tool_regex_tool_call_and_result_parts()
    {
        var result = Run(
            """
            [{"role":"assistant","content":[
              {"input":{"pattern":"weather|forecast","limit":10},"providerExecuted":true,"toolCallId":"srvtoolu_01SACvPAnp6ucMJsstB5qb3f","toolName":"tool_search_tool_regex","type":"tool-call"},
              {"output":{"type":"json","value":[{"type":"tool_reference","toolName":"get_weather"}]},"toolCallId":"srvtoolu_01SACvPAnp6ucMJsstB5qb3f","toolName":"tool_search_tool_regex","type":"tool-result"}]}]
            """,
            sendReasoning: false
        );

        AssertJson(
            """
            [{"content":[
              {"id":"srvtoolu_01SACvPAnp6ucMJsstB5qb3f","input":{"limit":10,"pattern":"weather|forecast"},"name":"tool_search_tool_regex","type":"server_tool_use"},
              {"content":{"tool_references":[{"tool_name":"get_weather","type":"tool_reference"}],"type":"tool_search_tool_search_result"},"tool_use_id":"srvtoolu_01SACvPAnp6ucMJsstB5qb3f","type":"tool_search_tool_result"}],
              "role":"assistant"}]
            """,
            result.Messages
        );
        Assert.Empty(result.Betas);
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData(
        "tool_search_tool_regex",
        "error-json",
        "\"{\\\"type\\\":\\\"tool_search_tool_result_error\\\",\\\"errorCode\\\":\\\"invalid_tool_input\\\"}\""
    )]
    [InlineData(
        "tool_search_tool_regex",
        "json",
        "{\"type\":\"tool_search_tool_result_error\",\"errorCode\":\"invalid_tool_input\"}"
    )]
    [InlineData(
        "tool_search_tool_bm25",
        "error-json",
        "\"{\\\"type\\\":\\\"tool_search_tool_result_error\\\",\\\"errorCode\\\":\\\"invalid_tool_input\\\"}\""
    )]
    [InlineData(
        "tool_search_tool_bm25",
        "json",
        "{\"type\":\"tool_search_tool_result_error\",\"errorCode\":\"invalid_tool_input\"}"
    )]
    [UpstreamTest(
        Prefix + "should convert $toolName $output.type error results",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_tool_search_error_results(
        string toolName,
        string outputType,
        string outputValue
    )
    {
        var toolCallId = "srvtoolu_" + toolName;
        var input =
            toolName == "tool_search_tool_regex"
                ? "{\"pattern\":\"[\"}"
                : "{\"query\":\"weather\"}";

        var result = Run(
            "[{\"role\":\"assistant\",\"content\":[{\"input\":"
                + input
                + ",\"providerExecuted\":true,\"toolCallId\":\""
                + toolCallId
                + "\",\"toolName\":\""
                + toolName
                + "\",\"type\":\"tool-call\"},{\"output\":{\"type\":\""
                + outputType
                + "\",\"value\":"
                + outputValue
                + "},\"toolCallId\":\""
                + toolCallId
                + "\",\"toolName\":\""
                + toolName
                + "\",\"type\":\"tool-result\"}]}]",
            sendReasoning: false
        );

        AssertJson(
            "{\"content\":{\"error_code\":\"invalid_tool_input\",\"type\":\"tool_search_tool_result_error\"},\"tool_use_id\":\""
                + toolCallId
                + "\",\"type\":\"tool_search_tool_result\"}",
            result.Messages[0]!["content"]![1]
        );
        Assert.Empty(result.Warnings);
    }
}
