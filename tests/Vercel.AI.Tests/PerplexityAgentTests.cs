// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Perplexity;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class PerplexityAgentTests
{
    [Fact]
    public async Task Generate_posts_an_agent_preset_request()
    {
        var handler = new AgentHandler(SampleResponse());
        var provider = Create(handler);

        var result = await provider.LanguageModel("low").DoGenerateAsync(Prompt("Hello"), CancellationToken.None);

        Assert.Equal("https://api.perplexity.ai/v1/agent", handler.Uri);
        Assert.Equal("Bearer secret", handler.Headers["Authorization"]);
        Assert.Equal("low", Body(handler).GetProperty("preset").GetString());
        Assert.False(Body(handler).TryGetProperty("model", out _));
        Assert.False(Body(handler).TryGetProperty("stream", out _));
        Assert.Equal("Hello", Body(handler).GetProperty("input")[0].GetProperty("content").GetString());
        Assert.Equal("Hello from Perplexity.", result.Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
    }

    [Fact]
    public async Task Custom_base_url_routes_the_agent_path()
    {
        var handler = new AgentHandler(SampleResponse());
        var provider = PerplexityProvider.Create(new OpenAICompatibleOptions
        {
            ApiKey = "secret",
            BaseUrl = "https://proxy.example/perplexity",
        }, handler);

        await provider.LanguageModel("fast").DoGenerateAsync(Prompt("Hello"), CancellationToken.None);

        Assert.Equal("https://proxy.example/perplexity/v1/agent", handler.Uri);
    }

    [Fact]
    public async Task Direct_and_legacy_sonar_ids_are_sent_as_models()
    {
        var handler = new AgentHandler(SampleResponse());
        var provider = Create(handler);

        await provider.LanguageModel("openai/gpt-5.1").DoGenerateAsync(Prompt("Hello"), CancellationToken.None);
        Assert.Equal("openai/gpt-5.1", Body(handler).GetProperty("model").GetString());

        await provider.LanguageModel("sonar-pro").DoGenerateAsync(Prompt("Hello"), CancellationToken.None);
        Assert.Equal("sonar-pro", Body(handler).GetProperty("model").GetString());
        Assert.False(Body(handler).TryGetProperty("preset", out _));
    }

    [Fact]
    public async Task Provider_options_tools_schema_and_reasoning_are_mapped()
    {
        var handler = new AgentHandler(SampleResponse());
        var provider = Create(handler);
        var options = Prompt("Hello");
        options.MaxOutputTokens = 200;
        options.Temperature = 0.4;
        options.TopP = 0.9;
        options.Reasoning = "high";
        options.JsonSchemaName = "answer";
        options.JsonSchema = Json("{\"type\":\"object\",\"properties\":{\"answer\":{\"type\":\"string\"}},\"required\":[\"answer\"],\"additionalProperties\":false}");
        options.Tools = new[]
        {
            new LanguageModelTool("weather", "Get the weather", Json("{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"]}")),
        };
        options.ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["perplexity"] = Json("{\"max_steps\":4,\"previous_response_id\":\"resp-previous\",\"store\":false,\"tools\":[{\"type\":\"web_search\",\"search_context_size\":\"low\"}],\"future_option\":{\"enabled\":true}}"),
        };

        await provider.LanguageModel("low").DoGenerateAsync(options, CancellationToken.None);

        var body = Body(handler);
        Assert.Equal("low", body.GetProperty("preset").GetString());
        Assert.Equal(200, body.GetProperty("max_output_tokens").GetInt32());
        Assert.Equal(0.4, body.GetProperty("temperature").GetDouble());
        Assert.Equal(0.9, body.GetProperty("top_p").GetDouble());
        Assert.Equal("high", body.GetProperty("reasoning").GetProperty("effort").GetString());
        Assert.Equal(4, body.GetProperty("max_steps").GetInt32());
        Assert.Equal("resp-previous", body.GetProperty("previous_response_id").GetString());
        Assert.False(body.GetProperty("store").GetBoolean());
        Assert.True(body.GetProperty("future_option").GetProperty("enabled").GetBoolean());
        Assert.Equal("json_schema", body.GetProperty("response_format").GetProperty("type").GetString());
        Assert.Equal("answer", body.GetProperty("response_format").GetProperty("json_schema").GetProperty("name").GetString());
        Assert.True(body.GetProperty("response_format").GetProperty("json_schema").GetProperty("strict").GetBoolean());
        Assert.Equal("web_search", body.GetProperty("tools")[0].GetProperty("type").GetString());
        Assert.Equal("function", body.GetProperty("tools")[1].GetProperty("type").GetString());
        Assert.Equal("weather", body.GetProperty("tools")[1].GetProperty("name").GetString());
        Assert.Equal("Get the weather", body.GetProperty("tools")[1].GetProperty("description").GetString());
    }

    [Fact]
    public async Task Invalid_provider_options_are_rejected_before_the_request()
    {
        var handler = new AgentHandler(SampleResponse());
        var provider = Create(handler);
        var options = Prompt("Hello");
        options.ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["perplexity"] = Json("{\"max_steps\":0}"),
        };

        var exception = await Assert.ThrowsAsync<AiSdkException>(() => provider.LanguageModel("low").DoGenerateAsync(options, CancellationToken.None));

        Assert.Contains("max_steps", exception.Message);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task Embeddings_stay_on_the_existing_client()
    {
        var handler = new AgentHandler(SampleResponse());
        var provider = Create(handler);

        var result = await provider.EmbeddingModel("pplx-embed-v1-4b").DoEmbedAsync(new[] { "hello" }, CancellationToken.None);

        Assert.Contains("/embeddings", handler.Uri);
        Assert.DoesNotContain("/v1/agent", handler.Uri);
        Assert.Equal("Bearer secret", handler.Headers["Authorization"]);
        Assert.NotEmpty(result.Embeddings);
    }

    [Fact]
    public async Task Image_input_and_tool_continuations_are_mapped()
    {
        var handler = new AgentHandler(SampleResponse());
        var provider = Create(handler);
        var call = new GeneratedToolCall("call-1", "weather", "{\"city\":\"San Francisco\"}", Json("{\"perplexity\":{\"thoughtSignature\":\"signature-1\"}}"));
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[]
                {
                    new TextContentPart("Describe these images"),
                    new FileContentPart("image/png", "https://example.com/image.png", null, null),
                    new FileContentPart("image/png", null, new byte[] { 0, 1, 2, 3 }, null),
                }),
                new AssistantModelMessage(null, new[] { call }, null),
                new ToolModelMessage("call-1", "weather", "{\"temperature\":18}", false),
            },
        };

        await provider.LanguageModel("low").DoGenerateAsync(options, CancellationToken.None);

        var input = Body(handler).GetProperty("input");
        Assert.Equal("input_image", input[0].GetProperty("content")[1].GetProperty("type").GetString());
        Assert.Equal("https://example.com/image.png", input[0].GetProperty("content")[1].GetProperty("image_url").GetString());
        Assert.Equal("data:image/png;base64,AAECAw==", input[0].GetProperty("content")[2].GetProperty("image_url").GetString());
        Assert.Equal("function_call", input[1].GetProperty("type").GetString());
        Assert.Equal("signature-1", input[1].GetProperty("thought_signature").GetString());
        Assert.Equal("function_call_output", input[2].GetProperty("type").GetString());
        Assert.Equal("{\"temperature\":18}", input[2].GetProperty("output").GetString());
        Assert.False(input[2].TryGetProperty("thought_signature", out _));
    }

    [Fact]
    public async Task Pdf_and_video_input_are_rejected()
    {
        var handler = new AgentHandler(SampleResponse());
        var provider = Create(handler);

        var pdf = await Assert.ThrowsAsync<AiSdkException>(() => provider.LanguageModel("low").DoGenerateAsync(FilePrompt("application/pdf"), CancellationToken.None));
        var video = await Assert.ThrowsAsync<AiSdkException>(() => provider.LanguageModel("low").DoGenerateAsync(FilePrompt("video/mp4"), CancellationToken.None));

        Assert.Contains("PDF", pdf.Message);
        Assert.Contains("video", video.Message);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task Fixture_exposes_text_sources_usage_cost_and_tool_counts()
    {
        var json = File.ReadAllText(Fixture("perplexity-agent-web-search.json"));
        var handler = new AgentHandler(json);
        var provider = Create(handler);

        var result = await provider.LanguageModel("fast").DoGenerateAsync(Prompt("Find TypeScript"), CancellationToken.None);

        Assert.Contains("TypeScript", result.Text);
        Assert.Contains(result.Content, part => part is GeneratedSource source && source.Url == "https://www.typescriptlang.org/");
        var cited = Assert.IsType<GeneratedSource>(result.Content.First(part => part is GeneratedSource source && source.Url == "https://www.typescriptlang.org/"));
        Assert.Equal("7", cited.Id);
        Assert.Equal(7, cited.ProviderMetadata!.Value.GetProperty("perplexity").GetProperty("resultId").GetInt32());
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("completed", result.RawFinishReason);
        Assert.Equal(3007, result.Usage.InputTokens);
        Assert.Equal(36, result.Usage.OutputTokens);
        Assert.Equal(3043, result.Usage.TotalTokens);
        Assert.Equal(1780, result.Usage.CacheReadTokens);
        Assert.Equal(1224, result.Usage.CacheWriteTokens);
        Assert.Equal(0, result.Usage.ReasoningTokens);
        Assert.Equal(3, result.Usage.NoCacheInputTokens);
        Assert.Equal(36, result.Usage.TextTokens);
        var metadata = result.ProviderMetadata!.Value.GetProperty("perplexity");
        Assert.Equal(JsonValueKind.Null, metadata.GetProperty("images").ValueKind);
        Assert.Equal(JsonValueKind.Null, metadata.GetProperty("usage").GetProperty("citationTokens").ValueKind);
        Assert.Equal(1, metadata.GetProperty("usage").GetProperty("numSearchQueries").GetInt32());
        Assert.Equal("USD", metadata.GetProperty("cost").GetProperty("currency").GetString());
        Assert.Equal(0.00119, metadata.GetProperty("cost").GetProperty("totalCost").GetDouble());
        Assert.Equal(1, metadata.GetProperty("toolCalls").GetProperty("search_web").GetProperty("invocation").GetInt32());
        Assert.Contains("search_results", result.RawResponse);
    }

    [Fact]
    public async Task Finance_traces_stay_on_the_raw_response()
    {
        var json = SampleResponse("{\"status\":\"completed\",\"output\":[{\"type\":\"finance_results\",\"results\":[{\"category\":\"quote\",\"content\":\"AAPL\"}]},{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"Hello from Perplexity.\"}]}]}");
        var handler = new AgentHandler(json);
        var result = await Create(handler).LanguageModel("low").DoGenerateAsync(Prompt("Hello"), CancellationToken.None);

        Assert.Equal("Hello from Perplexity.", result.Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Contains("finance_results", result.RawResponse);
    }

    [Fact]
    public async Task Malformed_search_results_and_bodies_fail()
    {
        var missingUrl = SampleResponse("{\"output\":[{\"type\":\"search_results\",\"results\":[{\"title\":\"Missing URL\"}]}]}");
        var missingOutput = "{\"id\":\"resp-123\",\"created_at\":1,\"model\":\"m\",\"object\":\"response\",\"status\":\"completed\"}";
        var provider = Create(new AgentHandler(missingUrl));

        var invalidItem = await Assert.ThrowsAsync<ApiException>(() => provider.LanguageModel("low").DoGenerateAsync(Prompt("Hello"), CancellationToken.None));
        var invalidBody = await Assert.ThrowsAsync<ApiException>(() => Create(new AgentHandler(missingOutput)).LanguageModel("low").DoGenerateAsync(Prompt("Hello"), CancellationToken.None));

        Assert.Equal(200, invalidItem.StatusCode);
        Assert.Equal("Invalid JSON response", invalidItem.Message);
        Assert.Equal(200, invalidBody.StatusCode);
    }

    [Fact]
    public async Task Usage_cost_function_calls_and_incomplete_reasons_are_exposed()
    {
        var response = SampleResponse("{\"status\":\"requires_action\",\"output\":[{\"id\":\"fc-123\",\"type\":\"function_call\",\"call_id\":\"call-123\",\"name\":\"weather\",\"arguments\":\"{\\\"city\\\":\\\"San Francisco\\\"}\",\"thought_signature\":\"signature-123\"}]}");
        var result = await Create(new AgentHandler(response)).LanguageModel("low").DoGenerateAsync(Prompt("Hello"), CancellationToken.None);
        var call = Assert.IsType<GeneratedToolCall>(result.Content[0]);

        Assert.Equal("call-123", call.ToolCallId);
        Assert.Equal("weather", call.ToolName);
        Assert.Equal("signature-123", call.ProviderMetadata!.Value.GetProperty("perplexity").GetProperty("thoughtSignature").GetString());
        Assert.Equal(FinishReason.ToolCalls, result.FinishReason);
        Assert.Equal("requires_action", result.RawFinishReason);
        Assert.Equal(120, result.Usage.InputTokens);
        Assert.Equal(20, result.Usage.CacheReadTokens);
        Assert.Equal(10, result.Usage.CacheWriteTokens);
        Assert.Equal(90, result.Usage.NoCacheInputTokens);
        Assert.Equal(5, result.Usage.ReasoningTokens);
        Assert.Equal(40, result.Usage.TextTokens);
        Assert.Equal(2, result.ProviderMetadata!.Value.GetProperty("perplexity").GetProperty("usage").GetProperty("numSearchQueries").GetInt32());
        Assert.Equal(0.003, result.ProviderMetadata.Value.GetProperty("perplexity").GetProperty("cost").GetProperty("toolCallsCost").GetDouble());

        var omitted = SampleResponse("{\"status\":\"requires_action\",\"output\":[{\"id\":\"fc-123\",\"type\":\"function_call\",\"call_id\":\"call-123\",\"name\":\"weather\",\"arguments\":\"{}\"}]}");
        var omittedResult = await Create(new AgentHandler(omitted)).LanguageModel("low").DoGenerateAsync(Prompt("Hello"), CancellationToken.None);
        Assert.False(Assert.IsType<GeneratedToolCall>(omittedResult.Content[0]).ProviderMetadata!.Value.GetProperty("perplexity").TryGetProperty("thoughtSignature", out _));

        var length = await Create(new AgentHandler(SampleResponse("{\"status\":\"incomplete\",\"incomplete_details\":{\"reason\":\"max_output_tokens\"}}"))).LanguageModel("low").DoGenerateAsync(Prompt("Hello"), CancellationToken.None);
        Assert.Equal(FinishReason.Length, length.FinishReason);
        Assert.Equal("max_output_tokens", length.RawFinishReason);

        var filtered = await Create(new AgentHandler(SampleResponse("{\"status\":\"incomplete\",\"incomplete_details\":{\"reason\":\"content_filter\"}}"))).LanguageModel("low").DoGenerateAsync(Prompt("Hello"), CancellationToken.None);
        Assert.Equal(FinishReason.ContentFilter, filtered.FinishReason);
    }

    [Fact]
    public async Task Http_200_failures_throw()
    {
        var json = SampleResponse("{\"status\":\"failed\",\"error\":{\"message\":\"Agent run failed\",\"type\":\"server_error\"},\"output\":[]}");
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => Create(new AgentHandler(json)).LanguageModel("low").DoGenerateAsync(Prompt("Hello"), CancellationToken.None));

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("Agent run failed", exception.Message);
    }

    [Fact]
    public async Task A_later_search_result_keeps_its_id_for_the_same_url()
    {
        var json = SampleResponse("{\"output\":[{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"Answer\",\"annotations\":[{\"type\":\"url_citation\",\"url\":\"https://example.com/source\",\"title\":\"Annotation title\"}]}]},{\"type\":\"search_results\",\"results\":[{\"id\":7,\"title\":\"Search result title\",\"url\":\"https://example.com/source\",\"snippet\":\"Search result snippet.\"}]}]}");
        var result = await Create(new AgentHandler(json)).LanguageModel("low").DoGenerateAsync(Prompt("Hello"), CancellationToken.None);
        var source = Assert.IsType<GeneratedSource>(result.Content.First(part => part is GeneratedSource));

        Assert.Equal("7", source.Id);
        Assert.Equal("Search result title", source.Title);
        Assert.Equal(7, source.ProviderMetadata!.Value.GetProperty("perplexity").GetProperty("resultId").GetInt32());
        Assert.Equal(1, result.Content.Count(part => part is GeneratedSource));
    }

    [Fact]
    public async Task Unsupported_sampling_warns_without_aliasing_sonar()
    {
        var handler = new AgentHandler(SampleResponse());
        var options = Prompt("Hello");
        options.TopK = 1;
        options.Reasoning = "none";
        options.ToolChoice = ToolChoice.Required;
        var result = await Create(handler).LanguageModel("sonar-pro").DoGenerateAsync(options, CancellationToken.None);

        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Message == "topK");
        Assert.Contains(result.Warnings, warning => warning.Message.Contains("reasoning"));
        Assert.Contains(result.Warnings, warning => warning.Message.Contains("toolChoice"));
        Assert.DoesNotContain(result.Warnings, warning => warning.Type == "deprecated");
        Assert.Equal("sonar-pro", Body(handler).GetProperty("model").GetString());
    }

    [Fact]
    public async Task Recorded_sse_emits_text_once_and_unique_sources()
    {
        var sse = File.ReadAllText(Fixture("perplexity-agent-web-search.sse"));
        var handler = new AgentHandler(sse, sse: true);
        var parts = await Read(Create(handler).LanguageModel("fast").DoStreamAsync(Prompt("Find TypeScript"), CancellationToken.None));

        Assert.DoesNotContain(parts, part => part is ErrorStreamPart);
        var text = string.Concat(parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta));
        Assert.Equal("The official TypeScript website is **typescriptlang.org**; TypeScript is a strongly typed programming language that builds on JavaScript and adds syntax for types.[7]", text);
        var sources = parts.OfType<SourceStreamPart>().ToList();
        Assert.NotEmpty(sources);
        Assert.Equal(sources.Count, sources.Select(source => source.Url).Distinct().Count());
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("completed", finish.RawFinishReason);
        Assert.Equal(3007, finish.Usage.InputTokens);
        Assert.True(Body(handler).GetProperty("stream").GetBoolean());
    }

    [Fact]
    public async Task Stream_recovers_terminal_text_without_repeating_parts()
    {
        var done = Event("{\"type\":\"response.output_text.done\",\"item_id\":\"msg-123\",\"output_index\":0,\"content_index\":0,\"text\":\"Hello from Perplexity.\"}");
        var parts = await Read(Create(new AgentHandler(done, sse: true)).LanguageModel("low").DoStreamAsync(Prompt("Hello"), CancellationToken.None));
        Assert.Equal(new[] { "text-start", "text-delta", "text-end" }, parts.Where(part => part.Type.StartsWith("text-", StringComparison.Ordinal)).Select(part => part.Type));
        Assert.Equal("Hello from Perplexity.", string.Concat(parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta)));

        var message = "{\"type\":\"message\",\"id\":\"msg-123\",\"content\":[{\"type\":\"output_text\",\"text\":\"Hello world.\"},{\"type\":\"output_text\",\"text\":\"Second part.\"}]}";
        var events = string.Join(string.Empty, new[]
        {
            Event("{\"type\":\"response.output_text.delta\",\"item_id\":\"msg-123\",\"content_index\":0,\"delta\":\"Hello \"}"),
            Event("{\"type\":\"response.output_text.done\",\"item_id\":\"msg-123\",\"content_index\":0,\"text\":\"Hello world.\"}"),
            Event("{\"type\":\"response.output_text.delta\",\"item_id\":\"msg-123\",\"content_index\":1,\"delta\":\"Second \"}"),
            Event("{\"type\":\"response.output_item.done\",\"output_index\":0,\"item\":" + message + "}"),
            Event("{\"type\":\"response.completed\",\"response\":" + SampleResponse("{\"output\":[" + message + "]}") + "}"),
        });
        var recovered = await Read(Create(new AgentHandler(events, sse: true)).LanguageModel("low").DoStreamAsync(Prompt("Hello"), CancellationToken.None));
        var deltas = recovered.OfType<TextDeltaStreamPart>().ToList();
        Assert.Equal(new[] { "msg-123", "msg-123", "msg-123:1", "msg-123:1" }, deltas.Select(part => part.Id));
        Assert.Equal("Hello world.Second part.", string.Concat(deltas.Select(part => part.Delta)));
    }

    [Fact]
    public async Task Stream_keeps_one_source_url_and_its_search_id()
    {
        var search = "{\"id\":7,\"title\":\"Search result\",\"url\":\"https://example.com/source\",\"snippet\":\"Search result content.\"}";
        var message = "{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"Answer\",\"annotations\":[{\"type\":\"url_citation\",\"url\":\"https://example.com/source\"}]}]}";
        var events = string.Join(string.Empty, new[]
        {
            Event("{\"type\":\"response.reasoning.fetch_url_results\",\"contents\":[{\"title\":\"Fetched page\",\"url\":\"https://example.com/source\",\"snippet\":\"Fetched content.\"}]}"),
            Event("{\"type\":\"response.reasoning.search_results\",\"results\":[" + search + "]}"),
            Event("{\"type\":\"response.output_item.done\",\"item\":" + message + "}"),
            Event("{\"type\":\"response.completed\",\"response\":" + SampleResponse("{\"output\":[{\"type\":\"search_results\",\"results\":[" + search + "]}," + message + "]}") + "}"),
        });
        var parts = await Read(Create(new AgentHandler(events, sse: true)).LanguageModel("low").DoStreamAsync(Prompt("Hello"), CancellationToken.None));
        var source = Assert.Single(parts.OfType<SourceStreamPart>());

        Assert.Equal("7", source.Id);
        Assert.Equal("https://example.com/source", source.Url);
        Assert.Equal(7, source.ProviderMetadata!.Value.GetProperty("perplexity").GetProperty("resultId").GetInt32());
    }

    [Fact]
    public async Task Stream_handles_reasoning_incomplete_text_failures_and_raw_chunks()
    {
        var reasoning = string.Join(string.Empty, new[]
        {
            Event("{\"type\":\"response.reasoning.started\",\"sequence_number\":0,\"thought\":\"Planning. \"}"),
            Event("{\"type\":\"response.reasoning.search_queries\",\"sequence_number\":1,\"thought\":\"Searching. \"}"),
            Event("{\"type\":\"response.reasoning.stopped\",\"sequence_number\":5,\"thought\":\"Done.\"}"),
        });
        var reasoned = await Read(Create(new AgentHandler(reasoning, sse: true)).LanguageModel("low").DoStreamAsync(Prompt("Hello"), CancellationToken.None));
        Assert.Equal("Planning. Searching. Done.", string.Concat(reasoned.OfType<ReasoningDeltaStreamPart>().Select(part => part.Delta)));
        Assert.Contains(reasoned, part => part is ReasoningStartStreamPart start && start.Id == "reasoning-0");
        Assert.Contains(reasoned, part => part is ReasoningEndStreamPart end && end.Id == "reasoning-0");

        var incomplete = Event("{\"type\":\"response.incomplete\",\"response\":" + SampleResponse("{\"status\":\"incomplete\",\"incomplete_details\":{\"reason\":\"max_output_tokens\"}}") + "}");
        var incompleteParts = await Read(Create(new AgentHandler(incomplete, sse: true)).LanguageModel("low").DoStreamAsync(Prompt("Hello"), CancellationToken.None));
        Assert.Equal("Hello from Perplexity.", string.Concat(incompleteParts.OfType<TextDeltaStreamPart>().Select(part => part.Delta)));
        var incompleteFinish = Assert.Single(incompleteParts.OfType<FinishStreamPart>());
        Assert.Equal(FinishReason.Length, incompleteFinish.FinishReason);
        Assert.Equal(120, incompleteFinish.Usage.InputTokens);

        var failed = Event("{\"type\":\"response.failed\",\"error\":{\"message\":\"Agent run failed\",\"type\":\"server_error\"}}");
        var failedParts = await Read(Create(new AgentHandler(failed, sse: true)).LanguageModel("low").DoStreamAsync(Prompt("Hello"), CancellationToken.None));
        Assert.Equal("Agent run failed", Assert.Single(failedParts.OfType<ErrorStreamPart>()).Message);
        Assert.Equal(FinishReason.Error, Assert.Single(failedParts.OfType<FinishStreamPart>()).FinishReason);

        var malformed = "data: {not-json}\n\n";
        var malformedParts = await Read(Create(new AgentHandler(malformed, sse: true)).LanguageModel("low").DoStreamAsync(Prompt("Hello"), CancellationToken.None));
        Assert.Contains(malformedParts, part => part is ErrorStreamPart);
        Assert.Equal(FinishReason.Error, Assert.Single(malformedParts.OfType<FinishStreamPart>()).FinishReason);

        var finance = "{\"type\":\"finance_results\",\"results\":[{\"category\":\"quote\"}]}";
        var rawEvents = Event("{\"type\":\"response.output_item.done\",\"item\":" + finance + "}")
            + Event("{\"type\":\"response.completed\",\"response\":" + SampleResponse("{\"output\":[" + finance + "]}") + "}");
        var rawOptions = Prompt("Hello");
        rawOptions.IncludeRawChunks = true;
        var rawParts = await Read(Create(new AgentHandler(rawEvents, sse: true)).LanguageModel("low").DoStreamAsync(rawOptions, CancellationToken.None));
        Assert.DoesNotContain(rawParts, part => part is ErrorStreamPart);
        Assert.Contains(rawParts.OfType<RawStreamPart>(), part => part.RawJson.Contains("finance_results"));
        Assert.Equal(FinishReason.Stop, Assert.Single(rawParts.OfType<FinishStreamPart>()).FinishReason);
    }

    [Fact]
    public async Task Stream_emits_function_calls_once()
    {
        var call = "{\"id\":\"fc-123\",\"type\":\"function_call\",\"call_id\":\"call-123\",\"name\":\"weather\",\"arguments\":\"{\\\"city\\\":\\\"San Francisco\\\"}\"}";
        var events = string.Join(string.Empty, new[]
        {
            Event("{\"type\":\"response.output_item.done\",\"item\":" + call + "}"),
            Event("{\"type\":\"response.completed\",\"response\":" + SampleResponse("{\"status\":\"requires_action\",\"output\":[" + call + "]}") + "}"),
        });
        var parts = await Read(Create(new AgentHandler(events, sse: true)).LanguageModel("low").DoStreamAsync(Prompt("Hello"), CancellationToken.None));
        var tool = Assert.Single(parts.OfType<ToolCallStreamPart>());

        Assert.Equal("call-123", tool.ToolCallId);
        Assert.Equal("weather", tool.ToolName);
        Assert.Equal(FinishReason.ToolCalls, Assert.Single(parts.OfType<FinishStreamPart>()).FinishReason);
    }

    private static PerplexityProvider Create(AgentHandler handler)
    {
        return PerplexityProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, handler);
    }

    private static LanguageModelCallOptions Prompt(string text)
    {
        return new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage(text) } };
    }

    private static LanguageModelCallOptions FilePrompt(string mediaType)
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[] { new FileContentPart(mediaType, null, new byte[] { 1 }, null) }),
            },
        };
    }

    private static string SampleResponse(string? overrides = null)
    {
        var response = "{"
            + "\"id\":\"resp-123\","
            + "\"created_at\":1784292159,"
            + "\"model\":\"openai/gpt-5.1\","
            + "\"object\":\"response\","
            + "\"output\":["
            + "{\"type\":\"search_results\",\"results\":[{\"id\":1,\"title\":\"Example source\",\"url\":\"https://example.com/source\",\"snippet\":\"An example search result.\",\"date\":\"2026-08-01\",\"source\":\"web\"}]},"
            + "{\"id\":\"msg-123\",\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"Hello from Perplexity.\",\"annotations\":[]}]}"
            + "],"
            + "\"status\":\"completed\","
            + "\"usage\":{"
            + "\"input_tokens\":120,"
            + "\"input_tokens_details\":{\"cache_creation_input_tokens\":10,\"cache_read_input_tokens\":20},"
            + "\"output_tokens\":45,"
            + "\"output_tokens_details\":{\"reasoning_tokens\":5},"
            + "\"total_tokens\":165,"
            + "\"tool_calls_details\":{\"search_web\":{\"invocation\":2}},"
            + "\"cost\":{\"currency\":\"USD\",\"input_cost\":0.001,\"output_cost\":0.002,\"tool_calls_cost\":0.003,\"total_cost\":0.006}"
            + "}}";
        if (overrides == null)
        {
            return response;
        }

        using var document = JsonDocument.Parse(response);
        using var patch = JsonDocument.Parse(overrides);
        var merged = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(document.RootElement.GetRawText())!;
        foreach (var property in patch.RootElement.EnumerateObject())
        {
            merged[property.Name] = property.Value.Clone();
        }

        return JsonSerializer.Serialize(merged);
    }

    private static string Event(string json)
    {
        return "data: " + json + "\n\n";
    }

    private static JsonElement Body(AgentHandler handler)
    {
        using var document = JsonDocument.Parse(handler.Body);
        return document.RootElement.Clone();
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static string Fixture(string name)
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", name);
    }

    private static async Task<List<LanguageModelStreamPart>> Read(IAsyncEnumerable<LanguageModelStreamPart> stream)
    {
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in stream)
        {
            parts.Add(part);
        }

        return parts;
    }

    private sealed class AgentHandler : HttpMessageHandler
    {
        private readonly string _body;
        private readonly bool _sse;

        public AgentHandler(string body, bool sse = false)
        {
            _body = body;
            _sse = sse;
        }

        public int Calls { get; private set; }

        public string Uri { get; private set; } = string.Empty;

        public string Body { get; private set; } = string.Empty;

        public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            Uri = request.RequestUri?.AbsoluteUri ?? string.Empty;
            Body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            Headers.Clear();
            foreach (var header in request.Headers)
            {
                Headers[header.Key] = string.Join(",", header.Value);
            }

            if (request.Headers.Authorization != null)
            {
                Headers["Authorization"] = request.Headers.Authorization.ToString();
            }

            if (Uri.Contains("/embeddings"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"data\":[{\"embedding\":[0.25,0.5]}]}", Encoding.UTF8, "application/json"),
                };
            }

            var mediaType = _sse ? "text/event-stream" : "application/json";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_body, Encoding.UTF8, mediaType),
            };
        }
    }
}
