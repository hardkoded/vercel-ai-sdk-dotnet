// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

internal static class ToolLoopAgentInstructions
{
    public static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }

    public static IReadOnlyDictionary<string, JsonElement> Options(string value)
    {
        return new Dictionary<string, JsonElement>
        {
            ["test"] = JsonSerializer.SerializeToElement(new Dictionary<string, string> { ["value"] = value }),
        };
    }

    public static void AssertSystem(ModelMessage message, string content, string? providerOptionValue)
    {
        var system = Assert.IsType<SystemModelMessage>(message);
        Assert.Equal(content, system.Content);
        if (providerOptionValue == null)
        {
            Assert.Null(system.ProviderOptions);
            return;
        }

        var options = Assert.IsAssignableFrom<IReadOnlyDictionary<string, JsonElement>>(system.ProviderOptions);
        Assert.Equal(providerOptionValue, options["test"].GetProperty("value").GetString());
    }

    public static void AssertUser(ModelMessage message)
    {
        var user = Assert.IsType<UserModelMessage>(message);
        Assert.Equal("Hello, world!", Assert.IsType<TextContentPart>(Assert.Single(user.Content)).Text);
    }
}
