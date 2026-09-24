// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests;

public sealed class CoreTests
{
    [Fact]
    public async Task Generate_text_returns_the_model_text()
    {
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text("hello") };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "Hi" });
        Assert.Equal("hello", result.Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
    }

    [Fact]
    public async Task Tools_run_and_a_second_step_can_answer()
    {
        var model = new TestLanguageModel();
        model.OnGenerate = _ => model.Calls.Count == 1
            ? new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedToolCall("c1", "lookup", "{\"q\":\"x\"}") },
                FinishReason.ToolCalls,
                LanguageModelUsage.Empty,
                "tool_calls")
            : TestLanguageModel.Text("done");

        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "find x",
            StopWhen = StopWhen.IsStepCount(2),
            Tools = new[]
            {
                Tool.Function("lookup", "Looks up a value", "{\"type\":\"object\"}", (_, _) => Task.FromResult("{\"ok\":true}")),
            },
        });

        Assert.Equal("done", result.Text);
        Assert.Single(result.ToolResults);
        Assert.Equal("{\"ok\":true}", result.ToolResults[0].OutputJson);
        Assert.Equal(2, model.Calls.Count);
    }

    [Fact]
    public async Task Structured_output_parses_the_last_step()
    {
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text("{\"city\":\"Paris\"}") };
        var result = await Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "where",
            Output = OutputSpec.Object("{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}}}", "place"),
        });

        Assert.Equal("Paris", result.Output!.Value.GetProperty("city").GetString());
    }

    [Fact]
    public async Task Stream_yields_text_deltas()
    {
        var model = new TestLanguageModel
        {
            StreamParts = new LanguageModelStreamPart[]
            {
                new TextDeltaStreamPart("text", "ab"),
                new TextDeltaStreamPart("text", "c"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(1, 2, 3), "stop"),
            },
        };
        var stream = Client().StreamTextAsync(new StreamTextOptions { Model = model, Prompt = "go" });
        var parts = new List<string>();
        await foreach (var delta in stream.TextStream())
        {
            parts.Add(delta);
        }

        Assert.Equal(new[] { "ab", "c" }, parts);
        Assert.Equal("abc", await stream.Text);
    }

    [Fact]
    public void Cosine_similarity_of_the_same_vector_is_one()
    {
        var score = Ai.CosineSimilarity(new[] { 1f, 0f }, new[] { 1f, 0f });
        Assert.Equal(1d, score, 5);
    }

    [Fact]
    public async Task Extract_reasoning_middleware_splits_think_tags()
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => TestLanguageModel.Text("<think>because</think> answer"),
        }.WrapLanguageModel(new ExtractReasoningMiddleware());
        var result = await Client().GenerateTextAsync(new GenerateTextOptions { Model = model, Prompt = "why" });
        Assert.Equal("answer", result.Text);
        Assert.Equal("because", result.ReasoningText);
    }

    [Fact]
    public void Registry_resolves_provider_and_model()
    {
        var model = new TestLanguageModel("echo");
        var registry = new ProviderRegistry();
        registry.Register("custom", new CustomProvider().AddLanguageModel(model));
        Assert.Same(model, registry.LanguageModel("custom:echo"));
    }

    [Fact]
    public async Task Agent_uses_its_model()
    {
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text("agent") };
        var agent = new Agent(new AgentOptions { Model = model }, Client());
        var result = await agent.GenerateAsync("hello");
        Assert.Equal("agent", result.Text);
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
