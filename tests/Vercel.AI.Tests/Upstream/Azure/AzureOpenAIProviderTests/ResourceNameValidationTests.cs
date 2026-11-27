// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Azure;
using Vercel.AI.Provider;
using AzureProvider = Vercel.AI.Azure.AzureOpenAIProvider;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>resourceName validation</c>.</summary>
public sealed class ResourceNameValidationTests
{
    private const string Id =
        "packages/azure/src/azure-openai-provider.test.ts::resourceName validation::";

    public static TheoryData<string> InvalidResourceNames =>
        new()
        {
            "resource\n",
            "resource\r",
            "resource.example",
            "my_resource",
            "résource",
            "-resource",
            "resource-",
            new string('a', 64),
            "",
            "user@internal:8080/#",
            "169.254.169.254:80/x#",
            "evil.example.com/#",
        };

    public static TheoryData<string> BoundaryResourceNames =>
        new() { "a", new string('a', 63), "My-Resource-1" };

    private static LanguageModelCallOptions Prompt() =>
        new() { Prompt = new ModelMessage[] { new UserModelMessage("Hello") } };

    [Theory]
    [MemberData(nameof(InvalidResourceNames))]
    [UpstreamTest(
        Id + "rejects %j before sending a request",
        Coverage = UpstreamCoverage.Covered,
        Note = "The port validates the resource name when the provider is created, so creating the provider and calling the model are one step of the assertion."
    )]
    public async Task Rejects_resourceName_before_sending_a_request(string resourceName)
    {
        var handler = new UpstreamRecordingHandler("{}");

        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            var provider = AzureProvider.Create(
                new AzureOpenAIOptions { ResourceName = resourceName, ApiKey = "test-key" },
                handler
            );
            await provider
                .LanguageModel("test-deployment")
                .DoGenerateAsync(Prompt(), CancellationToken.None);
        });

        Assert.Equal("ResourceName", exception.ParamName);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    [UpstreamTest(Id + "accepts a DNS-label resource name", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_a_DNS_label_resource_name()
    {
        var handler = new UpstreamRecordingHandler("{}", HttpStatusCode.InternalServerError);
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions { ResourceName = "my-resource", ApiKey = "test-key" },
            handler
        );

        await Assert.ThrowsAnyAsync<Exception>(() =>
            provider
                .LanguageModel("test-deployment")
                .DoGenerateAsync(Prompt(), CancellationToken.None)
        );

        Assert.Contains("https://my-resource.openai.azure.com/", handler.Uri);
    }

    [Theory]
    [MemberData(nameof(BoundaryResourceNames))]
    [UpstreamTest(
        Id + "accepts the DNS-label boundary case %j",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Accepts_the_DNS_label_boundary_case(string resourceName)
    {
        var handler = new UpstreamRecordingHandler("{}", HttpStatusCode.InternalServerError);
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions { ResourceName = resourceName, ApiKey = "test-key" },
            handler
        );

        await Assert.ThrowsAnyAsync<Exception>(() =>
            provider
                .LanguageModel("test-deployment")
                .DoGenerateAsync(Prompt(), CancellationToken.None)
        );

        Assert.Equal(
            resourceName.ToLowerInvariant() + ".openai.azure.com",
            new Uri(handler.Uri).Host
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "rejects an invalid AZURE_RESOURCE_NAME for non-language models",
        Coverage = UpstreamCoverage.Covered,
        Note = "The port validates the resource name when the provider is created, so creating the provider and calling the model are one step of the assertion."
    )]
    public async Task Rejects_an_invalid_AZURE_RESOURCE_NAME_for_non_language_models()
    {
        var previous = Environment.GetEnvironmentVariable("AZURE_RESOURCE_NAME");
        Environment.SetEnvironmentVariable("AZURE_RESOURCE_NAME", "user@internal:8080/#");
        try
        {
            var handler = new UpstreamRecordingHandler("{}");

            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                var provider = AzureProvider.Create(
                    new AzureOpenAIOptions { ApiKey = "test-key" },
                    handler
                );
                await provider
                    .EmbeddingModel("test-deployment")
                    .DoEmbedAsync(new[] { "hi" }, null, CancellationToken.None);
            });

            Assert.Equal("ResourceName", exception.ParamName);
            Assert.Equal(0, handler.Calls);
        }
        finally
        {
            Environment.SetEnvironmentVariable("AZURE_RESOURCE_NAME", previous);
        }
    }
}
