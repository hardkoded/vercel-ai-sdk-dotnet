// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; options.messages</c>.</summary>
[Collection("LogWarnings")]
public sealed class OptionsMessagesTests
{
    private const string Id = Prefix + "options.messages::";

    [Fact]
    [UpstreamTest(
        Id + "should support models that use \"this\" context in supportedUrls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_support_models_that_use_this_context_in_supportedUrls()
    {
        var supportedUrlsCalled = false;
        StreamObjectTestModel? model = null;
        model = Model(TextStream("{ \"content\": \"Hello, world!\" }"));
        model.OnSupportedUrls = () =>
        {
            supportedUrlsCalled = true;
            // Reference the model to verify context
            return model!.ModelId == "mock-model-id"
                ? new Dictionary<string, string> { ["image/*"] = "^https://.*$" }
                : new Dictionary<string, string>();
        };
        var options = ObjectOptions(model);
        options.Prompt = null;
        options.Messages =
        [
            new UserModelMessage([
                new FileContentPart("image/jpeg", "https://example.com/test.jpg", null, null),
            ]),
        ];

        var result = Client().StreamObjectAsync(options);

        var chunks = await Collect(result.TextStream());
        Assert.Equal("{ \"content\": \"Hello, world!\" }", string.Concat(chunks));
        Assert.True(supportedUrlsCalled);
    }
}
