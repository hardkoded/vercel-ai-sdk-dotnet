// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Perplexity;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class PerplexityLiveTests
{
    [SkippableFact]
    public async Task GeneratesAGroundedAnswerWithAPreset()
    {
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PERPLEXITY_API_KEY")));
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = Model("fast"),
            Prompt = "Find the official TypeScript website and describe TypeScript in one sentence with a citation.",
            MaxOutputTokens = 1024,
        });

        Assert.False(string.IsNullOrWhiteSpace(result.Text));
        Assert.NotEmpty(result.Sources);
        Assert.True(result.Usage.TotalTokens > 0);
        Assert.True(result.ProviderMetadata.HasValue);
        Assert.NotEqual(JsonValueKind.Null, result.ProviderMetadata.Value.GetProperty("perplexity").GetProperty("cost").ValueKind);
    }

    [SkippableFact]
    public async Task StreamsAGroundedAnswerWithUniqueSources()
    {
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PERPLEXITY_API_KEY")));
        var result = Client().StreamTextAsync(new StreamTextOptions
        {
            Model = Model("fast"),
            Prompt = "Find the official TypeScript website and describe TypeScript in one sentence with a citation.",
            MaxOutputTokens = 1024,
        });

        Assert.False(string.IsNullOrWhiteSpace(await result.Text));
        Assert.Equal(FinishReason.Stop, await result.FinishReason);
        var sources = new List<GeneratedSource>();
        foreach (var step in await result.Steps)
        {
            sources.AddRange(step.Sources);
        }

        Assert.NotEmpty(sources);
        Assert.Equal(sources.Count, sources.Select(source => source.Url).Distinct().Count());
        Assert.True((await result.Usage).TotalTokens > 0);
    }

    [SkippableFact]
    public async Task AcceptsADirectAgentApiModelId()
    {
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PERPLEXITY_API_KEY")));
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = Model("perplexity/sonar"),
            Prompt = "What is 2 + 2? Answer briefly.",
            MaxOutputTokens = 128,
        });

        Assert.Contains("4", result.Text);
    }

    [SkippableFact]
    public async Task GeneratesStructuredOutput()
    {
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PERPLEXITY_API_KEY")));
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = Model("low"),
            Prompt = "Return the city Paris and country France.",
            Output = OutputSpec.Object("{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"},\"country\":{\"type\":\"string\"}},\"required\":[\"city\",\"country\"],\"additionalProperties\":false}"),
            MaxOutputTokens = 512,
        });

        Assert.Equal("Paris", result.Output!.Value.GetProperty("city").GetString());
        Assert.Equal("France", result.Output.Value.GetProperty("country").GetString());
    }

    [SkippableFact]
    public async Task ContinuesAfterAClientFunctionCallWithGenerate()
    {
        await ContinueAfterFunctionCall(stream: false);
    }

    [SkippableFact]
    public async Task ContinuesAfterAClientFunctionCallWithStream()
    {
        await ContinueAfterFunctionCall(stream: true);
    }

    [SkippableFact]
    public async Task AcceptsInlineImageInput()
    {
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PERPLEXITY_API_KEY")));
        var bytes = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", "comic-cat.png"));
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = Model("low"),
            Messages = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[]
                {
                    new TextContentPart("Name the animal in this image in one word."),
                    new FileContentPart("image/png", null, bytes, "comic-cat.png"),
                }),
            },
            MaxOutputTokens = 256,
        });

        Assert.Contains("cat", result.Text.ToLowerInvariant());
    }

    [SkippableFact]
    public async Task ReportsAnInvalidModelAsAnApiError()
    {
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PERPLEXITY_API_KEY")));
        var exception = await Assert.ThrowsAsync<ApiException>(() => Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = Model("no-such-model"),
            Prompt = "This should fail",
        }));

        Assert.InRange(exception.StatusCode, 400, 499);
    }

    private static async Task ContinueAfterFunctionCall(bool stream)
    {
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PERPLEXITY_API_KEY")));
        var calls = 0;
        var options = new GenerateTextOptions
        {
            Model = Model("low"),
            Prompt = "Call lookupToken to get the token, then report the token exactly. Do not search the web.",
            Tools = new[]
            {
                Tool.Function(
                    "lookupToken",
                    "Retrieve the token. Only this tool knows it.",
                    "{\"type\":\"object\",\"properties\":{}}",
                    (args, cancellationToken) =>
                    {
                        calls++;
                        return Task.FromResult("\"opal-7291\"");
                    }),
            },
            StopWhen = StopWhen.IsStepCount(3),
            MaxOutputTokens = 1024,
        };

        if (stream)
        {
            var streamed = Client().StreamTextAsync(new StreamTextOptions
            {
                Model = options.Model,
                Prompt = options.Prompt,
                Tools = options.Tools,
                StopWhen = options.StopWhen,
                MaxOutputTokens = options.MaxOutputTokens,
            });
            Assert.Contains("opal-7291", await streamed.Text);
            Assert.True(calls > 0);
            Assert.True((await streamed.Steps).Count > 1);
            return;
        }

        var result = await Client().GenerateTextAsync(options);
        Assert.Contains("opal-7291", result.Text);
        Assert.True(calls > 0);
        Assert.True(result.Steps.Count > 1);
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "unused" }));
    }

    private static ILanguageModel Model(string modelId)
    {
        return PerplexityProvider.Create().LanguageModel(modelId);
    }
}
