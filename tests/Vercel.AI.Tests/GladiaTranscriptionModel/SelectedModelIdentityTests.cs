// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gladia;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.GladiaTranscriptionModel;

public sealed class SelectedModelIdentityTests
{
    [Theory]
    [InlineData("default")]
    [InlineData("solaria-3")]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::selected model identity::retains %s through workflow serialization and response metadata", Coverage = UpstreamCoverage.Covered, Commit = "3ab5e75d790c3657574cdbff56a8e3f14ba62bd7")]
    public async Task Retains_model_id_through_workflow_serialization_and_response_metadata(string modelId)
    {
        var handler = GladiaFixtures.Handler();
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);
        var original = (GladiaProvider.GladiaTranscriptionModel)provider.TranscriptionModel(modelId);

        var snapshot = original.Serialize();
        var json = JsonSerializer.Serialize(snapshot.Config);
        var config = JsonSerializer.Deserialize<Dictionary<string, object?>>(json)!;
        var restored = GladiaProvider.GladiaTranscriptionModel.Deserialize(
            new SerializedModelOptions(snapshot.ModelId, config),
            "test-api-key",
            handler);
        Assert.Equal(modelId, restored.ModelId);

        var request = new GladiaTranscriptionRequest
        {
            Body = new JsonObject { ["language_config"] = new JsonObject { ["languages"] = new JsonArray("en") } },
        };
        var result = await restored.TranscribeAsync(GladiaFixtures.Audio(), request, CancellationToken.None);

        Assert.Equal(modelId, result.Response.ModelId);
        foreach (var call in handler.Calls)
        {
            Assert.Equal("test-api-key", call.Header("x-gladia-key"));
            Assert.Equal("ai-sdk/" + Operations.AiSdkVersion.Version, call.Header("x-gladia-version"));
        }

        var body = JsonNode.Parse(handler.Calls[1].Text)!.AsObject();
        if (modelId == "default")
        {
            Assert.False(body.ContainsKey("model"));
        }
        else
        {
            Assert.Equal(modelId, (string?)body["model"]);
        }
    }
}
