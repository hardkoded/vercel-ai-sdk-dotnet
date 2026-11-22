// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.QuiverAI;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Util;
using static Vercel.AI.Tests.QuiverAILanguageModel.QuiverAILanguageModelSupport;
using QuiverModel = Vercel.AI.QuiverAI.QuiverAILanguageModel;

namespace Vercel.AI.Tests.QuiverAILanguageModel;

public sealed class WorkflowSerializationTests
{
    private const string Prefix =
        "packages/quiverai/src/quiverai-language-model.test.ts::QuiverAI language model > workflow serialization::";

    [Fact]
    [UpstreamTest(
        Prefix + "preserves model settings, authentication, and QuiverAI request policy",
        Coverage = UpstreamCoverage.Partial,
        Note = "The API key is not serialized, so it is passed to Deserialize. The port has no custom fetch option, so the custom-fetch guard is not ported."
    )]
    public async Task Preserves_model_settings_authentication_and_QuiverAI_request_policy()
    {
        var handler = Json(CreateResponse());
        var model = RoundTrip(
            CreateQuiverAI(handler, "https://example.com/quiverai/", options =>
                    options.Headers["x-custom"] = "custom-value"
                )
                .LanguageModel("arrow-2-telos"),
            handler
        );

        Assert.Equal("arrow-2-telos", model.ModelId);
        Assert.Equal("quiverai.responses", model.Provider);
        await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            model.DoGenerateAsync(
                new LanguageModelCallOptions
                {
                    Prompt = Call().Prompt,
                    ProviderOptions = ProviderOptions("{\"reasoningEffort\":\"max\"}"),
                },
                CancellationToken.None
            )
        );

        var result = await model.DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = Call().Prompt,
                ProviderOptions = ProviderOptions("{\"reasoningEffort\":\"high\"}"),
                JsonSchema = JsonDocument.Parse("{\"type\":\"object\"}").RootElement,
            },
            CancellationToken.None
        );

        Assert.Contains(result.Warnings, warning => warning.Feature == "responseFormat");
        var call = Assert.Single(handler.Calls);
        Assert.Equal(CustomUrl, call.Uri.AbsoluteUri);
        Assert.Equal("Bearer test-api-key", call.Header("Authorization"));
        Assert.Equal("custom-value", call.Header("x-custom"));
        Assert.Single(
            System.Text.RegularExpressions.Regex.Matches(
                call.Header("User-Agent")!,
                "ai-sdk-quiverai/"
            )
        );
        var body = Body(call);
        Assert.Equal("arrow-2-telos", body["model"]!.GetValue<string>());
        Assert.Equal("high", body["reasoning"]!["effort"]!.GetValue<string>());
        Assert.Null(body["text"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "restores QuiverAI HTTP error handling",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Restores_QuiverAI_HTTP_error_handling()
    {
        var handler = Json(
            "{\"status\":503,\"code\":\"service_unavailable\",\"message\":\"Model capacity is temporarily unavailable.\",\"request_id\":\"req_1\"}",
            503
        );
        var model = RoundTrip(CreateQuiverAI(handler).LanguageModel("arrow-2"), handler);

        var error = await Assert.ThrowsAnyAsync<ApiException>(() =>
            model.DoGenerateAsync(Call(), CancellationToken.None)
        );

        Assert.Equal("Model capacity is temporarily unavailable.", error.Message);
        Assert.Equal(503, error.StatusCode);
        Assert.True(ProviderHttp.IsRetryable(error.StatusCode));
    }

    [Theory]
    [InlineData("doGenerate")]
    [InlineData("doStream")]
    [UpstreamTest(
        Prefix + "restores QuiverAI response error metadata in %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Restores_QuiverAI_response_error_metadata_in(string method)
    {
        var response = CreateResponse(
            status: "failed",
            error: "{\"code\":\"service_unavailable\",\"message\":\"Try again later.\",\"status_code\":503}"
        );
        var handler =
            method == "doGenerate"
                ? Json(response)
                : Sse(
                    "{\"type\":\"response.failed\",\"sequence_number\":0,\"response\":"
                        + response
                        + "}"
                );
        var model = RoundTrip(CreateQuiverAI(handler).LanguageModel("arrow-2"), handler);

        if (method == "doGenerate")
        {
            var error = await Assert.ThrowsAsync<ApiException>(() =>
                model.DoGenerateAsync(Call(), CancellationToken.None)
            );
            Assert.Equal("Try again later.", error.Message);
            Assert.Equal(503, error.StatusCode);
            Assert.True(error.IsRetryable);
        }
        else
        {
            var client = new AiClient(
                GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
            );
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
            Assert.Equal("Try again later.", error.Message);
            Assert.Equal(503, error.StatusCode);
            Assert.True(error.IsRetryable);
        }
    }

    private static QuiverModel RoundTrip(ILanguageModel model, ParityHandler handler)
    {
        var serialized = ((QuiverModel)model).Serialize();
        var json = JsonSerializer.Serialize(new { serialized.ModelId, serialized.Config });
        using var document = JsonDocument.Parse(json);
        var config = new Dictionary<string, object?>();
        foreach (var property in document.RootElement.GetProperty("Config").EnumerateObject())
        {
            config[property.Name] = property.Value.Clone();
        }

        return QuiverModel.Deserialize(
            new SerializedModelOptions(
                document.RootElement.GetProperty("ModelId").GetString()!,
                config
            ),
            "test-api-key",
            handler
        );
    }
}
