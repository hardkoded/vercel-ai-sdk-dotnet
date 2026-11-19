// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; output = "object" &gt; error handling</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectErrorHandlingTests
{
    private const string Id = Prefix + "output = \"object\" > error handling::";

    private static async Task AssertAllReject(Exception error, params Task[] promises)
    {
        foreach (var promise in promises)
        {
            var thrown = await Assert.ThrowsAnyAsync<Exception>(() => promise);
            Assert.Same(error, thrown);
        }
    }

    private static void VerifyNoObjectGeneratedError(Exception? error, string message)
    {
        var noObject = Assert.IsType<NoObjectGeneratedException>(error);
        Assert.True(NoObjectGeneratedException.IsInstance(noObject));
        Assert.Equal(message, noObject.Message);
        Assert.Equal("id-1", noObject.Response.Id);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(123), noObject.Response.Timestamp);
        Assert.Equal("model-1", noObject.Response.ModelId);
        Assert.Null(noObject.Response.Headers);
        Assert.Equal(3, noObject.Usage.InputTokens);
        Assert.Equal(3, noObject.Usage.NoCacheInputTokens);
        Assert.Equal(10, noObject.Usage.OutputTokens);
        Assert.Equal(10, noObject.Usage.TextTokens);
        Assert.Equal(13, noObject.Usage.TotalTokens);
        Assert.Equal(FinishReason.Stop, noObject.FinishReason);
    }

    private static LanguageModelStreamPart Metadata() =>
        new ResponseMetadataStreamPart(
            "id-1",
            "model-1",
            DateTimeOffset.FromUnixTimeMilliseconds(123)
        );

    [Fact]
    [UpstreamTest(
        Id + "should reject pending result promises when doStream throws",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_pending_result_promises_when_doStream_throws()
    {
        var error = new InvalidOperationException("test error");
        var options = ObjectOptions(ThrowingModel(error));
        options.OnError = (_, _) => Task.CompletedTask;

        var result = Client().StreamObjectAsync(options);

        await AssertAllReject(
            error,
            result.Object,
            result.Usage,
            result.ProviderMetadata,
            result.Warnings,
            result.Request,
            result.Response,
            result.FinishReason
        );
    }

    [Fact]
    [UpstreamTest(
        Id
            + "should not emit an unhandled rejection when a result promise is awaited after failure",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_emit_an_unhandled_rejection_when_a_result_promise_is_awaited_after_failure()
    {
        var error = new InvalidOperationException("test error");
        var unobserved = new List<Exception>();
        void OnUnobserved(object? sender, UnobservedTaskExceptionEventArgs args)
        {
            // other tests share the process: only this test's error counts
            if (args.Exception.Flatten().InnerExceptions.Any(e => ReferenceEquals(e, error)))
            {
                unobserved.Add(args.Exception);
            }
        }
        TaskScheduler.UnobservedTaskException += OnUnobserved;
        try
        {
            var options = ObjectOptions(ThrowingModel(error));
            options.OnError = (_, _) => Task.CompletedTask;
            var result = Client().StreamObjectAsync(options);
            var objectPromise = result.Object;

            await Collect(result.FullStream());
            await Task.Delay(50);

            var thrown = await Assert.ThrowsAnyAsync<Exception>(() => objectPromise);
            Assert.Same(error, thrown);

            result = null!;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Assert.Empty(unobserved);
        }
        finally
        {
            TaskScheduler.UnobservedTaskException -= OnUnobserved;
        }
    }

    [Fact]
    [UpstreamTest(
        Id + "should reject pending result promises and report failure for an error stream part",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_pending_result_promises_and_report_failure_for_an_error_stream_part()
    {
        var error = new InvalidOperationException("test error");
        var errors = new List<Exception>();
        GenerateObjectStepEndEvent? stepEnd = null;
        GenerateObjectEndEvent<ContentResult>? end = null;
        var options = ObjectOptions(Model([new ErrorStreamPart(error)]));
        options.OnError = (e, _) =>
        {
            errors.Add(e);
            return Task.CompletedTask;
        };
        options.OnStepEnd = (e, _) =>
        {
            stepEnd = e;
            return Task.CompletedTask;
        };
        options.OnFinish = (e, _) =>
        {
            end = e;
            return Task.CompletedTask;
        };
        var result = Client().StreamObjectAsync(options);

        var parts = await Collect(result.FullStream());

        var part = Assert.IsType<ObjectStreamErrorPart>(Assert.Single(parts));
        Assert.Same(error, part.Error);
        Assert.Same(error, Assert.Single(errors));
        Assert.Equal(FinishReason.Error, stepEnd!.FinishReason);
        Assert.Same(LanguageModelUsage.Empty, stepEnd.Usage);
        Assert.Null(end!.Object);
        Assert.Same(error, end.Error);
        Assert.Equal(FinishReason.Error, end.FinishReason);
        Assert.Same(LanguageModelUsage.Empty, end.Usage);

        await AssertAllReject(
            error,
            result.Object,
            result.Usage,
            result.ProviderMetadata,
            result.Warnings,
            result.Response,
            result.FinishReason
        );
        Assert.Null((await result.Request).Body);
    }

    [Fact]
    [UpstreamTest(
        Id + "should preserve error parts and finish callbacks when onError throws",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_error_parts_and_finish_callbacks_when_onError_throws()
    {
        var error = new InvalidOperationException("provider error");
        var stepEnds = 0;
        var finishes = 0;
        var options = ObjectOptions(Model([new ErrorStreamPart(error)]));
        options.OnError = (_, _) => throw new InvalidOperationException("callback error");
        options.OnStepEnd = (_, _) =>
        {
            stepEnds++;
            return Task.CompletedTask;
        };
        options.OnFinish = (_, _) =>
        {
            finishes++;
            return Task.CompletedTask;
        };
        var result = Client().StreamObjectAsync(options);

        var parts = await Collect(result.FullStream());

        Assert.Same(error, Assert.IsType<ObjectStreamErrorPart>(Assert.Single(parts)).Error);
        Assert.Equal(1, stepEnds);
        Assert.Equal(1, finishes);
    }

    [Fact]
    [UpstreamTest(
        Id + "should preserve raw stream errors when onError throws",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_raw_stream_errors_when_onError_throws()
    {
        var error = new InvalidOperationException("test error");
        var errors = new List<Exception>();
        var model = new StreamObjectTestModel
        {
            OnStream = _ => Task.FromResult(new LanguageModelStreamResult(Failing(error))),
        };
        var options = ObjectOptions(model);
        options.OnError = (e, _) =>
        {
            errors.Add(e);
            throw new InvalidOperationException("callback error");
        };
        var result = Client().StreamObjectAsync(options);

        var thrown = await Assert.ThrowsAnyAsync<Exception>(() => Collect(result.FullStream()));
        Assert.Same(error, thrown);
        Assert.Same(error, Assert.Single(errors));

        await AssertAllReject(
            error,
            result.Object,
            result.Usage,
            result.ProviderMetadata,
            result.Warnings,
            result.Response,
            result.FinishReason
        );
        Assert.Null((await result.Request).Body);
    }

    [Fact]
    [UpstreamTest(
        Id + "should throw NoObjectGeneratedError when schema validation fails",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_NoObjectGeneratedError_when_schema_validation_fails()
    {
        var result = Client()
            .StreamObjectAsync(
                ObjectOptions(
                    Model([
                        new TextStartStreamPart("1"),
                        Delta("{ \"content\": 123 }"),
                        new TextEndStreamPart("1"),
                        Metadata(),
                        Finish(),
                    ])
                )
            );

        var error = await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await Collect(result.PartialObjectStream());
            await result.Object;
        });

        VerifyNoObjectGeneratedError(error, "No object generated: response did not match schema.");
    }

    [Fact]
    [UpstreamTest(
        Id + "should throw NoObjectGeneratedError when parsing fails",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_NoObjectGeneratedError_when_parsing_fails()
    {
        var result = Client()
            .StreamObjectAsync(
                ObjectOptions(
                    Model([
                        new TextStartStreamPart("1"),
                        Delta("{ broken json"),
                        new TextEndStreamPart("1"),
                        Metadata(),
                        Finish(),
                    ])
                )
            );

        var error = await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await Collect(result.PartialObjectStream());
            await result.Object;
        });

        VerifyNoObjectGeneratedError(error, "No object generated: could not parse the response.");
    }

    [Fact]
    [UpstreamTest(
        Id + "should throw NoObjectGeneratedError when no text is generated",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_NoObjectGeneratedError_when_no_text_is_generated()
    {
        var result = Client().StreamObjectAsync(ObjectOptions(Model([Metadata(), Finish()])));

        var error = await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await Collect(result.PartialObjectStream());
            await result.Object;
        });

        VerifyNoObjectGeneratedError(error, "No object generated: could not parse the response.");
    }
}
