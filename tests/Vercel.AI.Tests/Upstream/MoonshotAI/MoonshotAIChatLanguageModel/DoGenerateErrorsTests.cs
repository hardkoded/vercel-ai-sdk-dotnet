// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel.MoonshotLanguageModelSupportB;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel;

/// <summary>Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; errors</c>.</summary>
public sealed class DoGenerateErrorsTests
{
    private const string Prefix = TestFile + "::doGenerate > errors::";

    [Fact]
    [UpstreamTest(
        Prefix + "should reject unsupported media before sending a request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_unsupported_media_before_sending_a_request()
    {
        var handler = new SequencedHandler().Json("{}");

        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            CreateProvider(handler)
                .CreateChatModel("kimi-k3")
                .DoGenerateAsync(
                    new LanguageModelCallOptions
                    {
                        Prompt = new ModelMessage[]
                        {
                            new UserModelMessage(
                                new UserContentPart[]
                                {
                                    new FileContentPart(
                                        "image/svg+xml",
                                        null,
                                        Encoding.UTF8.GetBytes("<svg></svg>"),
                                        null
                                    ),
                                }
                            ),
                        },
                    },
                    CancellationToken.None
                )
        );

        Assert.Empty(handler.Bodies);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve the moonshot error envelope",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_the_moonshot_error_envelope()
    {
        var text = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "moonshotai-error.json")
        );

        await AssertEnvelope(text);
    }

    [Theory]
    [InlineData(
        "nullable code",
        "{\"error\":{\"message\":\"Invalid request with nullable code\",\"type\":\"invalid_request_error\",\"code\":null}}"
    )]
    [InlineData("message-only error", "{\"error\":{\"message\":\"Invalid request\"}}")]
    [UpstreamTest(Prefix + "should preserve a $name envelope", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_a_name_envelope(string name, string body)
    {
        Assert.NotEmpty(name);
        await AssertEnvelope(body);
    }

    private static async Task AssertEnvelope(string body)
    {
        var handler = new SequencedHandler().Json(body, HttpStatusCode.BadRequest);

        var error = await Assert.ThrowsAsync<BadRequestException>(() =>
            CreateProvider(handler)
                .CreateChatModel("kimi-k3")
                .DoGenerateAsync(TestPrompt(), CancellationToken.None)
        );

        using var document = JsonDocument.Parse(body);
        Assert.Equal(
            document.RootElement.GetProperty("error").GetProperty("message").GetString(),
            error.Message
        );
        Assert.NotNull(error.ErrorData);
        var data = error.ErrorData is JsonElement element
            ? element.GetRawText()
            : System.Text.Json.JsonSerializer.Serialize(error.ErrorData);
        Assert.True(
            System.Text.Json.Nodes.JsonNode.DeepEquals(
                System.Text.Json.Nodes.JsonNode.Parse(body),
                System.Text.Json.Nodes.JsonNode.Parse(data)
            ),
            data
        );
    }
}
