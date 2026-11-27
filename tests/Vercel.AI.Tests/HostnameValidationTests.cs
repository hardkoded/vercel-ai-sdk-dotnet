// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AmazonBedrock;
using Vercel.AI.Google;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

public sealed class IsValidHostnamePartTests
{
    [Theory]
    [MemberData(nameof(AcceptsCases))]
    public void Accepts(string value)
    {
        Assert.True(HostnameParts.IsValidHostnamePart(value));
    }

    [Theory]
    [MemberData(nameof(RejectsCases))]
    public void Rejects(string value)
    {
        Assert.False(HostnameParts.IsValidHostnamePart(value));
    }

    public static IEnumerable<object[]> AcceptsCases()
    {
        yield return Row("a");
        yield return Row("0");
        yield return Row("us-east-1");
        yield return Row("MY-resource");
        yield return Row("global");
        yield return Row(new string('a', 63));
    }

    public static IEnumerable<object[]> RejectsCases()
    {
        yield return Row(string.Empty);
        yield return Row(new string('a', 64));
        yield return Row("-a");
        yield return Row("a-");
        yield return Row("a.b");
        yield return Row("a_b");
        yield return Row("é");
        yield return Row("user@internal:8080/#");
        yield return Row("evil.example.com/#");
        yield return Row("169.254.169.254:80/x#");
        yield return Row("us-east-1/../..");
        yield return Row("us east 1");
        yield return Row("a\n");
        yield return Row("a\r");
        yield return Row("a\t");
        yield return Row("a\0");
        yield return Row("%61");
    }

    private static object[] Row(string value)
    {
        return new object[] { value };
    }
}

/// <summary>Amazon Bedrock runtime region validation.</summary>
public sealed class BedrockRuntimeRegionValidationTests
{
    [Theory]
    [InlineData("evil.example.com/#")]
    [InlineData("user@internal:8080/#")]
    [InlineData("us-east-1/../..")]
    [InlineData("us east 1")]
    [InlineData("")]
    [InlineData("region\n")]
    public async Task RejectsBeforeFetching(string region)
    {
        var handler = new ScriptedHandler();
        var provider = AmazonBedrockProvider.Create(
            new AmazonBedrockOptions
            {
                Region = region,
                AccessKeyId = "AKIA",
                SecretAccessKey = "secret",
                UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
            },
            handler
        );
        var error = await Assert.ThrowsAsync<ArgumentException>(() =>
            provider.LanguageModel("m").DoGenerateAsync(Prompt(), CancellationToken.None)
        );
        Assert.Contains("AWS region must be a single DNS label", error.Message);
        Assert.Equal(0, handler.Calls);
    }

    private static LanguageModelCallOptions Prompt()
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("hi") },
        };
    }
}
