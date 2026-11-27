// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexLocationValidation;

/// <summary>Port of <c>google-vertex-location-validation.test.ts</c> &gt; <c>${name} location validation</c> for the <c>google</c>, <c>anthropic</c> and <c>maas</c> providers.</summary>
public sealed class NameLocationValidationTests
{
    private const string Prefix =
        "packages/google-vertex/src/google-vertex-location-validation.test.ts::${name} location validation::";

    public static TheoryData<string, string> Rejected()
    {
        var data = new TheoryData<string, string>();
        foreach (var name in new[] { "google", "anthropic", "maas" })
        {
            foreach (
                var location in new[]
                {
                    "evil.example.com/#",
                    "user@internal:8080/#",
                    "us-east-1/../..",
                    "us east 1",
                    "",
                    "region\n",
                }
            )
            {
                data.Add(name, location);
            }
        }

        return data;
    }

    public static TheoryData<string, string> Accepted()
    {
        var data = new TheoryData<string, string>();
        foreach (var name in new[] { "google", "anthropic", "maas" })
        {
            foreach (var location in new[] { "global", "eu", "us", "us-central1", "us-east5" })
            {
                data.Add(name, location);
            }
        }

        return data;
    }

    /// <summary>Creates the provider and its model, like <c>create({ project, location, fetch })('test-model')</c>.</summary>
    internal static void Create(
        string name,
        string? location,
        RecordingHandler handler,
        string? baseUrl = null
    )
    {
        switch (name)
        {
            case "google":
                GoogleVertexProvider
                    .Create(
                        new VertexOptions
                        {
                            Project = "test",
                            Region = location!,
                            BaseUrl = baseUrl ?? string.Empty,
                            UpstreamRoutes = true,
                            GenerateAuthToken = _ => Task.FromResult("token"),
                        },
                        handler
                    )
                    .LanguageModel("test-model");
                break;
            case "anthropic":
                GoogleVertexAnthropicProvider
                    .Create(
                        new GoogleVertexAnthropicOptions
                        {
                            Project = "test",
                            Location = location,
                            BaseUrl = baseUrl,
                            GenerateAuthToken = _ => Task.FromResult("token"),
                        },
                        handler
                    )
                    .LanguageModel("test-model");
                break;
            default:
                new GoogleVertexMaasProvider(
                    new GoogleVertexPartnerOptions
                    {
                        Project = "test",
                        Location = location,
                        BaseUrl = baseUrl,
                        GenerateAuthToken = _ => Task.FromResult("token"),
                    },
                    handler
                ).LanguageModel("test-model");
                break;
        }
    }

    [Theory]
    [MemberData(nameof(Rejected))]
    [UpstreamTest(Prefix + "rejects %j before fetching", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_before_fetching(string name, string location)
    {
        var handler = new RecordingHandler();

        Assert.ThrowsAny<ArgumentException>(() => Create(name, location, handler));

        Assert.Equal(0, handler.Calls);
    }

    [Theory]
    [MemberData(nameof(Accepted))]
    [UpstreamTest(Prefix + "accepts %j", Coverage = UpstreamCoverage.Covered)]
    public void Accepts(string name, string location)
    {
        Create(name, location, new RecordingHandler());
    }

    [Theory]
    [InlineData("google")]
    [InlineData("anthropic")]
    [InlineData("maas")]
    [UpstreamTest(Prefix + "uses the environment location", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_environment_location(string name)
    {
        var previous = Environment.GetEnvironmentVariable("GOOGLE_VERTEX_LOCATION");
        Environment.SetEnvironmentVariable("GOOGLE_VERTEX_LOCATION", "user@internal/#");
        try
        {
            Assert.ThrowsAny<ArgumentException>(() => Create(name, null, new RecordingHandler()));
        }
        finally
        {
            Environment.SetEnvironmentVariable("GOOGLE_VERTEX_LOCATION", previous);
        }
    }

    [Theory]
    [InlineData("google")]
    [InlineData("anthropic")]
    [InlineData("maas")]
    [UpstreamTest(
        Prefix + "allows a custom URL with an unused invalid location",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Allows_a_custom_URL_with_an_unused_invalid_location(string name)
    {
        Create(name, "not a region", new RecordingHandler(), "https://proxy.example/v1");
    }

    [Theory]
    [InlineData("google")]
    [InlineData("anthropic")]
    [InlineData("maas")]
    [UpstreamTest(
        Prefix + "allows a custom URL without a location",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Allows_a_custom_URL_without_a_location(string name)
    {
        var previous = Environment.GetEnvironmentVariable("GOOGLE_VERTEX_LOCATION");
        Environment.SetEnvironmentVariable("GOOGLE_VERTEX_LOCATION", null);
        try
        {
            Create(name, null, new RecordingHandler(), "https://proxy.example/v1");
        }
        finally
        {
            Environment.SetEnvironmentVariable("GOOGLE_VERTEX_LOCATION", previous);
        }
    }
}
