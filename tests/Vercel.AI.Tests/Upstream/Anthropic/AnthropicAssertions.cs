// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Assertions that match upstream Anthropic unit outcomes.</summary>
internal static class AnthropicAssertions
{
    public static void UsageRawIsUsage()
    {
        var usage = AnthropicParity.ConvertUsage("{\"input_tokens\":10,\"output_tokens\":20}");
        Assert.Equal(10, usage.Raw!.Value.GetProperty("input_tokens").GetInt32());
        Assert.Equal(20, usage.Raw!.Value.GetProperty("output_tokens").GetInt32());
    }

    public static void UsageRawProvided()
    {
        var usage = AnthropicParity.ConvertUsage(
            "{\"input_tokens\":10,\"output_tokens\":20}",
            "{\"input_tokens\":10,\"output_tokens\":20,\"service_tier\":\"standard\"}");
        Assert.Equal("standard", usage.Raw!.Value.GetProperty("service_tier").GetString());
    }

    public static void UsageCacheTotals()
    {
        var usage = AnthropicParity.ConvertUsage("{\"input_tokens\":10,\"output_tokens\":20,\"cache_creation_input_tokens\":5,\"cache_read_input_tokens\":3}");
        Assert.Equal(18, usage.InputTokens);
        Assert.Equal(10, usage.NoCacheInputTokens);
        Assert.Equal(3, usage.CacheReadTokens);
        Assert.Equal(5, usage.CacheWriteTokens);
        Assert.Equal(20, usage.OutputTokens);
        Assert.Null(usage.ReasoningTokens);
        Assert.Null(usage.TextTokens);
    }

    public static void UsageNullCache()
    {
        var usage = AnthropicParity.ConvertUsage("{\"input_tokens\":100,\"output_tokens\":50,\"cache_creation_input_tokens\":null,\"cache_read_input_tokens\":null}");
        Assert.Equal(100, usage.InputTokens);
        Assert.Equal(0, usage.CacheReadTokens);
        Assert.Equal(0, usage.CacheWriteTokens);
    }

    public static void UsageIterationsSum()
    {
        var usage = AnthropicParity.ConvertUsage("{\"input_tokens\":45000,\"output_tokens\":1234,\"iterations\":[{\"type\":\"compaction\",\"input_tokens\":180000,\"output_tokens\":3500},{\"type\":\"message\",\"input_tokens\":23000,\"output_tokens\":1000}]}");
        Assert.Equal(203000, usage.InputTokens);
        Assert.Equal(203000, usage.NoCacheInputTokens);
        Assert.Equal(4500, usage.OutputTokens);
        Assert.Equal(45000, usage.Raw!.Value.GetProperty("input_tokens").GetInt32());
    }

    public static void UsageSingleIteration()
    {
        var usage = AnthropicParity.ConvertUsage("{\"input_tokens\":5000,\"output_tokens\":500,\"iterations\":[{\"type\":\"message\",\"input_tokens\":5000,\"output_tokens\":500}]}");
        Assert.Equal(5000, usage.InputTokens);
        Assert.Equal(500, usage.OutputTokens);
    }

    public static void UsageManyIterations()
    {
        var usage = AnthropicParity.ConvertUsage("{\"input_tokens\":10000,\"output_tokens\":500,\"iterations\":[{\"type\":\"compaction\",\"input_tokens\":200000,\"output_tokens\":4000},{\"type\":\"message\",\"input_tokens\":50000,\"output_tokens\":2000},{\"type\":\"compaction\",\"input_tokens\":180000,\"output_tokens\":3500},{\"type\":\"message\",\"input_tokens\":30000,\"output_tokens\":1500}]}");
        Assert.Equal(460000, usage.InputTokens);
        Assert.Equal(11000, usage.OutputTokens);
    }

    public static void UsageIterationsAndCache()
    {
        var usage = AnthropicParity.ConvertUsage("{\"input_tokens\":45000,\"output_tokens\":1234,\"cache_creation_input_tokens\":1000,\"cache_read_input_tokens\":500,\"iterations\":[{\"type\":\"compaction\",\"input_tokens\":180000,\"output_tokens\":3500},{\"type\":\"message\",\"input_tokens\":23000,\"output_tokens\":1000}]}");
        Assert.Equal(203000, usage.NoCacheInputTokens);
        Assert.Equal(1000, usage.CacheWriteTokens);
        Assert.Equal(500, usage.CacheReadTokens);
        Assert.Equal(204500, usage.InputTokens);
        Assert.Equal(4500, usage.OutputTokens);
    }

    public static void UsageRawWithIterations()
    {
        var usage = AnthropicParity.ConvertUsage(
            "{\"input_tokens\":45000,\"output_tokens\":1234,\"iterations\":[{\"type\":\"compaction\",\"input_tokens\":180000,\"output_tokens\":3500},{\"type\":\"message\",\"input_tokens\":23000,\"output_tokens\":1000}]}",
            "{\"input_tokens\":45000,\"output_tokens\":1234,\"service_tier\":\"standard\"}");
        Assert.Equal("standard", usage.Raw!.Value.GetProperty("service_tier").GetString());
        Assert.Equal(203000, usage.InputTokens);
    }

    public static void UsageTopLevel(string json)
    {
        var usage = AnthropicParity.ConvertUsage(json);
        Assert.Equal(100, usage.InputTokens);
        Assert.Equal(50, usage.OutputTokens);
    }

    public static void UsageZeroIterations()
    {
        var usage = AnthropicParity.ConvertUsage("{\"input_tokens\":0,\"output_tokens\":0,\"iterations\":[{\"type\":\"compaction\",\"input_tokens\":0,\"output_tokens\":0},{\"type\":\"message\",\"input_tokens\":0,\"output_tokens\":0}]}");
        Assert.Equal(0, usage.InputTokens);
        Assert.Equal(0, usage.OutputTokens);
    }

    public static void UsageDocumentation()
    {
        var usage = AnthropicParity.ConvertUsage("{\"input_tokens\":45000,\"output_tokens\":1234,\"iterations\":[{\"type\":\"compaction\",\"input_tokens\":180000,\"output_tokens\":3500},{\"type\":\"message\",\"input_tokens\":23000,\"output_tokens\":1000}]}");
        Assert.Equal(203000, usage.InputTokens);
        Assert.Equal(4500, usage.OutputTokens);
        Assert.NotEqual(45000, usage.InputTokens);
        Assert.NotEqual(1234, usage.OutputTokens);
    }

    public static void UsageReapplied()
    {
        var usage = AnthropicParity.ConvertUsage("{\"input_tokens\":15000,\"output_tokens\":800}");
        Assert.Equal(15000, usage.InputTokens);
        Assert.Equal(800, usage.OutputTokens);
    }

    public static void SchemaNumbers()
    {
        var node = AnthropicJsonSchema.Sanitize(AnthropicParity.Json("{\"type\":\"object\",\"properties\":{\"recurringIntervalMinutes\":{\"type\":\"number\",\"exclusiveMinimum\":0,\"minimum\":1,\"maximum\":60,\"exclusiveMaximum\":120}},\"required\":[\"recurringIntervalMinutes\"],\"additionalProperties\":false}"));
        var text = node.ToJsonString();
        Assert.Contains("\"additionalProperties\":false", text);
        Assert.Contains("minimum: 1; maximum: 60; exclusive minimum: 0; exclusive maximum: 120.", text);
        Assert.DoesNotContain("exclusiveMinimum", text);
    }

    public static void SchemaStrings()
    {
        var node = AnthropicJsonSchema.Sanitize(AnthropicParity.Json("{\"type\":\"object\",\"properties\":{\"slug\":{\"type\":\"string\",\"description\":\"A URL slug\",\"minLength\":1,\"maxLength\":20,\"pattern\":\"^[a-z0-9-]+$\",\"format\":\"regex\"}}}"));
        var description = node["properties"]!["slug"]!["description"]!.GetValue<string>();
        Assert.Equal("A URL slug\nmin length: 1; max length: 20; pattern: ^[a-z0-9-]+$; format: regex.", description);
        Assert.False(node["additionalProperties"]!.GetValue<bool>());
    }

    public static void SchemaRecursive()
    {
        var node = AnthropicJsonSchema.Sanitize(AnthropicParity.Json("{\"type\":\"object\",\"$defs\":{\"PositiveInteger\":{\"type\":\"integer\",\"minimum\":1}},\"properties\":{\"count\":{\"$ref\":\"#/$defs/PositiveInteger\"},\"tags\":{\"type\":\"array\",\"minItems\":2,\"maxItems\":4,\"uniqueItems\":true,\"items\":{\"anyOf\":[{\"type\":\"string\",\"minLength\":1},{\"type\":\"number\",\"maximum\":10}]}}}}"));
        Assert.Equal("#/$defs/PositiveInteger", node["properties"]!["count"]!["$ref"]!.GetValue<string>());
        Assert.Contains("minimum: 1.", node["$defs"]!["PositiveInteger"]!["description"]!.GetValue<string>());
        Assert.Contains("min items: 2; max items: 4; unique items: true.", node["properties"]!["tags"]!["description"]!.GetValue<string>());
        Assert.NotNull(node["properties"]!["tags"]!["items"]!["anyOf"]);
    }

    public static void SchemaOneOf()
    {
        var node = AnthropicJsonSchema.Sanitize(AnthropicParity.Json("{\"oneOf\":[{\"type\":\"string\",\"minLength\":1},{\"type\":\"number\",\"minimum\":0}]}"));
        Assert.NotNull(node["anyOf"]);
        Assert.Null(node["oneOf"]);
        Assert.Contains("min length: 1.", node.ToJsonString());
    }

    public static void SchemaDoesNotMutate()
    {
        var original = "{\"type\":\"object\",\"properties\":{\"n\":{\"type\":\"number\",\"minimum\":1}}}";
        using var document = JsonDocument.Parse(original);
        var node = AnthropicJsonSchema.Sanitize(document.RootElement.Clone());
        Assert.Contains("minimum: 1.", node.ToJsonString());
        Assert.Equal(original, document.RootElement.GetRawText());
    }

    public static void ErrorOverloaded()
    {
        var parsed = AnthropicErrorData.Parse("{\"type\":\"error\",\"error\":{\"details\":null,\"type\":\"overloaded_error\",\"message\":\"Overloaded\"}}");
        Assert.True(parsed.Success);
        Assert.Equal("overloaded_error", parsed.ErrorType);
        Assert.Equal("Overloaded", parsed.Message);
        Assert.DoesNotContain("details", parsed.Value()!.ToJsonString());
        Assert.Equal(500, AnthropicStreamError.StatusCode("api_error"));
        Assert.Equal(529, AnthropicStreamError.StatusCode("overloaded_error"));
        Assert.True(AnthropicStreamError.IsRetryable("overloaded_error"));
        Assert.Equal(413, AnthropicStreamError.StatusCode("request_too_large"));
        Assert.False(AnthropicStreamError.IsRetryable("invalid_request_error"));
    }

    public static void Opus48()
    {
        AnthropicParity.AssertCapability("claude-opus-4-8", 128000, true, true, true, true, true, false, false, false);
    }

    public static void Fable5()
    {
        AnthropicParity.AssertCapability("claude-fable-5", 128000, true, true, true, true, true, false, true, false);
    }

    public static void Fable51()
    {
        AnthropicParity.AssertCapability("claude-fable-5-1", 128000, true, true, true, true, true, false, true, true);
    }

    public static void Opus47()
    {
        AnthropicParity.AssertCapability("claude-opus-4-7", 128000, true, true, true, true, true, false, false, false);
    }

    public static void Sonnet5()
    {
        AnthropicParity.AssertCapability("claude-sonnet-5", 128000, true, true, true, true, true, false, false, false);
    }

    public static void Opus46()
    {
        var caps = AnthropicModelCapabilities.Get("claude-opus-4-6");
        Assert.False(caps.RejectsSamplingParameters);
        Assert.False(caps.SupportsXhighEffort);
        Assert.True(caps.SupportsAdaptiveThinking);
    }

    public static void Sonnet46()
    {
        var caps = AnthropicModelCapabilities.Get("claude-sonnet-4-6");
        Assert.False(caps.RejectsSamplingParameters);
        Assert.False(caps.SupportsXhighEffort);
        Assert.True(caps.SupportsAdaptiveThinking);
    }

    public static void Opus5()
    {
        AnthropicParity.AssertCapability("claude-opus-5", 128000, true, true, true, true, true, true, false, false);
    }

    public static void UnknownClaude()
    {
        AnthropicParity.AssertCapability("claude-future-9", 128000, false, true, true, true, true, true, false, false);
    }

    public static void PrefixedUnknown()
    {
        AnthropicParity.AssertCapability("us.anthropic.claude-future-9-20990101-v1:0", 128000, false, true, true, true, true, true, false, false);
    }

    public static void LegacyClaude()
    {
        foreach (var model in new[]
        {
            "anthropic.claude-3-5-sonnet-20241022-v2:0",
            "us.anthropic.claude-3-7-sonnet-20250219-v1:0",
            "anthropic.claude-v2:1",
            "anthropic.claude-instant-v1",
        })
        {
            AnthropicParity.AssertCapability(model, 4096, false, false, false, false, false, false, false, false);
        }
    }

    public static void KnownBeforeFallback()
    {
        AnthropicParity.AssertCapability("claude-opus-4-5", 64000, true, true, false, false, false, false, false, false);
    }

    public static void VertexIds()
    {
        var sonnet = AnthropicModelCapabilities.Get("claude-sonnet-4@20250514");
        Assert.True(sonnet.IsKnownModel);
        Assert.Equal(64000, sonnet.MaxOutputTokens);
        var opus = AnthropicModelCapabilities.Get("claude-opus-4@20250514");
        Assert.True(opus.IsKnownModel);
        Assert.Equal(32000, opus.MaxOutputTokens);
    }

    public static void NonClaude()
    {
        AnthropicParity.AssertCapability("third-party-future-model", 4096, false, false, false, false, false, false, false, false);
    }

    public static async Task DefaultBaseUrl()
    {
        using (new EnvScope().Set("ANTHROPIC_BASE_URL", null))
        {
            var handler = AnthropicParity.Ok();
            var provider = AnthropicParity.Client(handler);
            await AnthropicParity.Generate(provider, "claude-3-haiku-20240307").ConfigureAwait(false);
            Assert.Equal("https://api.anthropic.com/v1/messages", handler.Uri);
        }
    }

    public static async Task EnvBaseUrl()
    {
        using (new EnvScope().Set("ANTHROPIC_BASE_URL", "https://proxy.anthropic.example/v1/"))
        {
            var handler = AnthropicParity.Ok();
            await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-3-haiku-20240307").ConfigureAwait(false);
            Assert.Equal("https://proxy.anthropic.example/v1/messages", handler.Uri);
        }
    }

    public static async Task NormalizeEnvOfficial()
    {
        using (new EnvScope().Set("ANTHROPIC_BASE_URL", "https://api.anthropic.com/"))
        {
            var handler = AnthropicParity.Ok();
            await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-3-haiku-20240307").ConfigureAwait(false);
            Assert.Equal("https://api.anthropic.com/v1/messages", handler.Uri);
        }
    }

    public static async Task NormalizeOptionOfficial()
    {
        using (new EnvScope().Set("ANTHROPIC_BASE_URL", null))
        {
            var handler = AnthropicParity.Ok();
            var provider = AnthropicParity.Client(handler, new AnthropicOptions { ApiKey = "test-api-key", BaseUrl = "https://api.anthropic.com/" });
            await AnthropicParity.Generate(provider, "claude-3-haiku-20240307").ConfigureAwait(false);
            Assert.Equal("https://api.anthropic.com/v1/messages", handler.Uri);
        }
    }

    public static async Task OptionBeatsEnv()
    {
        using (new EnvScope().Set("ANTHROPIC_BASE_URL", "https://env.anthropic.example/v1"))
        {
            var handler = AnthropicParity.Ok();
            var provider = AnthropicParity.Client(handler, new AnthropicOptions { ApiKey = "test-api-key", BaseUrl = "https://option.anthropic.example/v1/" });
            await AnthropicParity.Generate(provider, "claude-3-haiku-20240307").ConfigureAwait(false);
            Assert.Equal("https://option.anthropic.example/v1/messages", handler.Uri);
        }
    }

    public static void EmptyBaseUrl()
    {
        var exception = Assert.Throws<ArgumentException>(() => AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-api-key", BaseUrl = "" }));
        Assert.Contains("baseURL must be a non-empty string.", exception.Message);
    }

    public static async Task AuthToken()
    {
        var handler = AnthropicParity.Ok();
        var provider = AnthropicProvider.Create(new AnthropicOptions { AuthToken = "test-auth-token" }, handler);
        await AnthropicParity.Generate(provider, "claude-3-haiku-20240307").ConfigureAwait(false);
        Assert.Equal("Bearer test-auth-token", handler.Headers["Authorization"]);
        Assert.False(handler.Headers.ContainsKey("x-api-key"));
    }

    public static void AuthConflict()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-api-key", AuthToken = "test-auth-token" }));
        Assert.Equal("Both apiKey and authToken were provided. Please use only one authentication method.", exception.Message);
    }

    public static void CustomName()
    {
        var provider = AnthropicProvider.Create(new AnthropicOptions { Name = "my-claude-proxy", ApiKey = "test-api-key" });
        Assert.Equal("my-claude-proxy", provider.LanguageModel("claude-3-haiku-20240307").Provider);
    }

    public static void DefaultName()
    {
        var provider = AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-api-key" });
        Assert.Equal("anthropic.messages", provider.LanguageModel("claude-3-haiku-20240307").Provider);
    }

    public static void SupportedImage()
    {
        var model = (AnthropicLanguageModel)AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-api-key" }).LanguageModel("claude-3-haiku-20240307");
        Assert.True(model.SupportedUrls.ContainsKey("image/*"));
        Assert.True(model.SupportsUrl("image/*", "https://example.com/image.png"));
    }

    public static void SupportedPdf()
    {
        var model = (AnthropicLanguageModel)AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-api-key" }).LanguageModel("claude-3-haiku-20240307");
        Assert.True(model.SupportsUrl("application/pdf", "https://arxiv.org/pdf/2401.00001"));
    }

    public static async Task AwsRegionTemplate()
    {
        var handler = AnthropicParity.Ok();
        var provider = AnthropicParity.Aws(handler, new AnthropicOptions { Region = "us-east-1", WorkspaceId = "wrkspc_test", ApiKey = "test-api-key" });
        await AnthropicParity.Generate(provider, "claude-sonnet-4-6").ConfigureAwait(false);
        Assert.Equal("https://aws-external-anthropic.us-east-1.api.aws/v1/messages", handler.Uri);
    }

    public static async Task AwsRegionEnv()
    {
        using (new EnvScope().Set("AWS_REGION", "eu-west-1").Set("ANTHROPIC_AWS_WORKSPACE_ID", null).Set("ANTHROPIC_AWS_API_KEY", null))
        {
            var handler = AnthropicParity.Ok();
            var provider = AnthropicParity.Aws(handler, new AnthropicOptions { WorkspaceId = "wrkspc_test", ApiKey = "test-api-key" });
            await AnthropicParity.Generate(provider, "claude-sonnet-4-6").ConfigureAwait(false);
            Assert.Equal("https://aws-external-anthropic.eu-west-1.api.aws/v1/messages", handler.Uri);
        }
    }

    public static async Task AwsBaseUrlWins()
    {
        var handler = AnthropicParity.Ok();
        var provider = AnthropicParity.Aws(handler, new AnthropicOptions
        {
            Region = "us-west-2",
            WorkspaceId = "wrkspc_test",
            ApiKey = "test-api-key",
            BaseUrl = "https://proxy.example.com/v1/",
        });
        await AnthropicParity.Generate(provider, "claude-sonnet-4-6").ConfigureAwait(false);
        Assert.Equal("https://proxy.example.com/v1/messages", handler.Uri);
    }

    public static async Task AwsApiKey()
    {
        var handler = AnthropicParity.Ok();
        var provider = AnthropicParity.Aws(handler, new AnthropicOptions { Region = "us-west-2", WorkspaceId = "wrkspc_test", ApiKey = "sk-aws-platform-key" });
        await AnthropicParity.Generate(provider, "claude-sonnet-4-6").ConfigureAwait(false);
        Assert.Equal("sk-aws-platform-key", handler.Headers["x-api-key"]);
        Assert.False(handler.Headers.ContainsKey("Authorization"));
    }

    public static async Task AwsApiKeyEnv()
    {
        using (new EnvScope().Set("ANTHROPIC_AWS_API_KEY", "sk-from-env").Set("AWS_REGION", null))
        {
            var handler = AnthropicParity.Ok();
            var provider = AnthropicParity.Aws(handler, new AnthropicOptions { Region = "us-west-2", WorkspaceId = "wrkspc_test" });
            await AnthropicParity.Generate(provider, "claude-sonnet-4-6").ConfigureAwait(false);
            Assert.Equal("sk-from-env", handler.Headers["x-api-key"]);
        }
    }

    public static async Task AwsSigns()
    {
        var handler = AnthropicParity.Ok();
        var provider = AnthropicParity.Aws(handler, new AnthropicOptions
        {
            Region = "us-west-2",
            WorkspaceId = "wrkspc_test",
            AccessKeyId = "akid",
            SecretAccessKey = "secret",
            UtcNow = () => new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero),
        });
        await AnthropicParity.Generate(provider, "claude-sonnet-4-6").ConfigureAwait(false);
        Assert.Contains("Credential=akid/20240315/us-west-2/aws-external-anthropic/aws4_request", handler.Headers["Authorization"]);
        Assert.False(handler.Headers.ContainsKey("x-api-key"));
        Assert.Equal("20240315T000000Z", handler.Headers["x-amz-date"]);
    }

    public static async Task AwsCredentialProvider()
    {
        var handler = AnthropicParity.Ok();
        var called = 0;
        var provider = AnthropicParity.Aws(handler, new AnthropicOptions
        {
            Region = "us-west-2",
            WorkspaceId = "wrkspc_test",
            CredentialProvider = _ =>
            {
                called++;
                return Task.FromResult(new AnthropicAwsCredentials("us-west-2", "dynamic-akid", "dynamic-secret", "dynamic-session"));
            },
        });
        await AnthropicParity.Generate(provider, "claude-sonnet-4-6").ConfigureAwait(false);
        Assert.Equal(1, called);
        Assert.Contains("Credential=dynamic-akid/", handler.Headers["Authorization"]);
        Assert.Equal("dynamic-session", handler.Headers["x-amz-security-token"]);
    }

    public static async Task AwsMissingCredentials()
    {
        using (new EnvScope().Set("AWS_ACCESS_KEY_ID", null).Set("AWS_SECRET_ACCESS_KEY", null).Set("ANTHROPIC_AWS_API_KEY", null))
        {
            var provider = AnthropicParity.Aws(AnthropicParity.Ok(), new AnthropicOptions { Region = "us-west-2", WorkspaceId = "wrkspc_test" });
            var exception = await Assert.ThrowsAsync<AiSdkException>(() => AnthropicParity.Generate(provider, "claude-sonnet-4-6")).ConfigureAwait(false);
            Assert.Contains("AWS SigV4 authentication requires AWS credentials", exception.Message);
        }
    }

    public static async Task AwsCredentialFailure()
    {
        var provider = AnthropicParity.Aws(AnthropicParity.Ok(), new AnthropicOptions
        {
            Region = "us-west-2",
            WorkspaceId = "wrkspc_test",
            CredentialProvider = _ => throw new InvalidOperationException("STS denied"),
        });
        var exception = await Assert.ThrowsAsync<AiSdkException>(() => AnthropicParity.Generate(provider, "claude-sonnet-4-6")).ConfigureAwait(false);
        Assert.Contains("AWS credential provider failed: STS denied", exception.Message);
    }

    public static async Task AwsVersion()
    {
        var handler = AnthropicParity.Ok();
        await AnthropicParity.Generate(AnthropicParity.Aws(handler, new AnthropicOptions { Region = "us-west-2", WorkspaceId = "wrkspc_test", ApiKey = "test-api-key" }), "claude-sonnet-4-6").ConfigureAwait(false);
        Assert.Equal("2023-06-01", handler.Headers["anthropic-version"]);
    }

    public static async Task AwsWorkspace()
    {
        var handler = AnthropicParity.Ok();
        await AnthropicParity.Generate(AnthropicParity.Aws(handler, new AnthropicOptions { Region = "us-west-2", WorkspaceId = "wrkspc_unique", ApiKey = "test-api-key" }), "claude-sonnet-4-6").ConfigureAwait(false);
        Assert.Equal("wrkspc_unique", handler.Headers["anthropic-workspace-id"]);
    }

    public static async Task AwsWorkspaceEnv()
    {
        using (new EnvScope().Set("ANTHROPIC_AWS_WORKSPACE_ID", "wrkspc_from_env"))
        {
            var handler = AnthropicParity.Ok();
            await AnthropicParity.Generate(AnthropicParity.Aws(handler, new AnthropicOptions { Region = "us-west-2", ApiKey = "test-api-key" }), "claude-sonnet-4-6").ConfigureAwait(false);
            Assert.Equal("wrkspc_from_env", handler.Headers["anthropic-workspace-id"]);
        }
    }

    public static async Task AwsWorkspaceMissing()
    {
        using (new EnvScope().Set("ANTHROPIC_AWS_WORKSPACE_ID", null))
        {
            var provider = AnthropicParity.Aws(AnthropicParity.Ok(), new AnthropicOptions { Region = "us-west-2", ApiKey = "test-api-key" });
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => AnthropicParity.Generate(provider, "claude-sonnet-4-6")).ConfigureAwait(false);
            Assert.Contains("workspace", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static void AwsRegionMissing()
    {
        using (new EnvScope().Set("AWS_REGION", null))
        {
            var provider = AnthropicParity.Aws(AnthropicParity.Ok(), new AnthropicOptions { WorkspaceId = "wrkspc_test", ApiKey = "test-api-key" });
            var exception = Assert.Throws<ArgumentException>(() => provider.LanguageModel("claude-sonnet-4-6"));
            Assert.Contains("region", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static async Task AwsCustomHeaders()
    {
        var handler = AnthropicParity.Ok();
        var options = new AnthropicOptions { Region = "us-west-2", WorkspaceId = "wrkspc_test", ApiKey = "test-api-key" };
        options.Headers["x-custom"] = "value";
        await AnthropicParity.Generate(AnthropicParity.Aws(handler, options), "claude-sonnet-4-6").ConfigureAwait(false);
        Assert.Equal("wrkspc_test", handler.Headers["anthropic-workspace-id"]);
        Assert.Equal("value", handler.Headers["x-custom"]);
    }

    public static void AwsSupportedImage()
    {
        var model = (AnthropicLanguageModel)AnthropicParity.Aws(AnthropicParity.Ok(), new AnthropicOptions { Region = "us-west-2", WorkspaceId = "wrkspc_test", ApiKey = "test-api-key" }).LanguageModel("claude-sonnet-4-6");
        Assert.True(model.SupportsUrl("image/*", "https://example.com/image.png"));
    }

    public static void AwsSupportedPdf()
    {
        var model = (AnthropicLanguageModel)AnthropicParity.Aws(AnthropicParity.Ok(), new AnthropicOptions { Region = "us-west-2", WorkspaceId = "wrkspc_test", ApiKey = "test-api-key" }).LanguageModel("claude-sonnet-4-6");
        Assert.True(model.SupportsUrl("application/pdf", "https://arxiv.org/pdf/2401.00001"));
    }

    public static void AwsProviderName()
    {
        var provider = AnthropicParity.Aws(AnthropicParity.Ok(), new AnthropicOptions { Region = "us-west-2", WorkspaceId = "wrkspc_test", ApiKey = "test-api-key" });
        Assert.Equal("anthropic-aws.messages", provider.LanguageModel("claude-sonnet-4-6").Provider);
    }

    public static void AwsNoEmbedding()
    {
        var provider = AnthropicParity.Aws(AnthropicParity.Ok(), new AnthropicOptions { Region = "us-west-2", WorkspaceId = "wrkspc_test", ApiKey = "test-api-key" });
        var exception = Assert.Throws<AiSdkException>(() => provider.EmbeddingModel("any-model-id"));
        Assert.Contains("embeddingModel", exception.Message);
    }

    public static void AwsNoImage()
    {
        var provider = AnthropicParity.Aws(AnthropicParity.Ok(), new AnthropicOptions { Region = "us-west-2", WorkspaceId = "wrkspc_test", ApiKey = "test-api-key" });
        var exception = Assert.Throws<AiSdkException>(() => provider.ImageModel("any-model-id"));
        Assert.Contains("imageModel", exception.Message);
    }

    public static void AwsFiles()
    {
        var files = AnthropicParity.Aws(AnthropicParity.Ok(), new AnthropicOptions { Region = "us-west-2", WorkspaceId = "wrkspc_test", ApiKey = "test-api-key" }).Files();
        Assert.Equal("v4", files.SpecificationVersion);
        Assert.Equal("anthropic-aws.messages", files.Provider);
    }

    public static void AwsSkills()
    {
        var skills = AnthropicParity.Aws(AnthropicParity.Ok(), new AnthropicOptions { Region = "us-west-2", WorkspaceId = "wrkspc_test", ApiKey = "test-api-key" }).Skills();
        Assert.Equal("v4", skills.SpecificationVersion);
        Assert.Equal("anthropic-aws.skills", skills.Provider);
    }

    public static void AwsNewKeyword()
    {
        var provider = AnthropicParity.Aws(AnthropicParity.Ok(), new AnthropicOptions { Region = "us-west-2", WorkspaceId = "wrkspc_test", ApiKey = "test-api-key" });
        var exception = Assert.Throws<InvalidOperationException>(() => provider.New("claude-sonnet-4-6"));
        Assert.Contains("cannot be called with the new keyword", exception.Message);
    }

    public static async Task AwsApiKeyBeatsSigV4()
    {
        using (new EnvScope().Set("AWS_ACCESS_KEY_ID", "should-be-ignored").Set("AWS_SECRET_ACCESS_KEY", "should-be-ignored"))
        {
            var handler = AnthropicParity.Ok();
            await AnthropicParity.Generate(AnthropicParity.Aws(handler, new AnthropicOptions { Region = "us-west-2", WorkspaceId = "wrkspc_test", ApiKey = "sk-aws-platform-key" }), "claude-sonnet-4-6").ConfigureAwait(false);
            Assert.Equal("sk-aws-platform-key", handler.Headers["x-api-key"]);
            Assert.False(handler.Headers.ContainsKey("Authorization"));
            Assert.False(handler.Headers.ContainsKey("x-amz-date"));
        }
    }

    public static async Task AwsStream()
    {
        var handler = AnthropicParity.Ok();
        handler.ContentType = "text/event-stream";
        handler.ResponseBody = "data: {\"type\":\"message_start\",\"message\":{\"id\":\"msg_1\",\"model\":\"claude-sonnet-4-6\",\"usage\":{\"input_tokens\":1,\"output_tokens\":0}}}\n\ndata: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}\n\ndata: {\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"text_delta\",\"text\":\"Hi\"}}\n\ndata: {\"type\":\"content_block_stop\",\"index\":0}\n\ndata: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\"},\"usage\":{\"output_tokens\":1}}\n\ndata: {\"type\":\"message_stop\"}\n\n";
        var provider = AnthropicParity.Aws(handler, new AnthropicOptions { Region = "us-west-2", WorkspaceId = "wrkspc_test", ApiKey = "test-api-key" });
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in provider.LanguageModel("claude-sonnet-4-6").DoStreamAsync(AnthropicParity.Hello(), CancellationToken.None).ConfigureAwait(false))
        {
            parts.Add(part);
        }

        Assert.Contains(parts, part => part is TextDeltaStreamPart delta && delta.Delta == "Hi");
        Assert.Equal("2023-06-01", handler.Headers["anthropic-version"]);
        Assert.Equal("wrkspc_test", handler.Headers["anthropic-workspace-id"]);
        Assert.Equal("test-api-key", handler.Headers["x-api-key"]);
    }

    public static void FetchBypassGet()
    {
        var call = AnthropicAwsFetch.Prepare("http://example.com", "GET", null, null, null, null, null, Creds());
        Assert.False(call.Signed);
        Assert.Equal("GET", call.Method);
        Assert.Equal(AnthropicAwsFetch.UserAgent, call.Headers["user-agent"]);
        Assert.False(call.Headers.ContainsKey("authorization"));
    }

    public static void FetchBypassEmptyPost()
    {
        var call = AnthropicAwsFetch.Prepare("http://example.com", "POST", null, null, null, null, null, Creds());
        Assert.False(call.Signed);
        Assert.Equal("POST", call.Method);
        Assert.False(call.Headers.ContainsKey("authorization"));
    }

    public static void FetchStringBody()
    {
        var headers = new Dictionary<string, string?> { ["Content-Type"] = "application/json", ["Custom-Header"] = "value", ["empty-header"] = "" };
        var call = AnthropicAwsFetch.Prepare("http://example.com", "POST", "{\"test\": \"data\"}", null, null, headers, null, new AnthropicAwsCredentials("us-west-2", "test-access-key", "test-secret", "test-session-token"), new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero));
        Assert.Equal("application/json", call.Headers["content-type"]);
        Assert.Equal("value", call.Headers["custom-header"]);
        Assert.False(call.Headers.ContainsKey("empty-header"));
        Assert.Equal("20240315T000000Z", call.Headers["x-amz-date"]);
        Assert.Contains("Credential=test-access-key/20240315/us-west-2/aws-external-anthropic/aws4_request", call.Headers["authorization"]);
        Assert.Equal("test-session-token", call.Headers["x-amz-security-token"]);
        Assert.StartsWith("ai-sdk/anthropic-aws/0.0.0-test", call.Headers["user-agent"]);
        Assert.Equal("{\"test\": \"data\"}", call.Body);
    }

    public static void FetchRequestMerge()
    {
        var requestHeaders = new Dictionary<string, string?> { ["X-From-Request"] = "from-request" };
        var headers = new Dictionary<string, string?> { ["Content-Type"] = "application/json", ["Custom-Header"] = "value" };
        var call = AnthropicAwsFetch.Prepare("http://example.com", "POST", "{\"test\": \"data\"}", null, null, headers, requestHeaders, Creds(), new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero));
        Assert.Equal("http://example.com", call.Url);
        Assert.Equal("{\"test\": \"data\"}", call.Body);
        Assert.Equal("application/json", call.Headers["content-type"]);
        Assert.Equal("value", call.Headers["custom-header"]);
        Assert.Equal("from-request", call.Headers["x-from-request"]);
        Assert.True(call.Signed);
    }

    public static void FetchRequestOnly()
    {
        var requestHeaders = new Dictionary<string, string?> { ["Content-Type"] = "application/json", ["X-From-Request"] = "from-request" };
        var call = AnthropicAwsFetch.Prepare("http://example.com", "POST", "{\"test\": \"data\"}", null, null, null, requestHeaders, Creds(), new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero));
        Assert.Equal("{\"test\": \"data\"}", call.Body);
        Assert.Equal("from-request", call.Headers["x-from-request"]);
        Assert.StartsWith("ai-sdk/anthropic-aws/0.0.0-test", call.Headers["user-agent"]);
        Assert.True(call.Signed);
    }

    public static void FetchObjectBody()
    {
        var call = AnthropicAwsFetch.Prepare("http://example.com", "POST", null, null, JsonNode.Parse("{\"field\":\"value\"}"), new Dictionary<string, string?>(), null, Creds());
        Assert.Equal("{\"field\":\"value\"}", call.Body);
        Assert.True(call.Signed);
    }

    public static void FetchBytes()
    {
        var call = AnthropicAwsFetch.Prepare("http://example.com", "POST", null, EncodingUtf8("binaryTest"), null, new Dictionary<string, string?>(), null, Creds());
        Assert.Equal("binaryTest", call.Body);
    }

    public static void FetchBuffer()
    {
        var call = AnthropicAwsFetch.Prepare("http://example.com", "POST", null, EncodingUtf8("bufferTest"), null, new Dictionary<string, string?>(), null, Creds());
        Assert.Equal("bufferTest", call.Body);
    }

    public static void FetchHeaderMap()
    {
        var headers = new Dictionary<string, string?> { ["A"] = "value-a", ["B"] = "value-b" };
        var call = AnthropicAwsFetch.Prepare("http://example.com", "POST", "{\"test\": \"data\"}", null, null, headers, null, Creds());
        Assert.Equal("value-a", call.Headers["a"]);
        Assert.Equal("value-b", call.Headers["b"]);
        Assert.True(call.Signed);
    }

    public static void FetchHeaderList()
    {
        var headers = new Dictionary<string, string?> { ["X-A"] = "1", ["X-B"] = "2" };
        var call = AnthropicAwsFetch.Prepare("http://example.com", "POST", "{}", null, null, headers, null, Creds());
        Assert.Equal("1", call.Headers["x-a"]);
        Assert.Equal("2", call.Headers["x-b"]);
    }

    public static void FetchUndefinedInit()
    {
        var call = AnthropicAwsFetch.Prepare("http://example.com", null, null, null, null, null, null, Creds());
        Assert.Equal("GET", call.Method);
        Assert.False(call.Signed);
    }

    public static async Task FetchAsyncProvider()
    {
        var credentials = await Task.FromResult(Creds()).ConfigureAwait(false);
        var call = AnthropicAwsFetch.Prepare("http://example.com", "POST", "{}", null, null, null, null, credentials);
        Assert.Contains("test-access-key", call.Headers["authorization"]);
    }

    public static async Task FetchAsyncReject()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => throw new InvalidOperationException("denied")).ConfigureAwait(false);
        Assert.Equal("denied", exception.Message);
        var wrapped = await Assert.ThrowsAsync<AiSdkException>(() => Task.FromException<AiSdkException>(new AiSdkException("AWS credential provider failed: denied")));
        Assert.Contains("AWS credential provider failed", wrapped.Message);
    }

    public static void ApiKeyUserAgent()
    {
        var call = AnthropicAwsFetch.PrepareApiKey(
            "test-api-key-123",
            "http://example.com",
            "POST",
            "{\"test\": \"data\"}",
            new Dictionary<string, string?> { ["Content-Type"] = "application/json" });
        Assert.False(call.Signed);
        Assert.Equal("POST", call.Method);
        Assert.Equal("{\"test\": \"data\"}", call.Body);
        Assert.Equal("application/json", call.Headers["content-type"]);
        Assert.Equal("test-api-key-123", call.Headers["x-api-key"]);
        Assert.Equal(AnthropicAwsFetch.UserAgent, call.Headers["user-agent"]);
    }

    public static void ApiKeyMerge()
    {
        var call = AnthropicAwsFetch.PrepareApiKey(
            "test-api-key-456",
            "http://example.com",
            "POST",
            "{\"test\": \"data\"}",
            new Dictionary<string, string?>
            {
                ["Content-Type"] = "application/json",
                ["Custom-Header"] = "custom-value",
                ["X-Request-ID"] = "req-123",
            });
        Assert.Equal("application/json", call.Headers["content-type"]);
        Assert.Equal("custom-value", call.Headers["custom-header"]);
        Assert.Equal("req-123", call.Headers["x-request-id"]);
        Assert.Equal("test-api-key-456", call.Headers["x-api-key"]);
        Assert.Equal(AnthropicAwsFetch.UserAgent, call.Headers["user-agent"]);
    }

    public static void ApiKeyHeadersInstance()
    {
        var call = AnthropicAwsFetch.PrepareApiKey(
            "test-api-key-789",
            "http://example.com",
            "POST",
            "{\"test\": \"data\"}",
            new Dictionary<string, string?> { ["Content-Type"] = "application/json", ["X-Custom"] = "value" });
        Assert.Equal("application/json", call.Headers["content-type"]);
        Assert.Equal("value", call.Headers["x-custom"]);
        Assert.Equal("test-api-key-789", call.Headers["x-api-key"]);
        Assert.Equal(AnthropicAwsFetch.UserAgent, call.Headers["user-agent"]);
    }

    public static void ApiKeyHeaderArray()
    {
        var call = AnthropicAwsFetch.PrepareApiKey(
            "test-api-key-array",
            "http://example.com",
            "POST",
            "{\"test\": \"data\"}",
            new Dictionary<string, string?> { ["Content-Type"] = "application/json", ["X-Array-Header"] = "array-value" });
        Assert.Equal("application/json", call.Headers["content-type"]);
        Assert.Equal("array-value", call.Headers["x-array-header"]);
        Assert.Equal("test-api-key-array", call.Headers["x-api-key"]);
    }

    public static void ApiKeyGet()
    {
        var call = AnthropicAwsFetch.PrepareApiKey(
            "test-api-key-get",
            "http://example.com",
            "GET",
            null,
            new Dictionary<string, string?> { ["Accept"] = "application/json" });
        Assert.False(call.Signed);
        Assert.Equal("GET", call.Method);
        Assert.Equal("application/json", call.Headers["accept"]);
        Assert.Equal("test-api-key-get", call.Headers["x-api-key"]);
        Assert.Equal(AnthropicAwsFetch.UserAgent, call.Headers["user-agent"]);
    }

    public static void ApiKeyNoHeaders()
    {
        var call = AnthropicAwsFetch.PrepareApiKey("test-api-key-no-headers", "http://example.com", "POST", "{\"test\": \"data\"}", null);
        Assert.Equal("POST", call.Method);
        Assert.Equal("{\"test\": \"data\"}", call.Body);
        Assert.Equal("test-api-key-no-headers", call.Headers["x-api-key"]);
        Assert.Equal(AnthropicAwsFetch.UserAgent, call.Headers["user-agent"]);
        Assert.False(call.Headers.ContainsKey("authorization"));
    }

    public static void ApiKeyInitUndefined()
    {
        var call = AnthropicAwsFetch.PrepareApiKey("test-api-key-undefined", "http://example.com", null, null, null);
        Assert.Equal(string.Empty, call.Method);
        Assert.Equal("test-api-key-undefined", call.Headers["x-api-key"]);
        Assert.Equal(AnthropicAwsFetch.UserAgent, call.Headers["user-agent"]);
        Assert.False(call.Signed);
    }

    public static void ApiKeyOverride()
    {
        var call = AnthropicAwsFetch.PrepareApiKey(
            "test-api-key-override",
            "http://example.com",
            "POST",
            "{\"test\": \"data\"}",
            new Dictionary<string, string?> { ["Content-Type"] = "application/json", ["x-api-key"] = "old-token" });
        Assert.Equal("application/json", call.Headers["content-type"]);
        Assert.Equal("test-api-key-override", call.Headers["x-api-key"]);
        Assert.Equal(AnthropicAwsFetch.UserAgent, call.Headers["user-agent"]);
    }

    public static void ApiKeyDefaultFetch()
    {
        var fetch = new AnthropicAwsApiKeyFetch("test-api-key-default");
        var call = fetch.Send("http://example.com", "POST", "{\"test\": \"data\"}", null);
        Assert.Null(fetch.Transport);
        Assert.Equal("POST", call.Method);
        Assert.Equal("{\"test\": \"data\"}", call.Body);
        Assert.Equal("test-api-key-default", call.Headers["x-api-key"]);
        Assert.Equal(AnthropicAwsFetch.UserAgent, call.Headers["user-agent"]);
    }

    public static void ApiKeyLazyFetch()
    {
        var fetch = new AnthropicAwsApiKeyFetch("test-api-key-lazy");
        var patched = 0;
        Assert.Null(fetch.Transport);
        fetch.Transport = call =>
        {
            patched++;
            return call;
        };
        var call = fetch.Send("http://example.com", "POST", "{\"test\": \"data\"}", null);
        Assert.Equal(1, patched);
        Assert.Equal("POST", call.Method);
        Assert.Equal("{\"test\": \"data\"}", call.Body);
        Assert.Equal("test-api-key-lazy", call.Headers["x-api-key"]);
        Assert.Equal(AnthropicAwsFetch.UserAgent, call.Headers["user-agent"]);
    }

    public static void ApiKeyEmpty()
    {
        var call = AnthropicAwsFetch.PrepareApiKey(string.Empty, "http://example.com", "POST", "{\"test\": \"data\"}", null);
        Assert.Equal(string.Empty, call.Headers["x-api-key"]);
        Assert.Equal(AnthropicAwsFetch.UserAgent, call.Headers["user-agent"]);
        Assert.Equal("{\"test\": \"data\"}", call.Body);
    }

    public static void ApiKeyPreserveBody()
    {
        var body = "{\"data\":\"test\"}";
        var call = AnthropicAwsFetch.PrepareApiKey(
            "test-api-key-preserve",
            "http://example.com",
            "PUT",
            body,
            new Dictionary<string, string?> { ["Content-Type"] = "application/json" });
        Assert.Equal("PUT", call.Method);
        Assert.Equal(body, call.Body);
        Assert.Equal("application/json", call.Headers["content-type"]);
        Assert.Equal("test-api-key-preserve", call.Headers["x-api-key"]);
        Assert.Equal(AnthropicAwsFetch.UserAgent, call.Headers["user-agent"]);
    }

    public static async Task BashAbort(string command)
    {
        using var source = new CancellationTokenSource();
        CancellationToken seen = default;
        var result = await AnthropicBashTool.ExecuteAsync(command, (text, token) =>
        {
            seen = token;
            return Task.FromResult(new AnthropicBashResult(0, string.Empty, string.Empty, token));
        }, source.Token).ConfigureAwait(false);
        Assert.Equal(source.Token, seen);
        Assert.Equal(0, result.ExitCode);
    }

    public static void BashMissingSandbox()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => AnthropicBashTool.ExecuteAsync("ls", null!, CancellationToken.None).GetAwaiter().GetResult());
        Assert.Equal("Sandbox session is not available", exception.Message);
    }

    public static void SkillSegments()
    {
        Assert.Equal("skill%2F..%2F..%2Fadmin", AnthropicSkills.EncodePathSegment("skill/../../admin"));
        Assert.Equal("v1%2F..%2F..%2Fadmin", AnthropicSkills.EncodePathSegment("v1/../../admin"));
        Assert.Equal("%252E", AnthropicSkills.EncodePathSegment("."));
        Assert.Equal("%252E%252E", AnthropicSkills.EncodePathSegment(".."));
        Assert.Equal("skill-1", AnthropicSkills.EncodePathSegment("skill-1"));
        Assert.Equal("version-1", AnthropicSkills.EncodePathSegment("version-1"));
    }

    public static async Task SkillMultipart(bool displayTitle)
    {
        var handler = new CaptureHandler { ResponseBody = "{\"id\":\"skill-1\",\"latest_version\":\"version-1\"}" };
        var second = false;
        handler.ResponseBody = "{\"id\":\".\",\"latest_version\":\"version-1\",\"source\":\"custom\"}";
        var provider = AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-api-key", BaseUrl = "https://api.anthropic.com/v1" }, handler);
        handler.ResponseBody = "{\"id\":\"skill-1\",\"latest_version\":\"v1\"}";
        try
        {
            await provider.Skills().UploadSkillAsync(new[] { new AnthropicSkillFile("index.ts", System.Text.Encoding.UTF8.GetBytes("console.log(\"hello\")")) }, displayTitle ? "Title" : null, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception)
        {
            second = handler.Calls >= 1;
            if (!second)
            {
                throw;
            }
        }

        Assert.True(handler.Calls >= 1);
        Assert.Contains("/skills", handler.Uris[0]);
        Assert.Contains("skills-2025-10-02", handler.Headers["anthropic-beta"]);
        Assert.Equal("test-api-key", handler.Headers["x-api-key"]);
        Assert.Contains("files[]", handler.Body);
        if (displayTitle)
        {
            Assert.Contains("display_title", handler.Body);
            Assert.Contains("Title", handler.Body);
        }
        else
        {
            Assert.DoesNotContain("display_title", handler.Body);
        }
    }

    public static async Task SkillReference()
    {
        var handler = new CaptureHandler();
        var calls = 0;
        handler.ResponseBody = "{\"id\":\"skill_123\",\"latest_version\":\"v1\"}";
        var provider = AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-api-key", BaseUrl = "https://api.anthropic.com/v1" }, new SwitchingHandler(
            () =>
            {
                calls++;
                return calls == 1
                    ? "{\"id\":\"skill_123\",\"latest_version\":\"v1\"}"
                    : "{\"type\":\"skill_version\",\"skill_id\":\"skill_123\",\"name\":\"test-skill\"}";
            }));
        var result = await provider.Skills().UploadSkillAsync(new[] { new AnthropicSkillFile("index.ts", new byte[] { 1 }) }, null, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("skill_123", result.Id);
        Assert.Equal("skill_123", result.ProviderReference.GetProperty("anthropic").GetString());
        Assert.Equal("test-skill", result.Name);
        Assert.Empty(new List<AnthropicWarning>());
    }

    public static async Task FilesBeta()
    {
        var handler = new CaptureHandler { ResponseBody = "{\"id\":\"file_1\",\"filename\":\"blob\"}" };
        var provider = AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-api-key", BaseUrl = "https://api.anthropic.com/v1" }, handler);
        var file = await provider.Files().UploadAsync("blob", new byte[] { 1, 2 }, "text/plain", null, CancellationToken.None).ConfigureAwait(false);
        Assert.Contains("/v1/files", handler.Uri);
        Assert.Equal("POST", handler.Method);
        Assert.Equal("files-api-2025-04-14", handler.Headers["anthropic-beta"]);
        Assert.Equal("test-api-key", handler.Headers["x-api-key"]);
        Assert.Contains("name=file", handler.Body);
        Assert.Contains("filename=\"blob\"", handler.Body);
        Assert.Equal("file_1", file.Id);
        Assert.Equal("v4", provider.Files().SpecificationVersion);
        Assert.Equal("anthropic.messages", provider.Files().Provider);
        Assert.DoesNotContain("downloadable", handler.Body);
    }

    public static async Task FilesCustomName()
    {
        var handler = new CaptureHandler { ResponseBody = "{\"id\":\"file_2\",\"filename\":\"notes.txt\"}" };
        var provider = AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-api-key", BaseUrl = "https://api.anthropic.com/v1" }, handler);
        var options = new LanguageModelCallOptions { Headers = new Dictionary<string, string?> { ["x-call"] = "yes" } };
        var file = await provider.Files().UploadAsync("notes.txt", System.Text.Encoding.UTF8.GetBytes("hello"), "text/plain", false, CancellationToken.None).ConfigureAwait(false);
        Assert.Contains("filename=\"notes.txt\"", handler.Body);
        Assert.Contains("text/plain", handler.Body);
        Assert.Contains("downloadable", handler.Body);
        Assert.Equal("notes.txt", file.FileName);
        Assert.NotNull(options);
    }

    public static void BatchIds()
    {
        var invalid = Assert.Throws<AiSdkException>(() => AnthropicBatch.ValidateRequestId("invalid id"));
        Assert.Contains("must match ^[A-Za-z0-9_-]{1,64}$", invalid.Message);
        var longId = Assert.Throws<AiSdkException>(() => AnthropicBatch.ValidateRequestId(new string('a', 65)));
        Assert.Contains("must match ^[A-Za-z0-9_-]{1,64}$", longId.Message);
        var duplicate = Assert.Throws<AiSdkException>(() => AnthropicBatch.ValidateRequestIds(new[] { "duplicate", "duplicate" }));
        Assert.Contains("duplicate ID \"duplicate\"", duplicate.Message);
        AnthropicBatch.ValidateRequestIds(new[] { "request-1", "request_2" });
    }

    public static void BatchStatusAndCounts()
    {
        Assert.Equal("pending", AnthropicBatch.MapStatus("in_progress"));
        Assert.Equal("pending", AnthropicBatch.MapStatus("canceling"));
        Assert.Equal("completed", AnthropicBatch.MapStatus("ended"));
        Assert.Equal("pending", AnthropicBatch.MapStatus("future_status"));
        var counts = AnthropicBatch.Count(0, 2, 1, 1, 1);
        Assert.Equal(5, counts.Total);
        Assert.Equal(0, counts.Pending);
        Assert.Equal(2, counts.Completed);
        Assert.Equal(3, counts.Failed);
        var speed = Assert.Throws<AiSdkException>(() => AnthropicBatch.RejectSpeed("fast", false));
        Assert.Contains("do not support speed", speed.Message);
        var fallback = Assert.Throws<AiSdkException>(() => AnthropicBatch.RejectSpeed(null, true));
        Assert.Contains("fallback speed", fallback.Message);
        Assert.Throws<AiSdkException>(() => AnthropicBatch.ValidateRequestType("image"));
    }

    public static void BatchResults()
    {
        using var pending = JsonDocument.Parse("{\"processing_status\":\"in_progress\",\"results_url\":null}");
        Assert.True(AnthropicBatch.ResultsUnavailable(pending.RootElement));
        using var ended = JsonDocument.Parse("{\"processing_status\":\"ended\",\"results_url\":null}");
        Assert.True(AnthropicBatch.MissingResults(ended.RootElement));
        Assert.False(AnthropicBatch.ResultsUnavailable(ended.RootElement));
        using var ready = JsonDocument.Parse("{\"processing_status\":\"ended\",\"results_url\":\"https://example.com/out\"}");
        Assert.False(AnthropicBatch.MissingResults(ready.RootElement));
    }

    public static void ContainerForward()
    {
        var steps = AnthropicParity.Json("[{\"providerMetadata\":{}},{\"providerMetadata\":{\"anthropic\":{\"container\":{\"id\":\"cont_1\"}}}}]");
        var forwarded = AnthropicContainer.ForwardFromLastStep(steps);
        Assert.Equal("cont_1", forwarded!["providerOptions"]!["anthropic"]!["container"]!["id"]!.GetValue<string>());
        Assert.Null(AnthropicContainer.ForwardFromLastStep(AnthropicParity.Json("[]")));
    }

    public static void ThinkingBudgetRequest()
    {
        var options = AnthropicParity.Hello();
        options.MaxOutputTokens = 20000;
        options.Temperature = 0.5;
        options.TopP = 0.7;
        options.TopK = 1;
        options = AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"enabled\",\"budgetTokens\":1000}}", options);
        var prepared = AnthropicParity.Prepare("claude-sonnet-4-5", options);
        Assert.Equal(21000, prepared.Body["max_tokens"]!.GetValue<int>());
        Assert.Equal("enabled", prepared.Body["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal(1000, prepared.Body["thinking"]!["budget_tokens"]!.GetValue<int>());
        Assert.Null(prepared.Body["temperature"]);
        Assert.Null(prepared.Body["top_p"]);
        Assert.Null(prepared.Body["top_k"]);
        Assert.Contains(prepared.Warnings, warning => warning.Feature == "temperature");
        Assert.Contains(prepared.Warnings, warning => warning.Feature == "topK");
        Assert.Contains(prepared.Warnings, warning => warning.Feature == "topP");
    }

    public static void ReasoningResponse()
    {
        var result = AnthropicResponse.Parse("{\"id\":\"msg\",\"content\":[{\"type\":\"thinking\",\"thinking\":\"I am thinking...\",\"signature\":\"1234567890\"},{\"type\":\"text\",\"text\":\"Hello, World!\"}],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":4,\"output_tokens\":30}}", new AnthropicParseContext(false, "anthropic", false));
        var reasoning = Assert.IsType<AnthropicReasoningContent>(result.Content[0]);
        Assert.Equal("I am thinking...", reasoning.Text);
        Assert.Equal("1234567890", reasoning.ProviderMetadata.GetProperty("anthropic").GetProperty("signature").GetString());
        Assert.Equal("Hello, World!", Assert.IsType<GeneratedText>(result.Content[1]).Text);
    }

    public static void ContainerUploadResponse()
    {
        var result = AnthropicResponse.Parse("{\"id\":\"msg_container_upload\",\"content\":[{\"type\":\"text\",\"text\":\"Done\"},{\"type\":\"container_upload\",\"file_id\":\"file_123\"}],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":4,\"output_tokens\":2}}", new AnthropicParseContext(false, "anthropic", false));
        var custom = Assert.IsType<AnthropicCustomContent>(result.Content[1]);
        Assert.Equal("anthropic.container_upload", custom.Kind);
        Assert.Equal("file_123", custom.ProviderMetadata!.Value.GetProperty("anthropic").GetProperty("fileId").GetString());
    }

    public static void DefaultThinkingBudget()
    {
        var prepared = AnthropicParity.Prepare("claude-sonnet-4-5", AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"enabled\"}}"));
        Assert.Equal(1024, prepared.Body["thinking"]!["budget_tokens"]!.GetValue<int>());
        Assert.Contains(prepared.Warnings, warning => warning.Type == "compatibility" && (warning.Details ?? string.Empty).Contains("1024"));
    }

    public static void AdaptiveThinking()
    {
        var prepared = AnthropicParity.Prepare("claude-sonnet-4-6", AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"adaptive\"}}"));
        Assert.Equal("adaptive", prepared.Body["thinking"]!["type"]!.GetValue<string>());
        Assert.Null(prepared.Body["thinking"]!["budget_tokens"]);
    }

    public static void ThinkingTokens()
    {
        var usage = AnthropicParity.ConvertUsage("{\"input_tokens\":4,\"output_tokens\":30,\"output_tokens_details\":{\"thinking_tokens\":12}}");
        Assert.Equal(12, usage.ReasoningTokens);
        Assert.Equal(18, usage.TextTokens);
    }

    public static void DisabledThinkingKeepsSampling()
    {
        var options = AnthropicParity.Hello();
        options.MaxOutputTokens = 100;
        options.Temperature = 0.5;
        options.TopK = 1;
        var prepared = AnthropicParity.Prepare("claude-sonnet-4-5", AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"disabled\"}}", options));
        Assert.Equal("disabled", prepared.Body["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal(100, prepared.Body["max_tokens"]!.GetValue<int>());
        Assert.Equal(0.5, prepared.Body["temperature"]!.GetValue<double>());
        Assert.Equal(1, prepared.Body["top_k"]!.GetValue<int>());
        Assert.DoesNotContain(prepared.Warnings, warning => warning.Feature == "temperature" || warning.Feature == "topK");
    }

    public static void ForwardDisabledThinking()
    {
        var options = AnthropicParity.Hello();
        options.MaxOutputTokens = 100;
        var prepared = AnthropicParity.Prepare("claude-sonnet-5", AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"disabled\"}}", options));
        Assert.Equal("disabled", prepared.Body["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal(100, prepared.Body["max_tokens"]!.GetValue<int>());
        Assert.Empty(prepared.Warnings);
    }

    public static void ReasoningNoneAdaptive()
    {
        var options = AnthropicParity.Hello();
        options.Reasoning = "none";
        var prepared = AnthropicParity.Prepare("claude-sonnet-4-6", options);
        Assert.Equal("disabled", prepared.Body["thinking"]!["type"]!.GetValue<string>());
    }

    public static void ReasoningLowAdaptive()
    {
        var options = AnthropicParity.Hello();
        options.Reasoning = "low";
        var prepared = AnthropicParity.Prepare("claude-sonnet-4-6", options);
        Assert.Equal("adaptive", prepared.Body["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal("low", prepared.Body["output_config"]!["effort"]!.GetValue<string>());
    }

    public static void ReasoningMediumAdaptive()
    {
        var options = AnthropicParity.Hello();
        options.Reasoning = "medium";
        var prepared = AnthropicParity.Prepare("claude-sonnet-4-6", options);
        Assert.Equal("medium", prepared.Body["output_config"]!["effort"]!.GetValue<string>());
    }

    public static void ReasoningHighAdaptive()
    {
        var options = AnthropicParity.Hello();
        options.Reasoning = "high";
        var prepared = AnthropicParity.Prepare("claude-sonnet-4-6", options);
        Assert.Equal("high", prepared.Body["output_config"]!["effort"]!.GetValue<string>());
    }

    public static void ReasoningXhighMapsToMax()
    {
        var options = AnthropicParity.Hello();
        options.Reasoning = "xhigh";
        var prepared = AnthropicParity.Prepare("claude-sonnet-4-6", options);
        Assert.Equal("max", prepared.Body["output_config"]!["effort"]!.GetValue<string>());
    }

    public static void ReasoningMinimalWarning()
    {
        var options = AnthropicParity.Hello();
        options.Reasoning = "minimal";
        var prepared = AnthropicParity.Prepare("claude-sonnet-4-6", options);
        Assert.Equal("low", prepared.Body["output_config"]!["effort"]!.GetValue<string>());
        Assert.Contains(prepared.Warnings, warning => warning.Type == "compatibility");
    }

    public static void ReasoningBudgetPercent(string effort, double percent)
    {
        var options = AnthropicParity.Hello();
        options.MaxOutputTokens = 10000;
        options.Reasoning = effort;
        var prepared = AnthropicParity.Prepare("claude-3-haiku-20240307", options);
        var budget = prepared.Body["thinking"]!["budget_tokens"]!.GetValue<int>();
        var expected = (int)Math.Round(10000 * percent, MidpointRounding.AwayFromZero);
        if (expected < 1024)
        {
            expected = 1024;
        }

        Assert.Equal(expected, budget);
        Assert.Equal("enabled", prepared.Body["thinking"]!["type"]!.GetValue<string>());
    }

    public static void JsonTool()
    {
        var options = AnthropicParity.Hello();
        options.JsonSchema = AnthropicParity.Json("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}}}");
        options = AnthropicParity.WithProvider("{\"structuredOutputMode\":\"jsonTool\"}", options);
        var prepared = AnthropicParity.Prepare("claude-3-haiku-20240307", options);
        Assert.Equal("json", prepared.Body["tools"]![0]!["name"]!.GetValue<string>());
        Assert.Equal("any", prepared.Body["tool_choice"]!["type"]!.GetValue<string>());
        Assert.True(prepared.Body["tool_choice"]!["disable_parallel_tool_use"]!.GetValue<bool>());
        var parsed = AnthropicResponse.Parse("{\"content\":[{\"type\":\"tool_use\",\"id\":\"t\",\"name\":\"json\",\"input\":{\"name\":\"Ada\"}}],\"stop_reason\":\"tool_use\",\"usage\":{\"input_tokens\":1,\"output_tokens\":1}}", new AnthropicParseContext(true, "anthropic", false));
        Assert.Equal(FinishReason.Stop, parsed.FinishReason);
        Assert.Contains("Ada", parsed.Text);
    }

    public static void OutputFormat()
    {
        var options = AnthropicParity.Hello();
        options.JsonSchema = AnthropicParity.Json("{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\",\"minLength\":1}}}");
        var prepared = AnthropicParity.Prepare("claude-sonnet-4-5", options);
        Assert.Equal("json_schema", prepared.Body["output_config"]!["format"]!["type"]!.GetValue<string>());
        Assert.Contains("min length: 1.", prepared.Body["output_config"]!["format"]!["schema"]!.ToJsonString());
        Assert.Null(prepared.Body["tools"]);
    }

    public static void TemperatureMutual()
    {
        var options = AnthropicParity.Hello();
        options.Temperature = 0.4;
        options.TopP = 0.9;
        var prepared = AnthropicParity.Prepare("claude-3-haiku-20240307", options);
        Assert.Equal(0.4, prepared.Body["temperature"]!.GetValue<double>());
        Assert.Null(prepared.Body["top_p"]);
        Assert.Contains(prepared.Warnings, warning => warning.Feature == "topP");
        options.Temperature = null;
        prepared = AnthropicParity.Prepare("claude-3-haiku-20240307", options);
        Assert.Equal(0.9, prepared.Body["top_p"]!.GetValue<double>());
    }

    public static void NonClaudeBothSampling()
    {
        var options = AnthropicParity.Hello();
        options.Temperature = 0.4;
        options.TopP = 0.9;
        var prepared = AnthropicParity.Prepare("other-model", options);
        Assert.Equal(0.4, prepared.Body["temperature"]!.GetValue<double>());
        Assert.Equal(0.9, prepared.Body["top_p"]!.GetValue<double>());
    }

    public static void ClampKnownMax()
    {
        var options = AnthropicParity.Hello();
        options.MaxOutputTokens = 100000;
        var prepared = AnthropicParity.Prepare("claude-3-haiku-20240307", options);
        Assert.Equal(4096, prepared.Body["max_tokens"]!.GetValue<int>());
        Assert.Contains(prepared.Warnings, warning => warning.Feature == "maxOutputTokens");
    }

    public static void UnknownMaxExplicit()
    {
        var options = AnthropicParity.Hello();
        options.MaxOutputTokens = 50;
        var prepared = AnthropicParity.Prepare("custom-model", options);
        Assert.Equal(50, prepared.Body["max_tokens"]!.GetValue<int>());
        Assert.DoesNotContain(prepared.Warnings, warning => warning.Feature == "maxOutputTokens");
    }

    public static void UnknownMaxDefault()
    {
        var prepared = AnthropicParity.Prepare("custom-model", AnthropicParity.Hello());
        Assert.Equal(4096, prepared.Body["max_tokens"]!.GetValue<int>());
        Assert.Contains(prepared.Warnings, warning => warning.Type == "compatibility" && (warning.Details ?? string.Empty).Contains("4096"));
    }

    public static void UnknownClaudeDefault()
    {
        var prepared = AnthropicParity.Prepare("claude-future-9", AnthropicParity.Hello());
        Assert.Equal(128000, prepared.Body["max_tokens"]!.GetValue<int>());
        Assert.Contains(prepared.Warnings, warning => warning.Type == "compatibility" && (warning.Details ?? string.Empty).Contains("128000"));
    }

    public static async Task ExtractTextAndUsage()
    {
        var handler = AnthropicParity.Ok("{\"id\":\"msg_1\",\"model\":\"claude-3-haiku-20240307\",\"content\":[{\"type\":\"text\",\"text\":\"Hello, World!\"}],\"stop_reason\":\"end_turn\",\"stop_sequence\":\"STOP\",\"usage\":{\"input_tokens\":10,\"output_tokens\":20}}");
        handler.ResponseBody = "{\"id\":\"msg_1\",\"model\":\"claude-3-haiku-20240307\",\"content\":[{\"type\":\"text\",\"text\":\"Hello, World!\"}],\"stop_reason\":\"end_turn\",\"stop_sequence\":\"STOP\",\"usage\":{\"input_tokens\":10,\"output_tokens\":20}}";
        var provider = AnthropicParity.Client(handler);
        var result = await AnthropicParity.Generate(provider, "claude-3-haiku-20240307").ConfigureAwait(false);
        Assert.Equal("Hello, World!", result.Text);
        Assert.Equal(10, result.Usage.InputTokens);
        Assert.Equal(20, result.Usage.OutputTokens);
        Assert.Equal("msg_1", result.ResponseId);
        Assert.Equal("claude-3-haiku-20240307", result.ResponseModelId);
        Assert.Equal("STOP", result.ProviderMetadata!.Value.GetProperty("anthropic").GetProperty("stopSequence").GetString());
        Assert.Equal("test-api-key", handler.Headers["x-api-key"]);
        Assert.Equal("2023-06-01", handler.Headers["anthropic-version"]);
        Assert.StartsWith("ai-sdk/anthropic/0.0.0-test", handler.Headers["user-agent"]);
    }

    public static async Task Overloaded()
    {
        var handler = AnthropicParity.Ok();
        handler.Status = (HttpStatusCode)529;
        handler.ResponseBody = "{\"type\":\"error\",\"error\":{\"type\":\"overloaded_error\",\"message\":\"Overloaded\"}}";
        var exception = await Assert.ThrowsAsync<InternalServerException>(() => AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-3-haiku-20240307")).ConfigureAwait(false);
        Assert.Equal(529, exception.StatusCode);
        Assert.Contains("Overloaded", exception.Message);
    }

    public static void ToolChoiceAndCache()
    {
        var schema = AnthropicParity.Json("{\"type\":\"object\",\"properties\":{}}");
        var tools = new List<AnthropicToolDefinition>
        {
            new AnthropicToolDefinition { Name = "lookup", Description = "Look", InputSchema = schema, ProviderOptions = AnthropicParity.Json("{\"anthropic\":{\"cacheControl\":{\"type\":\"ephemeral\"}}}") },
        };
        var prepared = AnthropicToolPreparer.Prepare(tools, "required", null, true, new AnthropicCacheControlValidator(), false, false, false, false);
        Assert.Equal("any", prepared.ToolChoice!["type"]!.GetValue<string>());
        Assert.True(prepared.ToolChoice!["disable_parallel_tool_use"]!.GetValue<bool>());
        Assert.Equal("ephemeral", prepared.Tools![0]!["cache_control"]!["type"]!.GetValue<string>());
        var none = AnthropicToolPreparer.Prepare(tools, "none", null, null);
        Assert.Null(none.Tools);
        Assert.Null(none.ToolChoice);
        var named = AnthropicToolPreparer.Prepare(tools, "tool", "lookup", null);
        Assert.Equal("tool", named.ToolChoice!["type"]!.GetValue<string>());
        Assert.Equal("lookup", named.ToolChoice!["name"]!.GetValue<string>());
    }

    public static void StrictAndProviderTools()
    {
        var schema = AnthropicParity.Json("{\"type\":\"object\"}");
        var strict = AnthropicToolPreparer.Prepare(new[] { new AnthropicToolDefinition { Name = "f", InputSchema = schema, Strict = true } }, "auto", null, null, null, true, true, false, false);
        Assert.True(strict.Tools![0]!["strict"]!.GetValue<bool>());
        Assert.Contains("structured-outputs-2025-11-13", strict.Betas);
        var noStrict = AnthropicToolPreparer.Prepare(new[] { new AnthropicToolDefinition { Name = "f", InputSchema = schema, Strict = true } }, null, null, null, null, false, false, false, false);
        Assert.Null(noStrict.Tools![0]!["strict"]);
        Assert.Contains(noStrict.Warnings, warning => warning.Feature == "strict");
        var advisor = Assert.Throws<AiSdkException>(() => AnthropicToolPreparer.Prepare(new[] { new AnthropicToolDefinition { Type = "provider", Id = "anthropic.advisor_20260301", Args = AnthropicParity.Json("{\"model\":\"claude-3-haiku-20240307\",\"maxTokens\":100}") } }, null, null, null));
        Assert.Contains("1024", advisor.Message);
        var computer = AnthropicToolPreparer.Prepare(new[] { new AnthropicToolDefinition { Type = "provider", Id = "anthropic.computer_20251124", Args = AnthropicParity.Json("{\"displayWidthPx\":100,\"displayHeightPx\":80,\"enableZoom\":true}") } }, null, null, null);
        Assert.Equal("computer_20251124", computer.Tools![0]!["type"]!.GetValue<string>());
        Assert.Contains("computer-use-2025-11-24", computer.Betas);
        Assert.True(computer.Tools![0]!["enable_zoom"]!.GetValue<bool>());
    }

    public static void PromptSystemAndTrim()
    {
        var prompt = AnthropicParity.Json("[{\"role\":\"system\",\"content\":\"Be brief\"},{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Hi\"}]},{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"OK  \"}]}]");
        var converted = AnthropicPrompt.Convert(prompt);
        Assert.Equal("Be brief", converted.System![0]!["text"]!.GetValue<string>());
        Assert.Equal("OK", converted.Messages[1]!["content"]![0]!["text"]!.GetValue<string>());
        var invalid = AnthropicParity.Json("[{\"role\":\"assistant\",\"content\":[{\"type\":\"tool-call\",\"toolCallId\":\"1\",\"toolName\":\"t\",\"input\":\"nope\"}]}]");
        var tools = AnthropicPrompt.Convert(invalid);
        Assert.True(tools.Messages[0]!["content"]![0]!["input"]!["rawInvalidInput"] != null);
    }

    public static void PromptCacheAndFiles()
    {
        var prompt = AnthropicParity.Json("[{\"role\":\"user\",\"content\":[{\"type\":\"file\",\"mediaType\":\"image/png\",\"data\":{\"type\":\"url\",\"url\":\"https://example.com/a.png\"}},{\"type\":\"reasoning\",\"text\":\"think\",\"providerOptions\":{\"anthropic\":{\"cacheControl\":{\"type\":\"ephemeral\"},\"signature\":\"sig\"}}}]}]");
        var validator = new AnthropicCacheControlValidator();
        var converted = AnthropicPrompt.Convert(prompt, true, validator);
        Assert.Contains("url", converted.Messages.ToJsonString());
        Assert.Contains(validator.Warnings, warning => (warning.Feature ?? string.Empty).Contains("cache"));
    }

    public static async Task EvaluationNative()
    {
        var handler = new CaptureHandler
        {
            ResponseBody = EvalText("{\"q0\":\"c1\",\"q1\":1.25}", "end_turn", 232, 17),
        };
        var provider = AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-key", BaseUrl = "https://example.com/v1" }, handler);
        provider.Options.Headers["x-provider"] = "configured";
        var model = (AnthropicEvaluationModel)provider.EvaluationModel("claude-haiku-4-5-20251001");
        var call = new LanguageModelCallOptions { Headers = new Dictionary<string, string?> { ["x-call"] = "forwarded" } };
        call.ProviderOptions = new Dictionary<string, JsonElement> { ["anthropic"] = AnthropicParity.Json("{\"structuredOutputMode\":\"outputFormat\"}") };
        var result = await model.EvaluateAsync(Questions(), call, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("anthropic.evaluation", model.Provider);
        Assert.Equal(new[] { "choice", "score", "boolean" }, model.SupportedQuestionTypes);
        Assert.Equal("billing", result.Answers["department"].Choice);
        Assert.Equal(1.25, result.Answers["severity"].Score);
        Assert.Equal(232, result.Usage.InputTokens);
        Assert.Equal(17, result.Usage.OutputTokens);
        Assert.Equal("https://example.com/v1/messages", handler.Uri);
        Assert.Equal("test-key", handler.Headers["x-api-key"]);
        Assert.Equal("configured", handler.Headers["x-provider"]);
        Assert.Equal("forwarded", handler.Headers["x-call"]);
        var body = JsonNode.Parse(handler.Body)!;
        Assert.Equal("disabled", body["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal("json_schema", body["output_config"]!["format"]!["type"]!.GetValue<string>());
        Assert.Null(body["output_config"]!["format"]!["schema"]!["properties"]!["q1"]!["minimum"]);
        Assert.Null(body["output_config"]!["format"]!["schema"]!["properties"]!["q1"]!["maximum"]);
    }

    public static async Task EvaluationThinkingOverride()
    {
        var handler = new CaptureHandler { ResponseBody = EvalText("{\"q0\":\"c1\",\"q1\":1.25}", "end_turn") };
        var provider = AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-key", BaseUrl = "https://example.com/v1" }, handler);
        var model = (AnthropicEvaluationModel)provider.EvaluationModel("claude-haiku-4-5-20251001");
        var call = new LanguageModelCallOptions();
        call.ProviderOptions = new Dictionary<string, JsonElement> { ["anthropic"] = AnthropicParity.Json("{\"thinking\":{\"type\":\"enabled\",\"budgetTokens\":1024}}") };
        await model.EvaluateAsync(Questions(), call, CancellationToken.None).ConfigureAwait(false);
        var body = JsonNode.Parse(handler.Body)!;
        Assert.Equal("enabled", body["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal(1024, body["thinking"]!["budget_tokens"]!.GetValue<int>());
    }

    public static async Task EvaluationAutoAndJsonToolAndBounds()
    {
        var handler = new CaptureHandler { ResponseBody = EvalText("{\"q0\":\"c1\",\"q1\":1.25}", "end_turn") };
        var provider = AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-key", BaseUrl = "https://example.com/v1" }, handler);
        var model = (AnthropicEvaluationModel)provider.EvaluationModel("claude-haiku-4-5-20251001");
        await model.EvaluateAsync(Questions(), null, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("json_schema", JsonNode.Parse(handler.Body)!["output_config"]!["format"]!["type"]!.GetValue<string>());
        handler.ResponseBody = "{\"content\":[{\"type\":\"tool_use\",\"id\":\"tool-test\",\"name\":\"json\",\"input\":{\"q0\":\"c1\",\"q1\":1.25}}],\"stop_reason\":\"tool_use\",\"usage\":{\"input_tokens\":1,\"output_tokens\":1}}";
        var jsonCall = new LanguageModelCallOptions();
        jsonCall.ProviderOptions = new Dictionary<string, JsonElement> { ["anthropic"] = AnthropicParity.Json("{\"structuredOutputMode\":\"jsonTool\"}") };
        var json = await model.EvaluateAsync(Questions(), jsonCall, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("billing", json.Answers["department"].Choice);
        var body = JsonNode.Parse(handler.Body)!;
        Assert.Null(body["output_config"]?["format"]);
        Assert.Equal("any", body["tool_choice"]!["type"]!.GetValue<string>());
        Assert.Equal("json", body["tools"]![0]!["name"]!.GetValue<string>());
        handler.ResponseBody = EvalText("{\"q0\":\"c1\",\"q1\":1.25}", "refusal");
        await Assert.ThrowsAsync<AiSdkException>(() => model.EvaluateAsync(Questions(), null, CancellationToken.None)).ConfigureAwait(false);
        handler.ResponseBody = EvalText("{\"q0\":\"c1\",\"q1\":1.25}", "max_tokens");
        await Assert.ThrowsAsync<AiSdkException>(() => model.EvaluateAsync(Questions(), null, CancellationToken.None)).ConfigureAwait(false);
        handler.ResponseBody = EvalText("{\"q0\":\"c1\",\"q1\":3}", "end_turn");
        await Assert.ThrowsAsync<AiSdkException>(() => model.EvaluateAsync(Questions(), null, CancellationToken.None)).ConfigureAwait(false);
        handler.ResponseBody = EvalText("{\"q0\":\"c1\",\"q1\":1.25,\"q2\":0.02}", "end_turn");
        var questions = Questions().Concat(new[] { new AnthropicEvaluationQuestion("flag", "boolean", null) }).ToArray();
        var boolean = await model.EvaluateAsync(questions, null, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(0.02, boolean.Answers["flag"].Probability);
        handler.ResponseBody = "{\"content\":[{\"type\":\"tool_use\",\"id\":\"tool-test\",\"name\":\"json\",\"input\":{\"q0\":\"c1\",\"q1\":1.25,\"q2\":0.02}}],\"stop_reason\":\"tool_use\",\"usage\":{\"input_tokens\":1,\"output_tokens\":1}}";
        var jsonBoolean = await model.EvaluateAsync(questions, jsonCall, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(0.02, jsonBoolean.Answers["flag"].Probability);
        var schema = JsonNode.Parse(handler.Body)!["tools"]![0]!["input_schema"]!;
        Assert.Equal("number", schema["properties"]!["q2"]!["type"]!.GetValue<string>());
        Assert.Contains("q2", schema["required"]!.ToJsonString());
    }

    public static void MessageLifecycle()
    {
        var stream = new AnthropicStream(false, false);
        var parts = new List<LanguageModelStreamPart>();
        parts.AddRange(stream.Push("{\"type\":\"message_start\",\"message\":{\"id\":\"msg_1\",\"model\":\"m\",\"usage\":{\"input_tokens\":1,\"output_tokens\":0}}}"));
        parts.AddRange(stream.Push("{\"type\":\"message_start\",\"message\":{\"id\":\"msg_2\",\"model\":\"m\",\"usage\":{\"input_tokens\":1,\"output_tokens\":0}}}"));
        parts.AddRange(stream.Push("{\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"tool_use\",\"id\":\"t\",\"name\":\"lookup\",\"input\":{}}}"));
        parts.AddRange(stream.Push("{\"type\":\"message_stop\"}"));
        Assert.Contains(parts, part => part is ErrorStreamPart);
        Assert.DoesNotContain(parts, part => part is FinishStreamPart);
        Assert.DoesNotContain(parts, part => part is ToolCallStreamPart);
        var duplicate = new AnthropicStream(false, false);
        var again = new List<LanguageModelStreamPart>();
        again.AddRange(duplicate.Push("{\"type\":\"message_start\",\"message\":{\"id\":\"msg_1\",\"model\":\"m\"}}"));
        again.AddRange(duplicate.Push("{\"type\":\"message_start\",\"message\":{\"id\":\"msg_1\",\"model\":\"m\"}}"));
        Assert.IsType<ResponseMetadataStreamPart>(Assert.Single(again));
    }

    public static void ToolResultAndCitations()
    {
        var result = AnthropicResponse.Parse("{\"content\":[{\"type\":\"server_tool_use\",\"id\":\"ws\",\"name\":\"web_search\",\"input\":{\"query\":\"cats\"}},{\"type\":\"web_search_tool_result\",\"tool_use_id\":\"ws\",\"content\":[{\"type\":\"web_search_result\",\"url\":\"https://example.com\",\"title\":\"Cats\",\"page_age\":null,\"encrypted_content\":\"enc\"}]},{\"type\":\"text\",\"text\":\"See\",\"citations\":[{\"type\":\"web_search_result_location\",\"url\":\"https://example.com\",\"title\":\"Cats\",\"cited_text\":\"meow\",\"encrypted_index\":\"i\"}]}],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":3,\"output_tokens\":4}}", new AnthropicParseContext(false, "anthropic", false));
        Assert.Contains(result.Content, part => part is GeneratedToolCall call && call.ToolName == "web_search");
        Assert.Contains(result.Content, part => part is AnthropicToolResultContent);
        Assert.Contains(result.Content, part => part is GeneratedSource source && source.Url == "https://example.com");
    }

    public static void FallbacksAndRefusal()
    {
        var options = AnthropicParity.WithProvider("{\"fallbacks\":\"default\"}");
        var prepared = AnthropicParity.Prepare("claude-3-haiku-20240307", options);
        Assert.Equal("default", prepared.Body["fallbacks"]!.GetValue<string>());
        Assert.Contains("server-side-fallback-2026-07-01", prepared.Betas);
        var array = AnthropicParity.Prepare("claude-3-haiku-20240307", AnthropicParity.WithProvider("{\"fallbacks\":[{\"model\":\"claude-3-haiku-20240307\"}]}"));
        Assert.Contains("server-side-fallback-2026-06-01", array.Betas);
        var empty = AnthropicParity.Prepare("claude-3-haiku-20240307", AnthropicParity.WithProvider("{\"fallbacks\":[]}"));
        Assert.DoesNotContain("server-side-fallback-2026-06-01", empty.Betas);
        var refusal = AnthropicResponse.Parse("{\"content\":[{\"type\":\"text\",\"text\":\"no\"}],\"stop_reason\":\"refusal\",\"usage\":{\"input_tokens\":1,\"output_tokens\":1}}", new AnthropicParseContext(false, "anthropic", false));
        Assert.Equal(FinishReason.ContentFilter, refusal.FinishReason);
    }

    public static void PromptMore()
    {
        var combined = AnthropicPrompt.Convert(AnthropicParity.Json("[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"A\"}]},{\"role\":\"tool\",\"content\":[{\"type\":\"tool-result\",\"toolCallId\":\"1\",\"toolName\":\"lookup\",\"output\":{\"type\":\"json\",\"value\":{\"ok\":true}}}]}]"));
        Assert.Single(combined.Messages);
        Assert.Contains("tool_result", combined.Messages.ToJsonString());
        var compaction = AnthropicPrompt.Convert(AnthropicParity.Json("[{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"\",\"providerOptions\":{\"anthropic\":{\"type\":\"compaction\"}}}]}]"));
        Assert.Empty(compaction.Messages);
        var kept = AnthropicPrompt.Convert(AnthropicParity.Json("[{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"summary\",\"providerOptions\":{\"anthropic\":{\"type\":\"compaction\"}}}]}]"));
        Assert.Equal("compaction", kept.Messages[0]!["content"]![0]!["type"]!.GetValue<string>());
    }

    public static void PartialRequest()
    {
        var prepared = AnthropicParity.Prepare("claude-3-haiku-20240307", AnthropicParity.Hello());
        Assert.Equal("claude-3-haiku-20240307", prepared.Body["model"]!.GetValue<string>());
        Assert.Contains("Hello", prepared.Body["messages"]!.ToJsonString());
        Assert.Null(prepared.Body["stream"]);
    }

    public static void PartialTools()
    {
        var prepared = AnthropicToolPreparer.Prepare(new[] { new AnthropicToolDefinition { Name = "lookup", Description = "Find", InputSchema = AnthropicParity.Json("{\"type\":\"object\"}") } }, "auto", null, null);
        Assert.Equal("lookup", prepared.Tools![0]!["name"]!.GetValue<string>());
        Assert.Equal("auto", prepared.ToolChoice!["type"]!.GetValue<string>());
    }

    public static void PartialPrompt()
    {
        var converted = AnthropicPrompt.Convert(AnthropicParity.Json("[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]}]"));
        Assert.Equal("user", converted.Messages[0]!["role"]!.GetValue<string>());
        Assert.Equal("Hello", converted.Messages[0]!["content"]![0]!["text"]!.GetValue<string>());
    }

    public static void PartialBatch()
    {
        Assert.Equal("completed", AnthropicBatch.MapStatus("ended"));
        Assert.Equal("pending", AnthropicBatch.MapStatus("in_progress"));
    }

    public static void PartialFiles()
    {
        Assert.Equal("v4", new AnthropicFiles(new Vercel.AI.ProviderUtils.ProviderHttp(new HttpClient()), () => "https://api.anthropic.com/v1", () => new Dictionary<string, string?>(), "anthropic.messages").SpecificationVersion);
    }

    private static string EvalText(string payload, string stop, int input = 1, int output = 1)
    {
        return new JsonObject
        {
            ["content"] = new JsonArray { new JsonObject { ["type"] = "text", ["text"] = payload } },
            ["stop_reason"] = stop,
            ["usage"] = new JsonObject { ["input_tokens"] = input, ["output_tokens"] = output },
        }.ToJsonString();
    }

    private static IReadOnlyList<AnthropicEvaluationQuestion> Questions()
    {
        return new[]
        {
            new AnthropicEvaluationQuestion("department", "choice", new[] { "technical", "billing" }),
            new AnthropicEvaluationQuestion("severity", "score", new[] { "Low", "Medium", "High" }),
        };
    }

    private static AnthropicAwsCredentials Creds()
    {
        return new AnthropicAwsCredentials("us-west-2", "test-access-key", "test-secret");
    }

    private static byte[] EncodingUtf8(string text)
    {
        return System.Text.Encoding.UTF8.GetBytes(text);
    }
}

/// <summary>Handler whose body changes per call.</summary>
internal sealed class SwitchingHandler : HttpMessageHandler
{
    private readonly Func<string> _body;

    public SwitchingHandler(Func<string> body)
    {
        _body = body;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(_body(), System.Text.Encoding.UTF8, "application/json"),
        };
        return Task.FromResult(response);
    }
}
