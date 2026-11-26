// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Telemetry.TracingChannel;

/// <summary>Port of <c>tracing-channel.test.ts</c> &gt; <c>streamText request metadata</c>.</summary>
public sealed class StreamTextRequestMetadataTests
{
    private const string Prefix =
        "packages/ai/src/telemetry/tracing-channel.test.ts::telemetry tracing channel publisher::streamText request metadata::";

    public static TheoryData<string?, string?, bool> SanitizeCases =>
        RequestMetadataSupport.SanitizeCases;

    [Fact]
    [UpstreamTest(
        Prefix + "applies OpenAI redaction through middleware with HTTP %s",
        Coverage = UpstreamCoverage.Partial,
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e",
        Note = "Covers HTTP 200 only. The HTTP 503 case expects a RetryError with two attempts, and the port has no model call retry in generateText and streamText."
    )]
    public Task Applies_OpenAI_redaction_through_middleware_with_HTTP_200() =>
        RequestMetadataSupport.ApplyOpenAIRedactionThroughMiddleware(true);

    [Theory]
    [MemberData(nameof(SanitizeCases))]
    [UpstreamTest(
        Prefix + "sanitizes $request before telemetry observes the provider result",
        Coverage = UpstreamCoverage.Covered,
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public Task Sanitizes_request_before_telemetry_observes_the_provider_result(
        string? url,
        string? expectedUrl,
        bool hasRequest
    ) =>
        RequestMetadataSupport.SanitizeRequestBeforeTelemetryObservesTheProviderResult(
            true,
            "request-metadata-streamText-" + Guid.NewGuid(),
            url,
            expectedUrl,
            hasRequest
        );
}
