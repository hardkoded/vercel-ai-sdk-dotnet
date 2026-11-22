// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.Telemetry.CreateTelemetryDispatcher;

/// <summary>
/// Port of <c>create-telemetry-dispatcher.test.ts</c> &gt; <c>credential redaction</c>, for the step request and response bodies of the
/// <c>onEnd</c> event, the metadata of the SDK that the port exposes.
/// </summary>
public sealed class CredentialRedactionTests
{
    private const string Prefix =
        "packages/ai/src/telemetry/create-telemetry-dispatcher.test.ts::credential redaction::";

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    private static async Task<GenerateTextResult> End(
        Func<JsonElement, JsonElement> redact,
        JsonElement body
    )
    {
        GenerateTextResult? delivered = null;
        var usage = new LanguageModelUsage(1, 1, 2);
        var step = new StepResult(
            "text",
            null,
            [],
            [],
            FinishReason.Stop,
            usage,
            [],
            request: new LanguageModelRequestMetadata { Body = body, RedactBody = redact },
            response: new StepResponse("id", DateTimeOffset.UnixEpoch, "model", null, null, body)
            {
                RedactBody = redact,
            }
        );
        var result = new GenerateTextResult(
            "text",
            null,
            [step],
            FinishReason.Stop,
            usage,
            null,
            []
        );
        var options = new TelemetryOptions
        {
            Integrations =
            [
                new TestTelemetry
                {
                    OnEnd = (r, _) =>
                    {
                        delivered = (GenerateTextResult)r;
                        return Task.CompletedTask;
                    },
                },
            ],
        };
        await Vercel.AI.TelemetryDispatcher.Create(options).OnEnd!(result, CancellationToken.None);
        Assert.Contains("test-secret", result.FinalStep.Request.Body!.Value.GetRawText());
        return delivered!;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "applies provider-owned views throughout SDK metadata without mutating originals",
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public async Task Applies_provider_owned_views_throughout_SDK_metadata_without_mutating_originals()
    {
        var delivered = await End(
            _ => Json("""{"credential":"[redacted]","model":"test"}"""),
            Json("""{"credential":"test-secret","model":"test"}""")
        );

        Assert.Equal(
            """{"credential":"[redacted]","model":"test"}""",
            delivered.FinalStep.Request.Body!.Value.GetRawText()
        );
        Assert.Equal(
            """{"credential":"[redacted]","model":"test"}""",
            delivered.FinalStep.Response.Body!.Value.GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "omits an annotated value if its redactor throws",
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public async Task Omits_an_annotated_value_if_its_redactor_throws()
    {
        var delivered = await End(
            _ => throw new InvalidOperationException("broken redactor"),
            Json("""{"credential":"test-secret"}""")
        );

        Assert.Null(delivered.FinalStep.Request.Body);
        Assert.Null(delivered.FinalStep.Response.Body);
    }
}
