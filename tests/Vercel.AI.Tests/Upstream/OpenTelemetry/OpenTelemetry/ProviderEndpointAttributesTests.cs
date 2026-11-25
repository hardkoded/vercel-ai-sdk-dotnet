// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateText;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>provider endpoint attributes</c>.</summary>
public sealed class ProviderEndpointAttributesTests
{
    private const string UpstreamCommit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e";
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry::provider endpoint attributes::";
    private const string Url =
        "https://user:password@model.example.test:8443/private?key=secret#fragment";

    private const string ObjectSchema =
        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}";

    private sealed class ObjectValue
    {
        public string Value { get; set; } = string.Empty;
    }

    private static readonly LanguageModelUsage Usage = new(2, 1, 3);

    private static AiClient Client() =>
        new(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));

    private static StepErrorModel Model(string? url) =>
        new(
            doGenerate: (_, _) =>
                Task.FromResult(
                    new LanguageModelGenerateResult(
                        [new GeneratedText("{\"value\":\"hello\"}")],
                        FinishReason.Stop,
                        Usage,
                        request: url is null ? null : new LanguageModelStreamRequest { Url = url }
                    )
                ),
            doStream: (_, _) =>
                Task.FromResult(
                    new LanguageModelStreamResult(
                        Parts(),
                        request: url is null ? null : new LanguageModelStreamRequest { Url = url }
                    )
                )
        );

    private static async IAsyncEnumerable<LanguageModelStreamPart> Parts()
    {
        yield return new TextStartStreamPart("text");
        yield return new TextDeltaStreamPart("text", "{\"value\":\"hello\"}");
        yield return new TextEndStreamPart("text");
        yield return new FinishStreamPart(FinishReason.Stop, Usage);
        await Task.CompletedTask;
    }

    [Theory]
    [InlineData("generate")]
    [InlineData("stream")]
    [InlineData("generateObject")]
    [InlineData("streamObject")]
    [UpstreamTest(
        Test + "records the endpoint reported by a %s call without URL secrets",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Records_the_endpoint_reported_by_a_call_without_URL_secrets(string mode)
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        var ends = new List<LanguageModelCallEndEvent>();
        var telemetry = new TelemetryOptions
        {
            Integrations = [integration],
            RecordInputs = false,
            RecordOutputs = false,
        };

        if (mode == "generate")
        {
            await Client()
                .GenerateTextAsync(
                    new GenerateTextOptions
                    {
                        Model = Model(Url),
                        Prompt = "hello",
                        Telemetry = telemetry,
                        OnLanguageModelCallEnd = (e, _) =>
                        {
                            ends.Add(e);
                            return Task.CompletedTask;
                        },
                    }
                );
        }
        else if (mode == "generateObject")
        {
            await Client()
                .GenerateObjectAsync(
                    new GenerateObjectOptions<ObjectValue>
                    {
                        Model = Model(Url),
                        Prompt = "hello",
                        Schema = ObjectSchema,
                        Telemetry = telemetry,
                    }
                );
        }
        else if (mode == "streamObject")
        {
            await foreach (
                var _ in Client()
                    .StreamObjectAsync(
                        new StreamObjectOptions<ObjectValue>
                        {
                            Model = Model(Url),
                            Prompt = "hello",
                            Schema = ObjectSchema,
                            Telemetry = telemetry,
                        }
                    )
                    .PartialObjectStream()
            ) { }
        }
        else
        {
            var result = Client()
                .StreamTextAsync(
                    new StreamTextOptions
                    {
                        Model = Model(Url),
                        Prompt = "hello",
                        Telemetry = telemetry,
                        OnLanguageModelCallEnd = (e, _) =>
                        {
                            ends.Add(e);
                            return Task.CompletedTask;
                        },
                    }
                );
            await result.ConsumeStreamAsync();
        }

        var span = Assert.Single(tracer.Spans, s => s.Name == "chat mock-model-id");
        Assert.Equal("model.example.test", span.Attributes["server.address"]);
        Assert.Equal(8443, span.Attributes["server.port"]);
        Assert.DoesNotMatch(
            "password|private|secret|fragment",
            string.Join(",", span.Attributes.Select(pair => pair.Key + "=" + pair.Value))
        );
        if (mode is "generate" or "stream")
        {
            Assert.Equal("https://model.example.test:8443", Assert.Single(ends).Request?.Url);
        }
    }

    [Fact]
    [UpstreamTest(
        Test + "does not guess an endpoint from a provider name",
        Coverage = UpstreamCoverage.Covered,
        Commit = UpstreamCommit
    )]
    public async Task Does_not_guess_an_endpoint_from_a_provider_name()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = Model(null),
                    Prompt = "hello",
                    Telemetry = new TelemetryOptions { Integrations = [integration] },
                }
            );

        var span = Assert.Single(tracer.Spans, s => s.Name == "chat mock-model-id");
        Assert.False(span.Attributes.ContainsKey("server.address"));
        Assert.False(span.Attributes.ContainsKey("server.port"));
    }
}
