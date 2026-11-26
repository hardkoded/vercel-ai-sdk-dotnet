// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Testing;
using Vercel.AI.Tests.Upstream.GenerateText;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>generate</c>, the sandbox and user-agent tests.</summary>
public sealed class GenerateTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate::should pass sandbox to prepareCall",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassSandboxToPrepareCall()
    {
        var sandbox = new TestSandboxSession();
        ISandboxSession? recordedSandbox = null;

        var agent = new Agent(
            new AgentOptions
            {
                Model = new TestLanguageModel(),
                PrepareCall = options =>
                {
                    recordedSandbox = options.ExperimentalSandbox;
                    return options;
                },
            },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync("Hello, world!", experimentalSandbox: sandbox);

        Assert.Same(sandbox, recordedSandbox);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate::should pass sandbox to tool execution",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassSandboxToToolExecution()
    {
        var sandbox = new TestSandboxSession();
        ISandboxSession? recordedSandbox = null;

        var agent = new Agent(
            new AgentOptions
            {
                Model = ToolLoopAgentToolCalls.GenerateModel(out _),
                Tools =
                [
                    ToolLoopAgentToolCalls.TestTool(
                        (value, options) =>
                        {
                            recordedSandbox = options.ExperimentalSandbox;
                            return value;
                        }
                    ),
                ],
            },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync("test", experimentalSandbox: sandbox);

        Assert.Same(sandbox, recordedSandbox);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate::should tag the user-agent with the agent identifier",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldTagTheUserAgentWithTheAgentIdentifier()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(
            new AgentOptions { Model = model },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync("Hello, world!");

        Assert.Contains(
            "ai-sdk-agent/tool-loop",
            Assert.Single(model.Calls).Headers!["user-agent"]
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate::should preserve a caller-provided user-agent",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPreserveACallerProvidedUserAgent()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(
            new AgentOptions
            {
                Model = model,
                Headers = new Dictionary<string, string?> { ["user-agent"] = "my-app/1.0" },
            },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync("Hello, world!");

        var ua = Assert.Single(model.Calls).Headers!["user-agent"];
        Assert.Contains("my-app/1.0", ua);
        Assert.Contains("ai-sdk-agent/tool-loop", ua);
    }

    private const string Schema =
        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}";

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate::should forward toolOrder to generateText",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldForwardToolOrderToGenerateText()
    {
        var model = new TestLanguageModel();

        var agent = new Agent(
            new AgentOptions
            {
                Model = model,
                Tools =
                [
                    Tool.Function("zebra", null, Schema, (_, _) => Task.FromResult("\"zebra\"")),
                    Tool.Function("alpha", null, Schema, (_, _) => Task.FromResult("\"alpha\"")),
                    Tool.Function("middle", null, Schema, (_, _) => Task.FromResult("\"middle\"")),
                ],
                ToolOrder = ["middle"],
            },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync("Hello, world!");

        Assert.Equal(
            new[] { "middle", "alpha", "zebra" },
            model.Calls[0].Tools!.Select(tool => tool.Name)
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate::should use prepareCall",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldUsePrepareCall()
    {
        var model = new TestLanguageModel();
        var agent = new Agent<PrepareCallOptions>(
            new AgentOptions<PrepareCallOptions>
            {
                Model = model,
                PrepareCall = (call, options) =>
                {
                    call.ProviderOptions = ToolLoopAgentInstructions.Options(options.Value);
                    return call;
                },
            },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync("Hello, world!", new PrepareCallOptions { Value = "test" });

        var options = Assert.IsAssignableFrom<IReadOnlyDictionary<string, JsonElement>>(
            Assert.Single(model.Calls).ProviderOptions
        );
        Assert.Equal("test", options["test"].GetProperty("value").GetString());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate::should pass abortSignal to generateText",
        Coverage = UpstreamCoverage.Covered,
        Note = "generate takes a CancellationToken, so the model call receives a token that the caller's token cancels."
    )]
    public async Task ShouldPassAbortSignalToGenerateText()
    {
        using var abortController = new CancellationTokenSource();
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text("reply") };
        var observed = new TokenRecordingLanguageModel(model);
        var agent = new Agent(
            new AgentOptions { Model = observed },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync("Hello, world!", abortController.Token);

        Assert.True(observed.Token.CanBeCanceled);
        Assert.False(observed.Token.IsCancellationRequested);
        abortController.Cancel();
        Assert.True(observed.Token.IsCancellationRequested);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate::should pass experimental_download to generateText",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassExperimentalDownloadToGenerateText()
    {
        var downloadCalls = new List<IReadOnlyList<DownloadRequestItem>>();
        var agent = new Agent(
            new AgentOptions
            {
                Model = new TestLanguageModel(),
                ExperimentalDownload = requested =>
                {
                    downloadCalls.Add(requested);
                    return Task.FromResult<IReadOnlyList<DownloadResult>>([
                        new DownloadResult([1, 2, 3], "image/png"),
                    ]);
                },
            },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync([
            new UserModelMessage([
                new FileContentPart("image/*", "https://example.com/image.png", null, null),
            ]),
        ]);

        var item = Assert.Single(Assert.Single(downloadCalls));
        Assert.Equal(new Uri("https://example.com/image.png"), item.Url);
        Assert.False(item.IsUrlSupportedByModel);
    }

    private sealed class PrepareCallOptions
    {
        public string Value { get; set; } = "";
    }

    private sealed class TokenRecordingLanguageModel(TestLanguageModel inner) : ILanguageModel
    {
        public CancellationToken Token { get; private set; }

        public string SpecificationVersion => inner.SpecificationVersion;

        public string Provider => inner.Provider;

        public string ModelId => inner.ModelId;

        public IReadOnlyDictionary<string, string> SupportedUrls => inner.SupportedUrls;

        public Task<LanguageModelGenerateResult> DoGenerateAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        )
        {
            Token = cancellationToken;
            return inner.DoGenerateAsync(options, cancellationToken);
        }

        public Task<LanguageModelStreamResult> DoStreamAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        ) => inner.DoStreamAsync(options, cancellationToken);
    }
}
