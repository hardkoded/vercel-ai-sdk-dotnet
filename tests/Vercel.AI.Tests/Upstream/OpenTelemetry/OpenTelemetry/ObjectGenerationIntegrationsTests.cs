// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;
using Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;
using StreamSupport = Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>object generation integrations</c>.</summary>
public sealed class ObjectGenerationIntegrationsTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > object generation integrations::";

    private const string ExpectedInput =
        "[{\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello\"}]}]";
    private const string ExpectedSystem = "[{\"type\":\"text\",\"content\":\"Return a greeting\"}]";

    [Fact]
    [UpstreamTest(
        Test + "records generateObject instructions on the chat span",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Records_generateObject_instructions_on_the_chat_span()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        var options = GenerateObjectTestSupport.ObjectOptions(
            new GenerateObjectTestModel { Provider = "openai.chat", ModelId = "gpt-4" }
        );
        options.Instructions = "Return a greeting";
        options.Prompt = "Hello";
        options.Telemetry = new TelemetryOptions { Integrations = [integration] };

        await GenerateObjectTestSupport.Client().GenerateObjectAsync(options);

        var chatSpan = Assert.Single(tracer.Spans, s => s.Name == "chat gpt-4");
        Assert.Equal(ExpectedInput, chatSpan.Attributes["gen_ai.input.messages"]);
        Assert.Equal(ExpectedSystem, chatSpan.Attributes["gen_ai.system_instructions"]);
    }

    [Fact]
    [UpstreamTest(
        Test + "records streamObject instructions on the chat span",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Records_streamObject_instructions_on_the_chat_span()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        var parts = StreamSupport.TextStream("{\"content\":\"Hello\"}");
        var options = StreamSupport.ObjectOptions(
            new StreamObjectTestModel
            {
                Provider = "openai.chat",
                ModelId = "gpt-4",
                OnStream = _ => Task.FromResult(StreamSupport.Result(parts)),
            }
        );
        options.Instructions = "Return a greeting";
        options.Prompt = "Hello";
        options.Telemetry = new TelemetryOptions { Integrations = [integration] };

        await StreamSupport.Collect(
            GenerateObjectTestSupport.Client().StreamObjectAsync(options).PartialObjectStream()
        );

        var chatSpan = Assert.Single(tracer.Spans, s => s.Name == "chat gpt-4");
        Assert.Equal(ExpectedInput, chatSpan.Attributes["gen_ai.input.messages"]);
        Assert.Equal(ExpectedSystem, chatSpan.Attributes["gen_ai.system_instructions"]);
    }
}
