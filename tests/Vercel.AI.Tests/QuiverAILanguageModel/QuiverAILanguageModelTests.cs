// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;
using Vercel.AI.Util;
using static Vercel.AI.Tests.QuiverAILanguageModel.QuiverAILanguageModelSupport;

namespace Vercel.AI.Tests.QuiverAILanguageModel;

public sealed class QuiverAILanguageModelTests
{
    private const string Prefix =
        "packages/quiverai/src/quiverai-language-model.test.ts::QuiverAI language model::";

    private const string FlatError =
        "{\"status\":503,\"code\":\"service_unavailable\",\"message\":\"Model capacity is temporarily unavailable.\",\"request_id\":\"req_1\"}";

    private const string FailedError =
        "{\"code\":\"service_unavailable\",\"message\":\"Try again later.\",\"status_code\":503}";

    [Fact]
    [UpstreamTest(
        Prefix + "preserves flat QuiverAI HTTP error details",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_flat_QuiverAI_HTTP_error_details()
    {
        var model = CreateQuiverAI(Json(FlatError, 503)).LanguageModel("arrow-2");

        var error = await Assert.ThrowsAnyAsync<ApiException>(() =>
            model.DoGenerateAsync(Call(), CancellationToken.None)
        );

        Assert.Equal("Model capacity is temporarily unavailable.", error.Message);
        Assert.Equal(503, error.StatusCode);
        Assert.True(error.IsRetryable);
        var data = JsonNode.Parse(error.ResponseBody!)!;
        Assert.Equal(503, data["status"]!.GetValue<int>());
        Assert.Equal("service_unavailable", data["code"]!.GetValue<string>());
        Assert.Equal("req_1", data["request_id"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "uses response error status codes for retry classification",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Uses_response_error_status_codes_for_retry_classification()
    {
        var model = CreateQuiverAI(Json(CreateResponse(status: "failed", error: FailedError)))
            .LanguageModel("arrow-2");

        var error = await Assert.ThrowsAnyAsync<ApiException>(() =>
            model.DoGenerateAsync(Call(), CancellationToken.None)
        );

        Assert.Equal("Try again later.", error.Message);
        Assert.Equal(503, error.StatusCode);
        Assert.True(error.IsRetryable);
        var data = JsonSerializer.SerializeToNode(error.ErrorData)!;
        Assert.Equal("service_unavailable", data["code"]!.GetValue<string>());
        Assert.Equal("Try again later.", data["message"]!.GetValue<string>());
        Assert.Equal(503, data["status_code"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "generates text through the stateless Responses endpoint",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Generates_text_through_the_stateless_Responses_endpoint()
    {
        var handler = Json(
            CreateResponse(
                "[{\"id\":\"msg_1\",\"type\":\"message\",\"role\":\"assistant\",\"status\":\"completed\",\"content\":[{\"type\":\"output_text\",\"text\":\"Use a simple blue compass.\",\"annotations\":[]}]}]"
            )
        );

        var result = await CreateQuiverAI(handler)
            .LanguageModel("arrow-2")
            .DoGenerateAsync(
                new LanguageModelCallOptions
                {
                    Prompt = Call().Prompt,
                    ProviderOptions = ProviderOptions(
                        "{\"reasoningEffort\":\"high\",\"reasoningSummary\":\"auto\"}"
                    ),
                },
                CancellationToken.None
            );

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("Use a simple blue compass.", text.Text);
        JsonAssert.Equal(text.ProviderMetadata!.Value, "{\"quiverai\":{\"itemId\":\"msg_1\"}}");
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Null(result.RawFinishReason);
        Assert.Equal(20, result.Usage.InputTokens);
        Assert.Equal(12, result.Usage.NoCacheInputTokens);
        Assert.Equal(5, result.Usage.CacheReadTokens);
        Assert.Equal(3, result.Usage.CacheWriteTokens);
        Assert.Equal(12, result.Usage.OutputTokens);
        Assert.Equal(8, result.Usage.TextTokens);
        Assert.Equal(4, result.Usage.ReasoningTokens);
        Assert.NotNull(result.Usage.Raw);

        var call = Assert.Single(handler.Calls);
        Assert.Equal(Url, call.Uri.AbsoluteUri);
        Assert.Equal("Bearer test-api-key", call.Header("Authorization"));
        Assert.StartsWith("application/json", call.Header("Content-Type"));
        Assert.Contains("ai-sdk-quiverai/", call.Header("User-Agent"));
        Assert.Single(
            System.Text.RegularExpressions.Regex.Matches(
                call.Header("User-Agent")!,
                "ai-sdk-quiverai/"
            )
        );
        JsonAssert.Equal(
            Body(call),
            "{\"model\":\"arrow-2\",\"input\":[{\"type\":\"message\",\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Create an icon.\"}]}],\"reasoning\":{\"effort\":\"high\",\"summary\":\"auto\"}}"
        );
    }

    [Theory]
    [InlineData("doGenerate")]
    [InlineData("doStream")]
    [UpstreamTest(
        Prefix + "replays opaque reasoning ids without private or encrypted reasoning in %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Replays_opaque_reasoning_ids_without_private_or_encrypted_reasoning_in(
        string method
    )
    {
        var handler = method == "doGenerate" ? Json(CreateResponse()) : Sse();
        var model = CreateQuiverAI(handler).LanguageModel("arrow-2");
        var options = new LanguageModelCallOptions
        {
            Prompt =
            [
                new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantReasoningPart(
                            "private reasoning",
                            ProviderOptions(
                                "{\"itemId\":\"rs_opaque\",\"reasoningSummary\":[{\"type\":\"summary_text\",\"text\":\"safe summary\"}],\"reasoningContent\":[{\"type\":\"reasoning_text\",\"text\":\"private reasoning\"}],\"reasoningEncryptedContent\":\"encrypted\"}"
                            )
                        ),
                        new AssistantToolCallPart(
                            new GeneratedToolCall(
                                "call_1",
                                "write_file",
                                "{\"path\":\"icon.svg\"}",
                                JsonDocument
                                    .Parse("{\"quiverai\":{\"itemId\":\"fc_1\"}}")
                                    .RootElement
                            )
                        ),
                    }
                ),
                new ToolModelMessage("call_1", "write_file", "{\"staged\":true}", false),
            ],
        };

        if (method == "doGenerate")
        {
            await model.DoGenerateAsync(options, CancellationToken.None);
        }
        else
        {
            await ReadAll(await model.DoStreamAsync(options, CancellationToken.None));
        }

        JsonAssert.Equal(
            Body(Assert.Single(handler.Calls))["input"],
            "[{\"type\":\"reasoning\",\"id\":\"rs_opaque\",\"summary\":[{\"type\":\"summary_text\",\"text\":\"safe summary\"}]},{\"type\":\"function_call\",\"id\":\"fc_1\",\"call_id\":\"call_1\",\"name\":\"write_file\",\"arguments\":\"{\\\"path\\\":\\\"icon.svg\\\"}\"},{\"type\":\"function_call_output\",\"call_id\":\"call_1\",\"output\":\"{\\\"staged\\\":true}\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "warns and omits structured output settings",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Warns_and_omits_structured_output_settings()
    {
        var handler = Json(CreateResponse());

        var result = await CreateQuiverAI(handler)
            .LanguageModel("arrow-2")
            .DoGenerateAsync(
                new LanguageModelCallOptions
                {
                    Prompt = Call().Prompt,
                    JsonSchema = JsonDocument.Parse("{\"type\":\"object\"}").RootElement,
                },
                CancellationToken.None
            );

        var warning = Assert.Single(result.Warnings, w => w.Feature == "responseFormat");
        Assert.Equal("unsupported", warning.Type);
        Assert.Null(Body(Assert.Single(handler.Calls))["text"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "validates QuiverAI reasoning values",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Validates_QuiverAI_reasoning_values()
    {
        await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            CreateQuiverAI(Json(CreateResponse()))
                .LanguageModel("arrow-2")
                .DoGenerateAsync(
                    new LanguageModelCallOptions
                    {
                        Prompt = Call().Prompt,
                        ProviderOptions = ProviderOptions("{\"reasoningEffort\":\"max\"}"),
                    },
                    CancellationToken.None
                )
        );
    }

    [Theory]
    [InlineData("response.failed")]
    [InlineData("error")]
    [UpstreamTest(
        Prefix + "preserves retry metadata for streamed %s events",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_retry_metadata_for_streamed_events(string type)
    {
        var handler =
            type == "response.failed"
                ? Sse(
                    "{\"type\":\"response.failed\",\"sequence_number\":0,\"response\":"
                        + CreateResponse(status: "failed", error: FailedError)
                        + "}"
                )
                : Sse("{\"type\":\"error\",\"sequence_number\":0,\"error\":" + FailedError + "}");
        var model = CreateQuiverAI(handler).LanguageModel("arrow-2");
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
        var result = client.StreamTextAsync(
            new StreamTextOptions
            {
                Model = model,
                Prompt = "Create an icon.",
                MaxRetries = 0,
            }
        );

        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        var failure = Assert.IsType<ErrorPart>(Assert.Single(parts, p => p is ErrorPart));
        var error = Assert.IsType<Vercel.AI.Prompt.StreamProviderError>(failure.Error);
        Assert.Equal(503, error.StatusCode);
        Assert.True(error.IsRetryable);
    }

    [Theory]
    [InlineData("doGenerate")]
    [InlineData("doStream")]
    [UpstreamTest(
        Prefix + "validates native reasoning and maps unsupported top-level reasoning in %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Validates_native_reasoning_and_maps_unsupported_top_level_reasoning_in(
        string method
    )
    {
        var handler = method == "doGenerate" ? Json(CreateResponse()) : Sse();
        var model = CreateQuiverAI(handler).LanguageModel("arrow-2");

        Task Invoke(LanguageModelCallOptions options) =>
            method == "doGenerate"
                ? model.DoGenerateAsync(options, CancellationToken.None)
                : model.DoStreamAsync(options, CancellationToken.None);

        foreach (
            var quiverai in new[]
            {
                "{\"reasoningEffort\":\"none\"}",
                "{\"reasoningSummary\":\"detailed\"}",
            }
        )
        {
            await Assert.ThrowsAsync<InvalidArgumentException>(() =>
                Invoke(
                    new LanguageModelCallOptions
                    {
                        Prompt = Call().Prompt,
                        ProviderOptions = ProviderOptions(quiverai),
                    }
                )
            );
        }

        var options = new LanguageModelCallOptions { Prompt = Call().Prompt, Reasoning = "none" };
        IReadOnlyList<CallWarning> warnings;
        if (method == "doGenerate")
        {
            warnings = (await model.DoGenerateAsync(options, CancellationToken.None)).Warnings;
        }
        else
        {
            var parts = await ReadAll(await model.DoStreamAsync(options, CancellationToken.None));
            warnings = parts.OfType<StreamStartStreamPart>().First().Warnings;
        }

        var warning = Assert.Single(warnings, w => w.Feature == "reasoning");
        Assert.Equal("compatibility", warning.Type);
        Assert.Equal(
            "reasoning \"none\" is not directly supported by this model. mapped to effort \"low\".",
            warning.Message
        );
        Assert.Equal(
            "low",
            Body(Assert.Single(handler.Calls))["reasoning"]!["effort"]!.GetValue<string>()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "streams complete function calls and terminal usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Streams_complete_function_calls_and_terminal_usage()
    {
        const string item =
            "{\"id\":\"fc_1\",\"type\":\"function_call\",\"status\":\"completed\",\"call_id\":\"call_1\",\"name\":\"write_file\",\"arguments\":\"{\\\"path\\\":\\\"icon.svg\\\"}\"}";
        var handler = Sse(
            "{\"type\":\"response.output_item.added\",\"sequence_number\":0,\"output_index\":0,\"item\":{\"id\":\"fc_1\",\"type\":\"function_call\",\"status\":\"in_progress\",\"call_id\":\"call_1\",\"name\":\"write_file\",\"arguments\":\"\"}}",
            "{\"type\":\"response.function_call_arguments.delta\",\"sequence_number\":1,\"item_id\":\"fc_1\",\"output_index\":0,\"call_id\":\"call_1\",\"delta\":\"{\\\"path\\\":\"}",
            "{\"type\":\"response.function_call_arguments.done\",\"sequence_number\":2,\"item_id\":\"fc_1\",\"output_index\":0,\"call_id\":\"call_1\",\"arguments\":\"{\\\"path\\\":\\\"icon.svg\\\"}\"}",
            "{\"type\":\"response.output_item.done\",\"sequence_number\":3,\"output_index\":0,\"item\":"
                + item
                + "}",
            "{\"type\":\"response.completed\",\"sequence_number\":4,\"response\":"
                + CreateResponse("[" + item + "]")
                + "}"
        );

        var parts = await ReadAll(
            await CreateQuiverAI(handler)
                .LanguageModel("arrow-2-telos")
                .DoStreamAsync(Call(), CancellationToken.None)
        );

        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("call_1", call.ToolCallId);
        Assert.Equal("write_file", call.ToolName);
        Assert.Equal("{\"path\":\"icon.svg\"}", call.ArgumentsJson);
        JsonAssert.Equal(call.ProviderMetadata!.Value, "{\"quiverai\":{\"itemId\":\"fc_1\"}}");
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.ToolCalls, finish.FinishReason);
        Assert.Equal(20, finish.Usage.InputTokens);
        Assert.Equal(12, finish.Usage.NoCacheInputTokens);
        Assert.Equal(5, finish.Usage.CacheReadTokens);
        Assert.Equal(3, finish.Usage.CacheWriteTokens);
        Assert.Equal(12, finish.Usage.OutputTokens);
        Assert.Equal(8, finish.Usage.TextTokens);
        Assert.Equal(4, finish.Usage.ReasoningTokens);
        Assert.NotNull(finish.Usage.Raw);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "streams complete custom tool calls before execution",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Streams_complete_custom_tool_calls_before_execution()
    {
        const string item =
            "{\"id\":\"ct_1\",\"type\":\"custom_tool_call\",\"status\":\"completed\",\"call_id\":\"call_custom_1\",\"name\":\"write_svg\",\"input\":\"<svg />\"}";
        var handler = Sse(
            "{\"type\":\"response.output_item.added\",\"sequence_number\":0,\"output_index\":0,\"item\":{\"id\":\"ct_1\",\"type\":\"custom_tool_call\",\"status\":\"in_progress\",\"call_id\":\"call_custom_1\",\"name\":\"write_svg\",\"input\":\"\"}}",
            "{\"type\":\"response.custom_tool_call_input.delta\",\"sequence_number\":1,\"item_id\":\"ct_1\",\"output_index\":0,\"delta\":\"<svg\"}",
            "{\"type\":\"response.custom_tool_call_input.delta\",\"sequence_number\":2,\"item_id\":\"ct_1\",\"output_index\":0,\"delta\":\" />\"}",
            "{\"type\":\"response.output_item.done\",\"sequence_number\":3,\"output_index\":0,\"item\":"
                + item
                + "}",
            "{\"type\":\"response.completed\",\"sequence_number\":4,\"response\":"
                + CreateResponse("[" + item + "]")
                + "}"
        );

        var parts = await ReadAll(
            await CreateQuiverAI(handler)
                .LanguageModel("arrow-2")
                .DoStreamAsync(Call(), CancellationToken.None)
        );

        var start = Assert.Single(parts.OfType<ToolInputStartStreamPart>());
        Assert.Equal("call_custom_1", start.Id);
        Assert.Equal("write_svg", start.ToolName);
        Assert.Equal(
            new[] { "<svg", " />" },
            parts.OfType<ToolInputDeltaStreamPart>().Select(part => part.Delta)
        );
        Assert.All(parts.OfType<ToolInputDeltaStreamPart>(), part =>
            Assert.Equal("call_custom_1", part.Id)
        );
        Assert.Equal("call_custom_1", Assert.Single(parts.OfType<ToolInputEndStreamPart>()).Id);
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("call_custom_1", call.ToolCallId);
        Assert.Equal("write_svg", call.ToolName);
        Assert.Equal("\"<svg />\"", call.ArgumentsJson);
        JsonAssert.Equal(call.ProviderMetadata!.Value, "{\"quiverai\":{\"itemId\":\"ct_1\"}}");
        Assert.Equal(
            FinishReason.ToolCalls,
            Assert.IsType<FinishStreamPart>(parts[^1]).FinishReason
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "cancels a Responses request with the supplied abort signal",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port has no custom fetch with an abort signal. The test cancels the CancellationToken of the call."
    )]
    public async Task Cancels_a_Responses_request_with_the_supplied_abort_signal()
    {
        var started = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var handler = new ParityHandler
        {
            RespondAsync = async call =>
            {
                started.TrySetResult(true);
                await Task.Delay(Timeout.Infinite, call.CancellationToken).ConfigureAwait(false);
                throw new InvalidOperationException("Unreachable.");
            },
        };
        using var cts = new CancellationTokenSource();

        var request = CreateQuiverAI(handler)
            .LanguageModel("arrow-2")
            .DoGenerateAsync(Call(), cts.Token);
        await started.Task;
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => request);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "supports caller-executed custom tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Supports_caller_executed_custom_tools()
    {
        var handler = Json(
            CreateResponse(
                "[{\"id\":\"ct_1\",\"type\":\"custom_tool_call\",\"status\":\"completed\",\"call_id\":\"call_custom_1\",\"name\":\"write_svg\",\"input\":\"<svg />\"}]"
            )
        );

        var result = await CreateQuiverAI(handler)
            .LanguageModel("arrow-2")
            .DoGenerateAsync(
                new LanguageModelCallOptions
                {
                    Prompt = Call().Prompt,
                    Tools =
                    [
                        LanguageModelTool.ProviderDefined(
                            "quiverai.custom",
                            "write_svg",
                            JsonDocument
                                .Parse(
                                    "{\"description\":\"Return SVG source.\",\"format\":{\"type\":\"text\"}}"
                                )
                                .RootElement
                        ),
                    ],
                    ToolChoice = ToolChoice.Tool("write_svg"),
                },
                CancellationToken.None
            );

        var body = Body(Assert.Single(handler.Calls));
        JsonAssert.Equal(
            body["tools"],
            "[{\"type\":\"custom\",\"name\":\"write_svg\",\"description\":\"Return SVG source.\",\"format\":{\"type\":\"text\"}}]"
        );
        JsonAssert.Equal(body["tool_choice"], "{\"type\":\"custom\",\"name\":\"write_svg\"}");
        var call = Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content));
        Assert.Equal("call_custom_1", call.ToolCallId);
        Assert.Equal("write_svg", call.ToolName);
        Assert.Equal("\"<svg />\"", call.ArgumentsJson);
        JsonAssert.Equal(call.ProviderMetadata!.Value, "{\"quiverai\":{\"itemId\":\"ct_1\"}}");
    }
}
