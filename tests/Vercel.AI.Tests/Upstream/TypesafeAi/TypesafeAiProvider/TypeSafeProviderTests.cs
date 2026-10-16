// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using Vercel.AI.Operations;
using Vercel.AI.TypeSafeAi;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.TypesafeAi.TypesafeAiProvider;

/// <summary>Port of <c>typesafe-ai-provider.test.ts</c> &gt; <c>TypeSafe provider</c>.</summary>
public sealed class TypeSafeProviderTests
{
    private const string Prefix = "packages/typesafe-ai/src/typesafe-ai-provider.test.ts::TypeSafe provider::";

    [Fact]
    [UpstreamTest(Prefix + "provides the experimental decision capability", Coverage = UpstreamCoverage.Covered)]
    public void Provides_the_experimental_decision_capability()
    {
        var model = TypeSafeAiProvider.Default.DecisionModel("jev-latest");
        Assert.Equal("v4", TypeSafeAiProvider.Default.SpecificationVersion);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal("typesafe.decision", model.Provider);
        Assert.Equal("jev-latest", model.ModelId);
        Assert.Equal(new[] { "choice", "score", "boolean", "box" }, model.SupportedQuestionTypes);
    }

    [Fact]
    [UpstreamTest(Prefix + "retains the deprecated factory alias", Coverage = UpstreamCoverage.Partial, Note = "C# cannot make two methods the same object, so the test checks that EvaluationModel returns a model with the decision provider id instead of checking function identity.")]
    public void Retains_the_deprecated_factory_alias()
    {
#pragma warning disable CS0618
        var model = TypeSafeAiProvider.Default.EvaluationModel("jev-latest");
#pragma warning restore CS0618
        Assert.Equal("typesafe.decision", model.Provider);
        Assert.Equal("jev-latest", model.ModelId);
    }

    [Theory]
    [InlineData("languageModel")]
    [InlineData("embeddingModel")]
    [InlineData("imageModel")]
    [UpstreamTest(Prefix + "rejects unsupported %s factories", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_unsupported_factories(string factory)
    {
        var provider = new TypeSafeAiProvider();
        Assert.Throws<NoSuchModelError>(() =>
        {
            switch (factory)
            {
                case "languageModel":
                    provider.LanguageModel("unknown");
                    break;
                case "embeddingModel":
                    provider.EmbeddingModel("unknown");
                    break;
                default:
                    provider.ImageModel("unknown");
                    break;
            }
        });
    }

    [Fact]
    [UpstreamTest(Prefix + "uses the default base URL when TYPESAFE_AI_BASE_URL is not set", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_the_default_base_URL_when_TYPESAFE_AI_BASE_URL_is_not_set()
    {
        using var environment = new EnvironmentScope(null);
        var server = new RecordingHandler();

        await Decide(TypeSafeAiProvider.Create(new TypeSafeAiOptions { ApiKey = "test-api-key" }, server));

        Assert.Equal("https://api.typesafe.ai/v1/systemone", Assert.Single(server.Urls));
    }

    [Fact]
    [UpstreamTest(Prefix + "uses TYPESAFE_AI_BASE_URL without a trailing slash", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_TYPESAFE_AI_BASE_URL_without_a_trailing_slash()
    {
        using var environment = new EnvironmentScope("https://proxy.example/v1/");
        var server = new RecordingHandler();

        await Decide(TypeSafeAiProvider.Create(new TypeSafeAiOptions { ApiKey = "test-api-key" }, server));

        Assert.Equal("https://proxy.example/v1/systemone", Assert.Single(server.Urls));
    }

    [Fact]
    [UpstreamTest(Prefix + "prefers the baseURL option over TYPESAFE_AI_BASE_URL", Coverage = UpstreamCoverage.Covered)]
    public async Task Prefers_the_baseURL_option_over_TYPESAFE_AI_BASE_URL()
    {
        using var environment = new EnvironmentScope("https://environment.example/v1");
        var server = new RecordingHandler();

        await Decide(TypeSafeAiProvider.Create(
            new TypeSafeAiOptions { ApiKey = "test-api-key", BaseUrl = "https://option.example/v1/" },
            server));

        Assert.Equal("https://option.example/v1/systemone", Assert.Single(server.Urls));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "configures the default provider from TYPESAFE_AI_BASE_URL at import time",
        Coverage = UpstreamCoverage.Covered,
        Note = "The default provider reads the variable when it is first created.")]
    public void Configures_the_default_provider_from_TYPESAFE_AI_BASE_URL_at_import_time()
    {
        using var environment = new EnvironmentScope("https://singleton.example/v1/");
        TypeSafeAiProvider.ResetDefault();
        try
        {
            Assert.Equal("https://singleton.example/v1", TypeSafeAiProvider.Default.ResolvedBaseUrl);
        }
        finally
        {
            TypeSafeAiProvider.ResetDefault();
        }
    }

    private static Task Decide(TypeSafeAiProvider provider)
    {
        var questions = new Dictionary<string, DecisionQuestion>
        {
            ["configured"] = new DecisionQuestion("boolean", "Did the request use the configured endpoint?", null),
        };
        var state = new List<object?>
        {
            new Dictionary<string, object?> { ["type"] = "text", ["text"] = "The request used the configured endpoint." },
        };
        using var options = System.Text.Json.JsonDocument.Parse("{}");
        return provider.DecisionModel("jev-latest").DoDecideAsync(
            new DecisionModelCall(state, questions, options.RootElement.Clone(), new Dictionary<string, string>(), CancellationToken.None),
            CancellationToken.None);
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<string> Urls { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Urls.Add(request.RequestUri!.AbsoluteUri);
            const string body = "{\"model\":\"jev-latest\",\"answers\":{\"configured\":{\"type\":\"noul\",\"noul\":1}}}";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
        }
    }

    private sealed class EnvironmentScope : IDisposable
    {
        private readonly string? _previous = Environment.GetEnvironmentVariable(TypeSafeAiProvider.BaseUrlEnvironmentVariable);

        public EnvironmentScope(string? value)
        {
            Environment.SetEnvironmentVariable(TypeSafeAiProvider.BaseUrlEnvironmentVariable, value);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable(TypeSafeAiProvider.BaseUrlEnvironmentVariable, _previous);
        }
    }
}
