// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class BedrockRequestMetadataTests
{
    [Fact]
    public async Task ShouldPassRequestMetadataInGenerateRequests()
    {
        var handler = new ScriptedHandler();
        var model = CreateModel(handler);
        var result = await model.DoGenerateAsync(Call(@"{
            ""amazon-bedrock"": {
                ""requestMetadata"": { ""team"": ""search"", ""environment"": ""prod"" },
                ""reasoningConfig"": { ""type"": ""enabled"", ""budgetTokens"": 1024 },
                ""serviceTier"": ""default"",
                ""additionalModelRequestFields"": { ""topK"": 1 }
            }
        }"), CancellationToken.None);

        Assert.Equal("ok", result.Text);
        Assert.Contains("/converse", handler.Uri);
        Assert.StartsWith("AWS4-HMAC-SHA256", handler.Headers["Authorization"]);
        using var document = JsonDocument.Parse(handler.Body);
        var metadata = document.RootElement.GetProperty("requestMetadata");
        Assert.Equal(2, Count(metadata));
        Assert.Equal("search", metadata.GetProperty("team").GetString());
        Assert.Equal("prod", metadata.GetProperty("environment").GetString());
        Assert.Equal(1, CountOf(handler.Body, "\"requestMetadata\""));
        Assert.False(document.RootElement.TryGetProperty("reasoningConfig", out _));
        Assert.False(document.RootElement.TryGetProperty("serviceTier", out _));
        Assert.False(document.RootElement.TryGetProperty("additionalModelRequestFields", out _));
        Assert.DoesNotContain("reasoningConfig", handler.Body);
        Assert.DoesNotContain("serviceTier", handler.Body);
        Assert.DoesNotContain("additionalModelRequestFields", handler.Body);
    }

    [Fact]
    public async Task ShouldOmitRequestMetadataFromGenerateRequestsWhenNotProvided()
    {
        var handler = new ScriptedHandler();
        var model = CreateModel(handler);
        await model.DoGenerateAsync(new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("hi") },
        }, CancellationToken.None);

        using var document = JsonDocument.Parse(handler.Body);
        Assert.False(document.RootElement.TryGetProperty("requestMetadata", out _));
        Assert.DoesNotContain("requestMetadata", handler.Body);
    }

    [Fact]
    public async Task RejectsNonStringValuesInTheRecord()
    {
        var handler = new ScriptedHandler();
        var model = CreateModel(handler);
        await Assert.ThrowsAsync<ArgumentException>(() => model.DoGenerateAsync(Call(@"{
            ""amazon-bedrock"": { ""requestMetadata"": { ""team"": 42 } }
        }"), CancellationToken.None));

        Assert.Equal(0, handler.Calls);
        Assert.Equal(string.Empty, handler.Body);
    }

    [Fact]
    public async Task ShouldPassRequestMetadataInStreamRequests()
    {
        var handler = new ScriptedHandler();
        var model = CreateModel(handler);
        await foreach (var part in model.DoStreamAsync(Call(@"{
            ""amazon-bedrock"": { ""requestMetadata"": { ""team"": ""search"", ""environment"": ""prod"" } }
        }"), CancellationToken.None))
        {
            _ = part;
        }

        Assert.Contains("/converse", handler.Uri);
        using var document = JsonDocument.Parse(handler.Body);
        var metadata = document.RootElement.GetProperty("requestMetadata");
        Assert.Equal("search", metadata.GetProperty("team").GetString());
        Assert.Equal("prod", metadata.GetProperty("environment").GetString());
        Assert.Equal(2, Count(metadata));
    }

    [Fact]
    public async Task ShouldOmitRequestMetadataFromStreamRequestsWhenNotProvided()
    {
        var handler = new ScriptedHandler();
        var model = CreateModel(handler);
        await foreach (var part in model.DoStreamAsync(new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("hi") },
        }, CancellationToken.None))
        {
            _ = part;
        }

        using var document = JsonDocument.Parse(handler.Body);
        Assert.False(document.RootElement.TryGetProperty("requestMetadata", out _));
        Assert.DoesNotContain("requestMetadata", handler.Body);
        Assert.Contains("/converse", handler.Uri);
    }

    [Fact]
    public async Task AcceptsARecordOfStringString()
    {
        var handler = new ScriptedHandler();
        var model = CreateModel(handler);
        var result = await model.DoGenerateAsync(Call(@"{
            ""amazon-bedrock"": { ""requestMetadata"": { ""team"": ""search"", ""environment"": ""prod"" } }
        }"), CancellationToken.None);

        Assert.Equal("ok", result.Text);
        using var document = JsonDocument.Parse(handler.Body);
        var metadata = document.RootElement.GetProperty("requestMetadata");
        Assert.Equal(2, Count(metadata));
        Assert.Equal("search", metadata.GetProperty("team").GetString());
        Assert.Equal("prod", metadata.GetProperty("environment").GetString());
    }

    [Fact]
    public async Task IsOptional()
    {
        var handler = new ScriptedHandler();
        var model = CreateModel(handler);
        var result = await model.DoGenerateAsync(Call(@"{ ""amazon-bedrock"": {} }"), CancellationToken.None);

        Assert.Equal("ok", result.Text);
        using var document = JsonDocument.Parse(handler.Body);
        Assert.False(document.RootElement.TryGetProperty("requestMetadata", out _));
        Assert.DoesNotContain("requestMetadata", handler.Body);
    }

    [Theory]
    [InlineData("amazonBedrock")]
    [InlineData("bedrock")]
    public async Task Generate_accepts_upstream_provider_option_keys(string key)
    {
        var handler = new ScriptedHandler();
        var model = CreateModel(handler);
        await model.DoGenerateAsync(Call("{\"" + key + "\": { \"requestMetadata\": { \"team\": \"search\", \"environment\": \"prod\" } }}"), CancellationToken.None);

        using var document = JsonDocument.Parse(handler.Body);
        var metadata = document.RootElement.GetProperty("requestMetadata");
        Assert.Equal("search", metadata.GetProperty("team").GetString());
        Assert.Equal("prod", metadata.GetProperty("environment").GetString());
    }

    [Fact]
    public async Task Generate_uses_the_first_present_provider_key()
    {
        var handler = new ScriptedHandler();
        var model = CreateModel(handler);
        await model.DoGenerateAsync(Call(@"{
            ""amazon-bedrock"": { ""reasoningConfig"": { ""type"": ""enabled"" } },
            ""amazonBedrock"": { ""requestMetadata"": { ""team"": ""later"" } },
            ""bedrock"": { ""requestMetadata"": { ""team"": ""legacy"" } }
        }"), CancellationToken.None);

        using var document = JsonDocument.Parse(handler.Body);
        Assert.False(document.RootElement.TryGetProperty("requestMetadata", out _));
        Assert.DoesNotContain("reasoningConfig", handler.Body);

        handler = new ScriptedHandler();
        model = CreateModel(handler);
        await model.DoGenerateAsync(Call(@"{
            ""amazonBedrock"": { ""requestMetadata"": { ""team"": ""primary"" } },
            ""bedrock"": { ""requestMetadata"": { ""team"": ""legacy"" } }
        }"), CancellationToken.None);

        using var second = JsonDocument.Parse(handler.Body);
        Assert.Equal("primary", second.RootElement.GetProperty("requestMetadata").GetProperty("team").GetString());
    }

    private static AmazonBedrockLanguageModel CreateModel(HttpMessageHandler handler)
    {
        var provider = AmazonBedrockProvider.Create(new AmazonBedrockOptions
        {
            AccessKeyId = "AKIA",
            SecretAccessKey = "secret",
            UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
        }, handler);
        return (AmazonBedrockLanguageModel)provider.LanguageModel("anthropic.claude-3-haiku");
    }

    private static LanguageModelCallOptions Call(string providerOptionsJson)
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("hi") },
            ProviderOptions = ParseOptions(providerOptionsJson),
        };
    }

    private static Dictionary<string, JsonElement> ParseOptions(string json)
    {
        using var document = JsonDocument.Parse(json);
        var options = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var property in document.RootElement.EnumerateObject())
        {
            options[property.Name] = property.Value.Clone();
        }

        return options;
    }

    private static int Count(JsonElement element)
    {
        var count = 0;
        foreach (var _ in element.EnumerateObject())
        {
            count++;
        }

        return count;
    }

    private static int CountOf(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
