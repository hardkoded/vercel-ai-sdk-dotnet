// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.GenerateObject;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject.GenerateObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; options.messages</c>.</summary>
[Collection("LogWarnings")]
public sealed class OptionsMessagesTests
{
    private const string Id =
        "packages/ai/src/generate-object/generate-object.test.ts::generateObject > options.messages::";

    private sealed class ThisContextModel : GenerateObjectTestModel
    {
        public bool SupportedUrlsCalled { get; private set; }

        // References instance state to verify the property is read on the model.
        public override IReadOnlyDictionary<string, string> SupportedUrls
        {
            get
            {
                SupportedUrlsCalled = true;
                return ModelId == "mock-model-id"
                    ? new Dictionary<string, string> { ["image/*"] = "^https://.*$" }
                    : new Dictionary<string, string>();
            }
        }
    }

    [Fact]
    [UpstreamTest(
        Id + "should support models that use \"this\" context in supportedUrls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_support_models_that_use_this_context_in_supportedUrls()
    {
        var model = new ThisContextModel();
        var options = ObjectOptions(model);
        options.Prompt = null;
        options.Messages =
        [
            new UserModelMessage([new ImageUrlContentPart("https://example.com/test.jpg")]),
        ];

        var result = await Client().GenerateObjectAsync(options);

        Assert.Equal("Hello, world!", result.Object.Content);
        Assert.True(model.SupportedUrlsCalled);
    }
}
