// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Mistral;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Mistral.MistralConversationLanguageModel.MistralConversationFixtures;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralConversationLanguageModel;

/// <summary>Port of <c>mistral-conversation-language-model.test.ts</c> &gt; <c>Mistral web search</c>.</summary>
public sealed class MistralWebSearchTests
{
    private const string Prefix =
        "packages/mistral/src/mistral-conversation-language-model.test.ts::Mistral web search::";

    public static TheoryData<string> Methods => new() { "doGenerate", "doStream" };

    public static TheoryData<string[]> FailedStreams =>
        new()
        {
            {
                new[]
                {
                    "{\"type\":\"conversation.response.error\",\"message\":\"Search failed\",\"code\":500}",
                }
            },
            { new[] { "{\"type\":\"message.output.delta\",\"id\":\"message-1\",\"content\":42}" } },
            {
                new[]
                {
                    "{\"type\":\"tool.execution.started\",\"id\":\"search-1\",\"name\":\"web_search\",\"arguments\":\"\"}",
                    "{\"type\":\"conversation.response.done\",\"usage\":" + UsageJson + "}",
                }
            },
            {
                new[]
                {
                    "{\"type\":\"message.output.delta\",\"id\":\"message-1\",\"content\":\"Truncated\"}",
                }
            },
        };

    [Theory]
    [MemberData(nameof(Methods))]
    [UpstreamTest(
        Prefix + "%s forwards provider configuration and call options",
        Coverage = UpstreamCoverage.Partial,
        Note = "The abort signal becomes the cancellation token, which is checked through the token the HTTP handler receives."
    )]
    public async Task Forwards_provider_configuration_and_call_options(string method)
    {
        var capture = new RecordingHandler(
            method == "doGenerate"
                ? GenerateBody("[" + Answer + "]")
                : "data: {\"type\":\"conversation.response.done\",\"usage\":" + UsageJson + "}\n\n"
        );
        var provider = MistralProvider.Create(
            new OpenAICompatibleOptions
            {
                ApiKey = "custom-api-key",
                BaseUrl = "https://custom.example.com/v1/",
            },
            capture
        );
        provider.Options.Headers["x-provider"] = "provider-value";
        var model = provider.Conversation("mistral-small-latest");
        using var abort = new CancellationTokenSource();
        var options = new LanguageModelCallOptions
        {
            Prompt = PromptMessages,
            Tools = Tools,
            Headers = new Dictionary<string, string?> { ["x-call"] = "call-value" },
        };

        IReadOnlyDictionary<string, string> responseHeaders;
        if (method == "doGenerate")
        {
            var result = await model.DoGenerateAsync(options, abort.Token);
            Assert.Contains(result.Content, content =>
                content is GeneratedText { Text: "Spain won." }
            );
            Assert.Equal(FinishReason.Stop, result.FinishReason);
            Assert.Null(result.RawFinishReason);
            AssertJson(
                capture.Body,
                result.Request!.Body is { } body ? JsonNode.Parse(body.GetRawText()) : null
            );
            responseHeaders = result.ResponseHeaders;
        }
        else
        {
            var result = await model.DoStreamAsync(options, abort.Token);
            var parts = await Read(result);
            Assert.IsType<StreamStartStreamPart>(parts[0]);
            Assert.Empty(((StreamStartStreamPart)parts[0]).Warnings);
            Assert.Equal(FinishReason.Stop, ((FinishStreamPart)parts[^1]).FinishReason);
            AssertJson(
                capture.Body,
                result.Request!.Body is { } body ? JsonNode.Parse(body.GetRawText()) : null
            );
            responseHeaders = result.Response!.Headers;
        }

        Assert.Equal("https://custom.example.com/v1/conversations", capture.Uri!.ToString());
        Assert.Equal("Bearer custom-api-key", capture.Headers["Authorization"]);
        Assert.Equal("provider-value", capture.Headers["x-provider"]);
        Assert.Equal("call-value", capture.Headers["x-call"]);
        Assert.Equal("response-value", responseHeaders["x-response"]);

        // The abort signal is the cancellation token. A call that is waiting on the provider stops when it is cancelled.
        var hanging = new HangingHandler();
        var pending = MistralProvider
            .Create(new OpenAICompatibleOptions { ApiKey = "custom-api-key" }, hanging)
            .Conversation("mistral-small-latest");
        using var cancel = new CancellationTokenSource();
        Task call =
            method == "doGenerate"
                ? pending.DoGenerateAsync(options, cancel.Token)
                : pending.DoStreamAsync(options, cancel.Token);
        await hanging.Started.Task;
        cancel.Cancel();
        var error = await Record.ExceptionAsync(() => call);
        Assert.IsAssignableFrom<OperationCanceledException>(error);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "uses Conversations even when no search tools are supplied",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Uses_Conversations_even_when_no_search_tools_are_supplied()
    {
        var capture = Capture("[" + Answer + "]");
        var model = Model(capture);

        await model.DoGenerateAsync(
            new LanguageModelCallOptions { Prompt = PromptMessages },
            CancellationToken.None
        );

        Assert.Equal("mistral.conversation", model.Provider);
        AssertJson(
            "{\"model\":\"mistral-small-latest\",\"store\":false,\"inputs\":[{\"type\":\"message.input\",\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Who won?\"}]}],\"completion_args\":{}}",
            Body(capture)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "exposes standard and premium provider-executed tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Exposes_standard_and_premium_provider_executed_tools()
    {
        var search = MistralTools.WebSearch();
        Assert.True(search.IsProviderDefined);
        Assert.Equal("mistral.web_search", search.ProviderToolId);
        Assert.Equal("{}", search.Args.GetRawText());

        var premium = MistralTools.WebSearchPremium();
        Assert.True(premium.IsProviderDefined);
        Assert.Equal("mistral.web_search_premium", premium.ProviderToolId);
        Assert.Equal("{}", premium.Args.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "routes search through Conversations and maps the tool name, result, sources, and usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Routes_search_through_Conversations_and_maps_the_tool_name_result_sources_and_usage()
    {
        var capture = Capture();

        var result = await Model(capture)
            .DoGenerateAsync(
                new LanguageModelCallOptions { Prompt = PromptMessages, Tools = Tools },
                CancellationToken.None
            );

        Assert.Equal(4, result.Content.Count);
        var call = Assert.IsAssignableFrom<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("search-1", call.ToolCallId);
        Assert.Equal("search", call.ToolName);
        Assert.Equal("{\"arguments\":\"latest results\"}", call.ArgumentsJson);
        Assert.True(call.ProviderExecuted);
        AssertJson(
            "{\"mistral\":{\"type\":\"tool.execution\",\"name\":\"web_search\"}}",
            JsonNode.Parse(call.ProviderMetadata!.Value.GetRawText())
        );
        var toolResult = Assert.IsType<GeneratedToolResult>(result.Content[1]);
        Assert.Equal("search-1", toolResult.ToolCallId);
        Assert.Equal("search", toolResult.ToolName);
        AssertJson("{\"info\":{\"status\":\"success\"}}", JsonNode.Parse(toolResult.ResultJson));
        Assert.Equal("Spain won.", Assert.IsType<GeneratedText>(result.Content[2]).Text);
        var source = Assert.IsType<GeneratedSource>(result.Content[3]);
        Assert.False(string.IsNullOrEmpty(source.Id));
        Assert.Equal("https://example.com/results", source.Url);
        Assert.Equal("Results", source.Title);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal(10, result.Usage.InputTokens);
        Assert.Equal(20, result.Usage.OutputTokens);
        AssertJson(UsageJson, JsonNode.Parse(result.Usage.Raw!.Value.GetRawText()));
        Assert.Equal("conversation-1", result.ResponseId);
        Assert.Equal("mistral-small-latest", result.ResponseModelId);
        Assert.Empty(result.Warnings);
        AssertJson(
            "{\"model\":\"mistral-small-latest\",\"store\":false,\"inputs\":[{\"type\":\"message.input\",\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Who won?\"}]}],\"tools\":[{\"type\":\"web_search\"}],\"completion_args\":{}}",
            Body(capture)
        );
        Assert.Equal("Bearer test-api-key", capture.Requests[0].Headers["Authorization"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "sends instructions and completion settings in their Conversations API locations",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Sends_instructions_and_completion_settings_in_their_Conversations_API_locations()
    {
        var capture = Capture();

        await Model(capture)
            .DoGenerateAsync(
                new LanguageModelCallOptions
                {
                    Prompt = new ModelMessage[]
                    {
                        new SystemModelMessage("Be accurate."),
                        new SystemModelMessage("Be concise."),
                        PromptMessages[0],
                    },
                    Tools = Tools,
                    MaxOutputTokens = 100,
                    Temperature = 0.3,
                    TopP = 0.9,
                    FrequencyPenalty = 0.1,
                    PresencePenalty = 0.2,
                    StopSequences = new[] { "END" },
                    Seed = 1,
                    JsonResponse = true,
                    JsonSchema = Node("{\"type\":\"object\"}"),
                    ToolChoice = ToolChoice.Required,
                    Reasoning = "high",
                },
                CancellationToken.None
            );

        AssertMatches(
            "{\"instructions\":\"Be accurate.\\n\\nBe concise.\",\"completion_args\":{\"max_tokens\":100,\"temperature\":0.3,\"top_p\":0.9,\"frequency_penalty\":0.1,\"presence_penalty\":0.2,\"stop\":[\"END\"],\"random_seed\":1,\"tool_choice\":\"any\",\"reasoning_effort\":\"high\",\"response_format\":{\"type\":\"json_schema\",\"json_schema\":{\"schema\":{\"type\":\"object\"}}}}}",
            Body(capture)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "supports premium search and forcing a renamed search tool",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Supports_premium_search_and_forcing_a_renamed_search_tool()
    {
        var capture = Capture();

        await Model(capture)
            .DoGenerateAsync(
                new LanguageModelCallOptions
                {
                    Prompt = PromptMessages,
                    Tools = new[] { Search(), Search("mistral.web_search_premium", "news") },
                    ToolChoice = ToolChoice.Tool("news"),
                },
                CancellationToken.None
            );

        AssertMatches(
            "{\"tools\":[{\"type\":\"web_search_premium\"}],\"completion_args\":{\"tool_choice\":\"any\"}}",
            Body(capture)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "replays search executions and assistant answers on a follow-up turn",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Replays_search_executions_and_assistant_answers_on_a_follow_up_turn()
    {
        var capture = Capture();
        var model = Model(capture);

        var first = await model.DoGenerateAsync(
            new LanguageModelCallOptions { Prompt = PromptMessages, Tools = Tools },
            CancellationToken.None
        );
        await model.DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = Followup((GeneratedToolCall)first.Content[0]),
                Tools = Tools,
            },
            CancellationToken.None
        );

        AssertJson(
            "[{\"type\":\"message.input\",\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Who won?\"}]},{\"type\":\"function.call\",\"tool_call_id\":\"search-1\",\"name\":\"web_search\",\"arguments\":\"latest results\"},{\"type\":\"function.result\",\"tool_call_id\":\"search-1\",\"result\":\"{\\\"status\\\":\\\"success\\\"}\"},{\"type\":\"message.input\",\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Spain won.\"}]},{\"type\":\"message.input\",\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Who did they beat?\"}]}]",
            Body(capture, 1)["inputs"]
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves the executed function and raw search result for replay",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_the_executed_function_and_raw_search_result_for_replay()
    {
        var capture = Capture(
            "["
                + Execution
                    .Replace(
                        "\"name\":\"web_search\"",
                        "\"name\":\"web_search\",\"function\":\"web_fetch\""
                    )
                    .Replace(
                        "\"arguments\":\"latest results\"",
                        "\"arguments\":\"{\\\"url\\\":\\\"https://example.com\\\"}\""
                    )
                    .Replace(
                        "\"info\":{\"status\":\"success\"}",
                        "\"info\":{\"result\":\"[{\\\"text\\\":\\\"Retrieved page\\\"}]\"}"
                    )
                + ","
                + Answer
                + "]"
        );
        var model = Model(capture);

        var result = await model.DoGenerateAsync(
            new LanguageModelCallOptions { Prompt = PromptMessages, Tools = Tools },
            CancellationToken.None
        );
        var history = Followup(
            (GeneratedToolCall)result.Content[0],
            "{\"result\":\"[{\\\"text\\\":\\\"Retrieved page\\\"}]\"}"
        );
        await model.DoGenerateAsync(
            new LanguageModelCallOptions { Prompt = history, Tools = Tools },
            CancellationToken.None
        );

        var inputs = Body(capture, 1)["inputs"]!.AsArray();
        AssertJson(
            "[{\"type\":\"function.call\",\"tool_call_id\":\"search-1\",\"name\":\"web_fetch\",\"arguments\":\"{\\\"url\\\":\\\"https://example.com\\\"}\"},{\"type\":\"function.result\",\"tool_call_id\":\"search-1\",\"result\":\"[{\\\"text\\\":\\\"Retrieved page\\\"}]\"}]",
            new JsonArray(inputs[1]!.DeepClone(), inputs[2]!.DeepClone())
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "can replay search history after the tool is removed",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Can_replay_search_history_after_the_tool_is_removed()
    {
        var capture = Capture();
        var model = Model(capture);

        var first = await model.DoGenerateAsync(
            new LanguageModelCallOptions { Prompt = PromptMessages, Tools = Tools },
            CancellationToken.None
        );
        await model.DoGenerateAsync(
            new LanguageModelCallOptions { Prompt = Followup((GeneratedToolCall)first.Content[0]) },
            CancellationToken.None
        );

        AssertMatches(
            "{\"type\":\"function.call\",\"name\":\"web_search\"}",
            Body(capture, 1)["inputs"]![1]
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not retain history on a shared model instance",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Does_not_retain_history_on_a_shared_model_instance()
    {
        var capture = Capture();
        var model = Model(capture);

        await model.DoGenerateAsync(
            new LanguageModelCallOptions { Prompt = PromptMessages, Tools = Tools },
            CancellationToken.None
        );
        await model.DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[] { new UserModelMessage("Independent search") },
                Tools = Tools,
            },
            CancellationToken.None
        );

        AssertJson(
            "[{\"type\":\"message.input\",\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Independent search\"}]}]",
            Body(capture, 1)["inputs"]
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "supports function calls alongside search and replays their results",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Supports_function_calls_alongside_search_and_replays_their_results()
    {
        var capture = Capture(
            "["
                + Execution
                + ",{\"type\":\"function.call\",\"tool_call_id\":\"function-1\",\"name\":\"weather\",\"arguments\":{\"city\":\"Madrid\"}}]"
        );
        var model = Model(capture);
        var mixedTools = new[]
        {
            Search(),
            new LanguageModelTool("weather", null, Node("{\"type\":\"object\"}")),
        };

        var first = await model.DoGenerateAsync(
            new LanguageModelCallOptions { Prompt = PromptMessages, Tools = mixedTools },
            CancellationToken.None
        );

        Assert.Equal(FinishReason.ToolCalls, first.FinishReason);
        var function = Assert.IsAssignableFrom<GeneratedToolCall>(first.Content[2]);
        Assert.Equal("function-1", function.ToolCallId);
        Assert.Equal("weather", function.ToolName);
        Assert.Equal("{\"city\":\"Madrid\"}", function.ArgumentsJson);
        Assert.False(function.ProviderExecuted);
        Assert.Null(function.ProviderMetadata);

        var prompt = Followup((GeneratedToolCall)first.Content[0]).ToList();
        prompt.Add(
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantToolCallPart(
                        new GeneratedToolCall("function-1", "weather", "{\"city\":\"Madrid\"}")
                    ),
                }
            )
        );
        prompt.Add(
            new ToolModelMessage("function-1", "weather", "{\"temperature\":20}", false, "json")
        );
        await model.DoGenerateAsync(
            new LanguageModelCallOptions { Prompt = prompt, Tools = mixedTools },
            CancellationToken.None
        );

        var inputs = Body(capture, 1)["inputs"]!.AsArray();
        AssertJson(
            "[{\"type\":\"function.call\",\"tool_call_id\":\"function-1\",\"name\":\"weather\",\"arguments\":\"{\\\"city\\\":\\\"Madrid\\\"}\"},{\"type\":\"function.result\",\"tool_call_id\":\"function-1\",\"result\":\"{\\\"temperature\\\":20}\"}]",
            new JsonArray(inputs[^2]!.DeepClone(), inputs[^1]!.DeepClone())
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "handles string output, reasoning, duplicate references, and references without URLs",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Handles_string_output_reasoning_duplicate_references_and_references_without_URLs()
    {
        var capture = Capture(
            "[{\"type\":\"message.output\",\"content\":\"Hello\"},{\"type\":\"message.output\",\"content\":[{\"type\":\"thinking\",\"thinking\":[{\"type\":\"text\",\"text\":\"Think\"}]},"
                + Reference
                + ","
                + Reference
                + ","
                + Reference.Replace("\"url\":\"https://example.com/results\"", "\"url\":null")
                + ",{\"type\":\"text\",\"text\":\"\"}]}]"
        );
        capture.ResponseBody = capture.ResponseBody.Replace(
            ",\"usage\":" + UsageJson,
            ",\"usage\":{}"
        );

        var result = await Model(capture)
            .DoGenerateAsync(
                new LanguageModelCallOptions { Prompt = PromptMessages, Tools = Tools },
                CancellationToken.None
            );

        Assert.Equal(3, result.Content.Count);
        Assert.Equal("Hello", Assert.IsType<GeneratedText>(result.Content[0]).Text);
        Assert.Equal("Think", Assert.IsAssignableFrom<GeneratedReasoning>(result.Content[1]).Text);
        var source = Assert.IsType<GeneratedSource>(result.Content[2]);
        Assert.False(string.IsNullOrEmpty(source.Id));
        Assert.Equal("https://example.com/results", source.Url);
        Assert.Equal("Results", source.Title);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "warns about unsupported Conversations options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Warns_about_unsupported_Conversations_options()
    {
        var capture = Capture();

        var result = await Model(capture)
            .DoGenerateAsync(
                new LanguageModelCallOptions
                {
                    Prompt = PromptMessages,
                    Tools = Tools,
                    ProviderOptions = MistralOptions(
                        "{\"safePrompt\":true,\"parallelToolCalls\":false,\"promptCacheKey\":\"cache\",\"documentImageLimit\":1,\"documentPageLimit\":2}"
                    ),
                },
                CancellationToken.None
            );

        Assert.Equal(
            new[]
            {
                "safePrompt",
                "documentImageLimit",
                "documentPageLimit",
                "promptCacheKey",
                "parallelToolCalls",
            },
            result
                .Warnings.Where(warning => warning.Type == "unsupported")
                .Select(warning => warning.Feature)
                .ToArray()
        );
        var body = Body(capture);
        Assert.Null(body["safe_prompt"]);
        Assert.Null(body["completion_args"]!["parallel_tool_calls"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "streams search execution, reasoning, text, references and usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Streams_search_execution_reasoning_text_references_and_usage()
    {
        var capture = StreamCapture(
            "{\"type\":\"conversation.response.started\",\"conversation_id\":\"conversation-1\",\"created_at\":\"2026-01-01T00:00:00Z\"}",
            "{\"type\":\"tool.execution.started\",\"id\":\"search-1\",\"name\":\"web_search\",\"arguments\":\"\"}",
            "{\"type\":\"tool.execution.delta\",\"id\":\"search-1\",\"name\":\"web_search\",\"arguments\":\"latest \"}",
            "{\"type\":\"tool.execution.delta\",\"id\":\"search-1\",\"name\":\"web_search\",\"arguments\":\"results\"}",
            "{\"type\":\"tool.execution.done\",\"id\":\"search-1\",\"name\":\"web_search\",\"info\":{\"status\":\"success\"}}",
            "{\"type\":\"message.output.delta\",\"id\":\"message-1\",\"content\":{\"type\":\"thinking\",\"thinking\":[{\"type\":\"text\",\"text\":\"Think\"}]}}",
            "{\"type\":\"message.output.delta\",\"id\":\"message-1\",\"content_index\":1,\"content\":\"Spain \"}",
            "{\"type\":\"message.output.delta\",\"id\":\"message-1\",\"content_index\":1,\"content\":{\"type\":\"text\",\"text\":\"won.\"}}",
            "{\"type\":\"message.output.delta\",\"id\":\"message-1\",\"content_index\":2,\"content\":"
                + Reference
                + "}",
            "{\"type\":\"conversation.response.done\",\"usage\":" + UsageJson + "}"
        );
        var model = Model(capture);

        var result = await model.DoStreamAsync(
            new LanguageModelCallOptions { Prompt = PromptMessages, Tools = Tools },
            CancellationToken.None
        );
        var parts = await Read(result);

        Assert.Equal(
            new[]
            {
                "stream-start",
                "response-metadata",
                "tool-input-start",
                "tool-input-delta",
                "tool-input-end",
                "tool-call",
                "tool-result",
                "reasoning-start",
                "reasoning-delta",
                "reasoning-end",
                "text-start",
                "text-delta",
                "text-delta",
                "source",
                "text-end",
                "finish",
            },
            parts.Select(part => part.Type).ToArray()
        );
        var call = parts.OfType<ToolCallStreamPart>().Single();
        Assert.Equal("search", call.ToolName);
        Assert.True(call.ProviderExecuted);
        Assert.Equal("{\"arguments\":\"latest results\"}", call.ArgumentsJson);
        var finish = (FinishStreamPart)parts[^1];
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal(10, finish.Usage.InputTokens);
        Assert.Equal(20, finish.Usage.OutputTokens);
        AssertMatches("{\"stream\":true,\"tools\":[{\"type\":\"web_search\"}]}", Body(capture));

        capture.ResponseBody = GenerateBody("[" + Answer + "]");
        await model.DoGenerateAsync(
            new LanguageModelCallOptions { Prompt = Followup(ToolCall(call)), Tools = Tools },
            CancellationToken.None
        );
        AssertMatches(
            "{\"type\":\"function.call\",\"arguments\":\"latest results\"}",
            Body(capture, 1)["inputs"]![1]
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "streams incremental function calls alongside search",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Streams_incremental_function_calls_alongside_search()
    {
        var capture = StreamCapture(
            "{\"type\":\"function.call.delta\",\"tool_call_id\":\"function-1\",\"name\":\"weather\",\"arguments\":\"{\\\"city\\\":\",\"output_index\":0}",
            "{\"type\":\"function.call.delta\",\"tool_call_id\":\"function-1\",\"name\":\"weather\",\"arguments\":\"\\\"Madrid\\\"}\",\"output_index\":0}",
            "{\"type\":\"conversation.response.done\",\"usage\":" + UsageJson + "}"
        );

        var result = await Model(capture)
            .DoStreamAsync(
                new LanguageModelCallOptions { Prompt = PromptMessages, Tools = Tools },
                CancellationToken.None
            );
        var parts = await Read(result);

        var call = parts.OfType<ToolCallStreamPart>().Single();
        Assert.Equal("function-1", call.ToolCallId);
        Assert.Equal("weather", call.ToolName);
        Assert.Equal("{\"city\":\"Madrid\"}", call.ArgumentsJson);
        Assert.Equal(FinishReason.ToolCalls, ((FinishStreamPart)parts[^1]).FinishReason);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "streams premium search without execution info and reports the model ID",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Streams_premium_search_without_execution_info_and_reports_the_model_ID()
    {
        var capture = StreamCapture(
            "{\"type\":\"tool.execution.started\",\"id\":\"search-1\",\"name\":\"web_search_premium\",\"arguments\":\"query\",\"function\":\"web_search\"}",
            "{\"type\":\"tool.execution.done\",\"id\":\"search-1\",\"name\":\"web_search_premium\"}",
            "{\"type\":\"message.output.delta\",\"id\":\"message-1\",\"model\":\"mistral-small-latest\",\"content\":\"Answer\"}",
            "{\"type\":\"conversation.response.done\",\"usage\":" + UsageJson + "}"
        );

        var result = await Model(capture)
            .DoStreamAsync(
                new LanguageModelCallOptions
                {
                    Prompt = PromptMessages,
                    Tools = new[] { Search("mistral.web_search_premium", "news") },
                },
                CancellationToken.None
            );
        var parts = await Read(result);

        AssertMatches(
            "{\"mistral\":{\"function\":\"web_search\"}}",
            JsonNode.Parse(
                parts.OfType<ToolCallStreamPart>().Single().ProviderMetadata!.Value.GetRawText()
            )
        );
        var toolResult = parts.OfType<ToolResultStreamPart>().Single();
        Assert.Equal("search-1", toolResult.ToolCallId);
        Assert.Equal("news", toolResult.ToolName);
        Assert.Equal("{}", toolResult.Result.GetRawText());
        Assert.Contains(parts.OfType<ResponseMetadataStreamPart>(), part =>
            part.ModelId == "mistral-small-latest" && part.Id == null
        );
        Assert.Equal(FinishReason.Stop, ((FinishStreamPart)parts[^1]).FinishReason);
    }

    [Theory]
    [MemberData(nameof(FailedStreams))]
    [UpstreamTest(
        Prefix + "reports failed or incomplete streams",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Reports_failed_or_incomplete_streams(string[] events)
    {
        var capture = StreamCapture(events);

        var result = await Model(capture)
            .DoStreamAsync(
                new LanguageModelCallOptions { Prompt = PromptMessages, Tools = Tools },
                CancellationToken.None
            );
        var parts = await Read(result);

        Assert.Equal(FinishReason.Error, ((FinishStreamPart)parts[^1]).FinishReason);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "includes raw events when requested",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Includes_raw_events_when_requested()
    {
        var evt = "{\"type\":\"conversation.response.done\",\"usage\":" + UsageJson + "}";
        var capture = StreamCapture(evt);

        var result = await Model(capture)
            .DoStreamAsync(
                new LanguageModelCallOptions
                {
                    Prompt = PromptMessages,
                    Tools = Tools,
                    IncludeRawChunks = true,
                },
                CancellationToken.None
            );
        var parts = await Read(result);

        Assert.Contains(parts.OfType<RawStreamPart>(), part => part.RawJson == evt);
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly string _body;

        public RecordingHandler(string body)
        {
            _body = body;
        }

        public Uri? Uri { get; private set; }

        public string Body { get; private set; } = string.Empty;

        public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Uri = request.RequestUri;
            Body =
                request.Content == null
                    ? string.Empty
                    : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            foreach (var header in request.Headers)
            {
                Headers[header.Key] = string.Join(",", header.Value);
            }

            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    _body,
                    System.Text.Encoding.UTF8,
                    _body.StartsWith("data:", StringComparison.Ordinal)
                        ? "text/event-stream"
                        : "application/json"
                ),
            };
            response.Headers.TryAddWithoutValidation("x-response", "response-value");
            return response;
        }
    }

    private sealed class HangingHandler : HttpMessageHandler
    {
        public TaskCompletionSource<bool> Started { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Started.TrySetResult(true);
            await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException("Unreachable.");
        }
    }
}
