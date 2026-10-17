// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

// C# has no test-d.ts. Each method compiles the property use that the upstream type test checks.
// The catalog lists only *.test.ts files, so these methods carry no UpstreamTest attribute.
public sealed class ToolCallConcurrencyTypesTests
{
    // generate-text.test-d.ts > generateText types > should accept toolCallConcurrency
    [Fact]
    public void Should_accept_toolCallConcurrency_on_GenerateTextOptions()
    {
        var options = new GenerateTextOptions { Model = new TestLanguageModel(), Prompt = "Hello", ToolCallConcurrency = 1 };

        Assert.Equal(1, options.ToolCallConcurrency);
    }

    // stream-text.test-d.ts > streamText types > should accept toolCallConcurrency
    [Fact]
    public void Should_accept_toolCallConcurrency_on_StreamTextOptions()
    {
        var options = new StreamTextOptions { Model = new TestLanguageModel(), Prompt = "Hello", ToolCallConcurrency = 1 };

        Assert.Equal(1, options.ToolCallConcurrency);
    }

    // tool-loop-agent.test-d.ts > ToolLoopAgent > generate > should accept toolCallConcurrency in settings and prepareCall
    [Fact]
    public void Should_accept_toolCallConcurrency_in_settings_and_prepareCall()
    {
        var agent = new Agent(new AgentOptions
        {
            Model = new TestLanguageModel(),
            ToolCallConcurrency = 1,
            PrepareCall = options =>
            {
                options.ToolCallConcurrency = 2;
                return options;
            },
        });

        Assert.Equal(1, agent.Options.ToolCallConcurrency);
    }
}
