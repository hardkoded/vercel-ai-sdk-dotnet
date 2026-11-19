// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenAIResponsesInputTransformations;

/// <summary>Port of <c>openai-responses-input-transformations.test.ts</c> &gt; <c>Responses input transformations (%s)</c>.</summary>
public sealed class ResponsesInputTransformationsTests
{
    private const string Prefix =
        "packages/openai/src/responses/openai-responses-input-transformations.test.ts::Responses input transformations (%s)::";

    private const string Commit = "1a7440bcdea3a97a481dad5fea320f2accb703ae";

    public static TheoryData<string, string, string?> Cases()
    {
        var data = new TheoryData<string, string, string?>();
        foreach (var method in new[] { "generate", "stream" })
        {
            data.Add(
                method,
                "a removed reasoning item",
                "[{\"id\":\"rs_1\",\"type\":\"reasoning\",\"action\":\"removed\",\"reason\":\"prefix_mismatch\"}]"
            );
            data.Add(
                method,
                "several removed reasoning items",
                "[{\"id\":\"rs_1\",\"type\":\"reasoning\",\"action\":\"removed\",\"reason\":\"end_user_mismatch\"},{\"id\":\"rs_2\",\"type\":\"reasoning\",\"action\":\"removed\",\"reason\":\"model_mismatch\"}]"
            );
            data.Add(method, "no transformations", "[]");
            data.Add(
                method,
                "a future action and reason without an id",
                "[{\"type\":\"reasoning\",\"action\":\"future_action\",\"reason\":\"future_reason\"}]"
            );
            data.Add(method, "null", "null");
            data.Add(method, "a missing field", null);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(Prefix + "returns $name", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Returns(string method, string name, string? transformations)
    {
        Assert.NotNull(name);
        var response = new JsonObject
        {
            ["id"] = "resp_test",
            ["created_at"] = 0,
            ["model"] = "gpt-6.2-astra",
            ["output"] = new JsonArray(),
        };
        if (transformations != null)
        {
            response["input_transformations"] = JsonNode.Parse(transformations);
        }

        var capture = new OpenAICapture();
        if (method == "generate")
        {
            capture.ResponseJson = response.ToJsonString();
        }
        else
        {
            capture.ServerSentEvents =
                "data: {\"type\":\"response.completed\",\"response\":"
                + response.ToJsonString()
                + "}\n\ndata: [DONE]\n\n";
        }

        var model = OpenAIUpstream.Provider(capture).ResponsesModel("gpt-6.2-astra");
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
        };

        JsonElement? metadata;
        if (method == "generate")
        {
            metadata = (
                await model.DoGenerateAsync(options, CancellationToken.None)
            ).ProviderMetadata;
        }
        else
        {
            var parts = new List<LanguageModelStreamPart>();
            await foreach (var part in await model.DoStreamAsync(options, CancellationToken.None))
            {
                parts.Add(part);
            }

            metadata = parts.OfType<FinishStreamPart>().First().ProviderMetadata;
        }

        var openai =
            metadata is { ValueKind: JsonValueKind.Object } root
            && root.TryGetProperty("openai", out var value)
                ? value
                : default;
        if (transformations == null || transformations == "null")
        {
            Assert.False(
                openai.ValueKind == JsonValueKind.Object
                    && openai.TryGetProperty("inputTransformations", out _)
            );
        }
        else
        {
            OpenAIUpstream.Equal(
                JsonNode.Parse(openai.GetProperty("inputTransformations").GetRawText()),
                transformations
            );
        }
    }
}

/// <summary>Port of <c>openai-responses-input-transformations.test.ts</c> &gt; <c>Responses input transformations (stream, failed response)</c>.</summary>
public sealed class ResponsesInputTransformationsFailedResponseTests
{
    private const string FailedResponsePrefix =
        "packages/openai/src/responses/openai-responses-input-transformations.test.ts::Responses input transformations (stream, failed response)::";

    [Fact]
    [UpstreamTest(
        FailedResponsePrefix
            + "returns the transformations of a response that fails after output starts",
        Coverage = UpstreamCoverage.Covered,
        Commit = "1a7440bcdea3a97a481dad5fea320f2accb703ae"
    )]
    public async Task Returns_the_transformations_of_a_response_that_fails_after_output_starts()
    {
        const string transformations =
            "[{\"id\":\"rs_1\",\"type\":\"reasoning\",\"action\":\"removed\",\"reason\":\"prefix_mismatch\"}]";
        var capture = new OpenAICapture
        {
            ServerSentEvents =
                "data: {\"type\":\"response.created\",\"response\":{\"id\":\"resp_test\",\"created_at\":0,\"model\":\"gpt-6.2-astra\"}}\n\n"
                + "data: {\"type\":\"response.output_item.added\",\"output_index\":0,\"item\":{\"id\":\"msg_1\",\"type\":\"message\"}}\n\n"
                + "data: {\"type\":\"response.failed\",\"sequence_number\":2,\"response\":{\"error\":{\"code\":\"server_error\",\"message\":\"failed\"},\"input_transformations\":"
                + transformations
                + "}}\n\n",
        };

        var model = OpenAIUpstream.Provider(capture).ResponsesModel("gpt-6.2-astra");
        var parts = new List<LanguageModelStreamPart>();
        await foreach (
            var part in await model.DoStreamAsync(
                new LanguageModelCallOptions
                {
                    Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
                },
                CancellationToken.None
            )
        )
        {
            parts.Add(part);
        }

        var metadata = parts.OfType<FinishStreamPart>().First().ProviderMetadata!.Value;
        OpenAIUpstream.Equal(
            JsonNode.Parse(
                metadata.GetProperty("openai").GetProperty("inputTransformations").GetRawText()
            ),
            transformations
        );
    }
}
