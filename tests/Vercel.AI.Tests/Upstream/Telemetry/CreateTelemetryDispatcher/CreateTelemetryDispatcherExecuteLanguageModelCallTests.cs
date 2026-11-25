// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Telemetry.CreateTelemetryDispatcher;

/// <summary>Port of <c>create-telemetry-dispatcher.test.ts</c> &gt; <c>createTelemetryDispatcher &gt; executeLanguageModelCall</c>.</summary>
[Collection("TelemetryRegistry")]
public sealed class CreateTelemetryDispatcherExecuteLanguageModelCallTests
{
    private const string Id =
        "packages/ai/src/telemetry/create-telemetry-dispatcher.test.ts::createTelemetryDispatcher > executeLanguageModelCall::";

    [Fact]
    [UpstreamTest(Id + "passes through when no integrations implement executeLanguageModelCall")]
    public async Task Passes_through_when_no_integrations_implement_executeLanguageModelCall()
    {
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            CreateTelemetryDispatcherTests.Options(
                new TestTelemetry { OnStart = (_, _) => Task.CompletedTask }
            )
        );

        Assert.Equal(
            "result",
            await telemetry.ExecuteLanguageModelCallAsync(
                new LanguageModelCallStartEvent { CallId = "call-1" },
                () => Task.FromResult("result"),
                CancellationToken.None
            )
        );
    }

    [Fact]
    [UpstreamTest(Id + "wraps execute with a single integration")]
    public async Task Wraps_execute_with_a_single_integration()
    {
        var executed = 0;
        var wrapper = new Wrapper();
        var telemetry = Vercel.AI.TelemetryDispatcher.Create(
            CreateTelemetryDispatcherTests.Options(
                new TestTelemetry { ExecuteLanguageModelCall = wrapper }
            )
        );

        var result = await telemetry.ExecuteLanguageModelCallAsync(
            new LanguageModelCallStartEvent { CallId = "call-1" },
            () =>
            {
                executed++;
                return Task.FromResult("result");
            },
            CancellationToken.None
        );

        Assert.Equal("wrapped:result", result);
        Assert.Equal(1, wrapper.Calls);
        Assert.Equal(Vercel.AI.Operations.AiSdkVersion.Version, wrapper.Version);
        Assert.Equal("call-1", wrapper.CallId);
        Assert.Equal(1, executed);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [UpstreamTest(
        Id + "preserves stream replacements through %s redacted integrations",
        Coverage = UpstreamCoverage.Covered,
        Commit = "bd0f9e4ebda0d23a849e37860e2a1aebac9a9eef"
    )]
    public async Task Preserves_stream_replacements_through_redacted_integrations(
        int integrationCount
    )
    {
        var original = new LanguageModelStreamResult(
            Single("hello"),
            new LanguageModelStreamRequest(Credential("test-token"))
            {
                RedactBody = body => Credential("[redacted]"),
            }
        );
        var observed = new List<LanguageModelStreamResult>();
        var integration = new StreamReplacingWrapper(observed);
        var dispatcher = Vercel.AI.TelemetryDispatcher.Create(
            CreateTelemetryDispatcherTests.Options([
                .. Enumerable.Repeat<ITelemetry>(
                    new TestTelemetry { ExecuteLanguageModelCall = integration },
                    integrationCount
                ),
            ])
        );

        var result = await dispatcher.ExecuteLanguageModelCallAsync(
            new LanguageModelCallStartEvent { CallId = "call-1" },
            () => Task.FromResult(original),
            CancellationToken.None
        );

        Assert.Same(original, result);
        Assert.Equal(
            "test-token",
            result.Request!.Body!.Value.GetProperty("credential").GetString()
        );
        Assert.Equal(integrationCount, observed.Count);
        foreach (var value in observed)
        {
            Assert.Equal(
                "[redacted]",
                value.Request!.Body!.Value.GetProperty("credential").GetString()
            );
        }

        var parts = new List<string>();
        await foreach (var part in result.Stream)
        {
            parts.Add(((TextDeltaStreamPart)part).Delta);
        }

        Assert.Equal(["hello" + new string('!', integrationCount)], parts);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "redacts provider error metadata for nested integrations and preserves the application error",
        Coverage = UpstreamCoverage.Covered,
        Commit = "bd0f9e4ebda0d23a849e37860e2a1aebac9a9eef"
    )]
    public async Task Redacts_provider_error_metadata_for_nested_integrations_and_preserves_the_application_error()
    {
        var error = new ApiCallError(
            "provider failed",
            "https://example.test",
            TelemetryRedaction.WithTelemetryRedaction(Credential("test-token"), body =>
                Credential("[redacted]")
            )
        );
        var observed = new List<Exception>();
        var integration = new ErrorObservingWrapper(observed);
        var dispatcher = Vercel.AI.TelemetryDispatcher.Create(
            CreateTelemetryDispatcherTests.Options(
                new TestTelemetry { ExecuteLanguageModelCall = integration },
                new TestTelemetry { ExecuteLanguageModelCall = integration }
            )
        );

        var thrown = await Assert.ThrowsAsync<ApiCallError>(() =>
            dispatcher.ExecuteLanguageModelCallAsync<string>(
                new LanguageModelCallStartEvent { CallId = "call-1" },
                () => throw error,
                CancellationToken.None
            )
        );

        Assert.Same(error, thrown);
        Assert.Equal(2, observed.Count);
        foreach (var observedError in observed)
        {
            var api = Assert.IsType<ApiCallError>(observedError);
            Assert.Equal("provider failed", api.Message);
            var body = Assert.IsType<JsonElement>(api.RequestBodyValues);
            Assert.Equal("[redacted]", body.GetProperty("credential").GetString());
        }

        var original = Assert.IsType<TelemetryRedactedValue>(error.RequestBodyValues);
        Assert.Equal("test-token", original.Value.GetProperty("credential").GetString());
    }

    private static JsonElement Credential(string value) =>
        JsonDocument.Parse("{\"credential\":\"" + value + "\"}").RootElement.Clone();

    private static async IAsyncEnumerable<LanguageModelStreamPart> Single(string text)
    {
        yield return new TextDeltaStreamPart("0", text);
        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> Append(
        IAsyncEnumerable<LanguageModelStreamPart> source
    )
    {
        await foreach (var part in source)
        {
            yield return new TextDeltaStreamPart("0", ((TextDeltaStreamPart)part).Delta + "!");
        }
    }

    private sealed class StreamReplacingWrapper(List<LanguageModelStreamResult> observed)
        : IExecuteLanguageModelCall
    {
        public async Task<T> ExecuteAsync<T>(
            ExecuteLanguageModelCallOptions<T> options,
            CancellationToken cancellationToken
        )
        {
            var result = await options.Execute();
            if (result is LanguageModelStreamResult stream)
            {
                observed.Add(stream);
                stream.Stream = Append(stream.Stream);
            }

            return result;
        }
    }

    private sealed class ErrorObservingWrapper(List<Exception> observed) : IExecuteLanguageModelCall
    {
        public async Task<T> ExecuteAsync<T>(
            ExecuteLanguageModelCallOptions<T> options,
            CancellationToken cancellationToken
        )
        {
            try
            {
                return await options.Execute();
            }
            catch (Exception error)
            {
                observed.Add(error);
                throw;
            }
        }
    }

    private sealed class Wrapper : IExecuteLanguageModelCall
    {
        public int Calls { get; private set; }

        public string? Version { get; private set; }

        public string? CallId { get; private set; }

        public async Task<T> ExecuteAsync<T>(
            ExecuteLanguageModelCallOptions<T> options,
            CancellationToken cancellationToken
        )
        {
            Calls++;
            Version = options.Event.AiVersion;
            CallId = options.CallId;
            return (T)(object)("wrapped:" + await options.Execute());
        }
    }
}
