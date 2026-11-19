// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; output = "object" &gt; result.objectStream</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectResultObjectStreamTests
{
    private const string Id = Prefix + "output = \"object\" > result.objectStream::";

    private static readonly string[] Deltas =
    [
        "{}",
        "{\"content\":\"Hello, \"}",
        "{\"content\":\"Hello, world\"}",
        "{\"content\":\"Hello, world!\"}",
    ];

    [Fact]
    [UpstreamTest(Id + "should send object deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_object_deltas()
    {
        var model = CreateTestModel();
        StreamObjectResult<ContentResult>? result = null;
        var logged = await WithLogWarnings(() =>
        {
            result = Client().StreamObjectAsync(ObjectOptions(model));
            return Task.CompletedTask;
        });

        var first = Assert.Single(logged[0].Warnings);
        var deprecated = Assert.IsType<DeprecatedWarning>(first);
        Assert.Equal("streamObject", deprecated.Setting);
        Assert.Equal("Use streamText with an output setting instead.", deprecated.Message);
        Assert.Null(logged[0].Provider);

        Assert.Equal(Deltas, await CollectRaw(result!.PartialObjectStream()));

        var call = model.Calls[0];
        Assert.Equal(ContentSchema, call.JsonSchema!.Value.GetRawText());
        Assert.Null(call.JsonSchemaName);
        Assert.Null(call.JsonSchemaDescription);
    }

    [Fact]
    [UpstreamTest(Id + "should use name and description", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_name_and_description()
    {
        var model = CreateTestModel();
        var options = ObjectOptions(model);
        options.SchemaName = "test-name";
        options.SchemaDescription = "test description";

        var result = Client().StreamObjectAsync(options);

        Assert.Equal(Deltas, await CollectRaw(result.PartialObjectStream()));
        var prompt = Assert.IsType<UserModelMessage>(Assert.Single(model.Calls[0].Prompt));
        Assert.Equal("prompt", Assert.IsType<TextContentPart>(Assert.Single(prompt.Content)).Text);
        Assert.Equal(ContentSchema, model.Calls[0].JsonSchema!.Value.GetRawText());
        Assert.Equal("test-name", model.Calls[0].JsonSchemaName);
        Assert.Equal("test description", model.Calls[0].JsonSchemaDescription);
    }

    [Fact]
    [UpstreamTest(
        Id + "should suppress error in partialObjectStream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_suppress_error_in_partialObjectStream()
    {
        var options = ObjectOptions(ThrowingModel(new InvalidOperationException("test error")));
        options.OnError = (_, _) => Task.CompletedTask;

        var result = Client().StreamObjectAsync(options);

        Assert.Empty(await Collect(result.PartialObjectStream()));
    }

    [Fact]
    [UpstreamTest(
        Id + "should invoke onError callback with Error",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_invoke_onError_callback_with_Error()
    {
        var errors = new List<Exception>();
        var options = ObjectOptions(ThrowingModel(new InvalidOperationException("test error")));
        options.OnError = (error, _) =>
        {
            errors.Add(error);
            return Task.CompletedTask;
        };

        var result = Client().StreamObjectAsync(options);

        // consume stream
        await Collect(result.PartialObjectStream());

        var error = Assert.Single(errors);
        Assert.IsType<InvalidOperationException>(error);
        Assert.Equal("test error", error.Message);
    }
}
