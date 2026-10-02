// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class AgentParityTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate > instructions::should pass string instructions",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_passes_string_instructions()
    {
        var model = Reply();
        var agent = new Agent(new AgentOptions { Model = model, Instructions = "INSTRUCTIONS" }, Client());
        await agent.GenerateAsync("Hello, world!");
        var system = Assert.IsType<SystemModelMessage>(model.Calls[0].Prompt[0]);
        Assert.Equal("INSTRUCTIONS", system.Content);
        var user = Assert.IsType<UserModelMessage>(model.Calls[0].Prompt[1]);
        Assert.Equal("Hello, world!", Assert.IsType<TextContentPart>(Assert.Single(user.Content)).Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > stream::should pass string instructions",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_passes_string_instructions()
    {
        var model = Reply();
        var agent = new Agent(new AgentOptions { Model = model, Instructions = "INSTRUCTIONS" }, Client());
        await agent.Stream("Hello, world!").Text;
        Assert.Equal("INSTRUCTIONS", Assert.IsType<SystemModelMessage>(model.Calls[0].Prompt[0]).Content);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate > LanguageModelCallOptions forwarding::should forward temperature to generateText",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_forwards_temperature()
    {
        var model = Reply();
        await new Agent(new AgentOptions { Model = model, Temperature = 0.5 }, Client()).GenerateAsync("test");
        Assert.Equal(0.5, model.Calls[0].Temperature);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate > LanguageModelCallOptions forwarding::should forward maxOutputTokens to generateText",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_forwards_max_output_tokens()
    {
        var model = Reply();
        await new Agent(new AgentOptions { Model = model, MaxOutputTokens = 256 }, Client()).GenerateAsync("test");
        Assert.Equal(256, model.Calls[0].MaxOutputTokens);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate > LanguageModelCallOptions forwarding::should forward topP to generateText",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_forwards_top_p()
    {
        var model = Reply();
        await new Agent(new AgentOptions { Model = model, TopP = 0.9 }, Client()).GenerateAsync("test");
        Assert.Equal(0.9, model.Calls[0].TopP);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate > LanguageModelCallOptions forwarding::should forward topK to generateText",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_forwards_top_k()
    {
        var model = Reply();
        await new Agent(new AgentOptions { Model = model, TopK = 40 }, Client()).GenerateAsync("test");
        Assert.Equal(40, model.Calls[0].TopK);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate > LanguageModelCallOptions forwarding::should forward presencePenalty to generateText",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_forwards_presence_penalty()
    {
        var model = Reply();
        await new Agent(new AgentOptions { Model = model, PresencePenalty = 0.2 }, Client()).GenerateAsync("test");
        Assert.Equal(0.2, model.Calls[0].PresencePenalty);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate > LanguageModelCallOptions forwarding::should forward frequencyPenalty to generateText",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_forwards_frequency_penalty()
    {
        var model = Reply();
        await new Agent(new AgentOptions { Model = model, FrequencyPenalty = 0.3 }, Client()).GenerateAsync("test");
        Assert.Equal(0.3, model.Calls[0].FrequencyPenalty);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate > LanguageModelCallOptions forwarding::should forward stopSequences to generateText",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_forwards_stop_sequences()
    {
        var model = Reply();
        await new Agent(new AgentOptions { Model = model, StopSequences = new[] { "END" } }, Client()).GenerateAsync("test");
        Assert.Equal(new[] { "END" }, model.Calls[0].StopSequences);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate > LanguageModelCallOptions forwarding::should forward seed to generateText",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_forwards_seed()
    {
        var model = Reply();
        await new Agent(new AgentOptions { Model = model, Seed = 42 }, Client()).GenerateAsync("test");
        Assert.Equal(42, model.Calls[0].Seed);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate > RequestOptions forwarding::should forward headers to generateText",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_forwards_headers()
    {
        var model = Reply();
        await new Agent(
            new AgentOptions { Model = model, Headers = new Dictionary<string, string?> { ["x-custom"] = "value" } },
            Client()).GenerateAsync("test");
        Assert.Equal("value", model.Calls[0].Headers!["x-custom"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate::should pass abortSignal to generateText",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_forwards_the_abort_token()
    {
        using var source = new CancellationTokenSource();
        var model = Reply();
        var agent = new Agent(new AgentOptions { Model = model }, Client());
        await agent.GenerateAsync(new AgentCall { Prompt = "Hello, world!" }, source.Token);
        Assert.Equal(source.Token, model.Tokens[0]);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > stream::should pass abortSignal to streamText",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_forwards_the_abort_token()
    {
        using var source = new CancellationTokenSource();
        var model = Reply();
        var agent = new Agent(new AgentOptions { Model = model }, Client());
        await agent.Stream(new AgentCall { Prompt = "Hello, world!" }, source.Token).Text;
        Assert.Equal(source.Token, model.Tokens[0]);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onStepFinish > generate::should call onStepEnd from constructor and generate method in order",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_calls_constructor_and_method_on_step_end_in_order()
    {
        var calls = new List<string>();
        var agent = new Agent(new AgentOptions
        {
            Model = Reply(),
            OnStepEnd = (_, _) =>
            {
                calls.Add("constructor");
                return Task.CompletedTask;
            },
        }, Client());
        await agent.GenerateAsync(new AgentCall
        {
            Prompt = "Hello, world!",
            OnStepEnd = (_, _) =>
            {
                calls.Add("method");
                return Task.CompletedTask;
            },
        });
        Assert.Equal(new[] { "constructor", "method" }, calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onStepFinish > generate::should prefer onStepEnd over deprecated onStepFinish",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_prefers_on_step_end_over_on_step_finish()
    {
        var calls = new List<string>();
        var agent = new Agent(new AgentOptions
        {
            Model = Reply(),
            OnStepEnd = (_, _) =>
            {
                calls.Add("constructor-onStepEnd");
                return Task.CompletedTask;
            },
            OnStepFinish = (_, _) =>
            {
                calls.Add("constructor-onStepFinish");
                return Task.CompletedTask;
            },
        }, Client());
        await agent.GenerateAsync(new AgentCall
        {
            Prompt = "Hello, world!",
            OnStepEnd = (_, _) =>
            {
                calls.Add("method-onStepEnd");
                return Task.CompletedTask;
            },
            OnStepFinish = (_, _) =>
            {
                calls.Add("method-onStepFinish");
                return Task.CompletedTask;
            },
        });
        Assert.Equal(new[] { "constructor-onStepEnd", "method-onStepEnd" }, calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onStepFinish > generate::should call onStepFinish from constructor",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_calls_constructor_on_step_finish()
    {
        var calls = new List<string>();
        var agent = new Agent(new AgentOptions
        {
            Model = Reply(),
            OnStepFinish = (_, _) =>
            {
                calls.Add("constructor");
                return Task.CompletedTask;
            },
        }, Client());
        await agent.GenerateAsync("Hello, world!");
        Assert.Equal(new[] { "constructor" }, calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onStepFinish > generate::should call onStepFinish from generate method",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_calls_method_on_step_finish()
    {
        var calls = new List<string>();
        var agent = new Agent(new AgentOptions { Model = Reply() }, Client());
        await agent.GenerateAsync(new AgentCall
        {
            Prompt = "Hello, world!",
            OnStepFinish = (_, _) =>
            {
                calls.Add("method");
                return Task.CompletedTask;
            },
        });
        Assert.Equal(new[] { "method" }, calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onStepFinish > generate::should call both constructor and method onStepFinish in correct order",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_calls_both_on_step_finish_callbacks_in_order()
    {
        var calls = new List<string>();
        var agent = new Agent(new AgentOptions
        {
            Model = Reply(),
            OnStepFinish = (_, _) =>
            {
                calls.Add("constructor");
                return Task.CompletedTask;
            },
        }, Client());
        await agent.GenerateAsync(new AgentCall
        {
            Prompt = "Hello, world!",
            OnStepFinish = (_, _) =>
            {
                calls.Add("method");
                return Task.CompletedTask;
            },
        });
        Assert.Equal(new[] { "constructor", "method" }, calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onStepFinish > generate::should pass stepResult to onStepFinish callback",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_passes_the_step_result_to_on_step_finish()
    {
        StepResult? captured = null;
        var agent = new Agent(new AgentOptions { Model = Reply() }, Client());
        await agent.GenerateAsync(new AgentCall
        {
            Prompt = "Hello, world!",
            OnStepFinish = (step, _) =>
            {
                captured = step;
                return Task.CompletedTask;
            },
        });
        Assert.NotNull(captured);
        Assert.Equal(FinishReason.Stop, captured!.FinishReason);
        Assert.Equal(0, captured.StepNumber);
        Assert.Equal("reply", captured.Text);
        Assert.Equal(3, captured.Usage.InputTokens);
        Assert.Equal(10, captured.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onStepFinish > stream::should call onStepFinish from constructor",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_calls_constructor_on_step_finish()
    {
        var calls = new List<string>();
        var agent = new Agent(new AgentOptions
        {
            Model = Reply(),
            OnStepFinish = (_, _) =>
            {
                calls.Add("constructor");
                return Task.CompletedTask;
            },
        }, Client());
        await agent.Stream("Hello, world!").Text;
        Assert.Equal(new[] { "constructor" }, calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onStepFinish > stream::should call onStepFinish from stream method",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_calls_method_on_step_finish()
    {
        var calls = new List<string>();
        var agent = new Agent(new AgentOptions { Model = Reply() }, Client());
        await agent.Stream(new AgentCall
        {
            Prompt = "Hello, world!",
            OnStepFinish = (_, _) =>
            {
                calls.Add("method");
                return Task.CompletedTask;
            },
        }).Text;
        Assert.Equal(new[] { "method" }, calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onStepFinish > stream::should call both constructor and method onStepFinish in correct order",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_calls_both_on_step_finish_callbacks_in_order()
    {
        var calls = new List<string>();
        var agent = new Agent(new AgentOptions
        {
            Model = Reply(),
            OnStepFinish = (_, _) =>
            {
                calls.Add("constructor");
                return Task.CompletedTask;
            },
        }, Client());
        await agent.Stream(new AgentCall
        {
            Prompt = "Hello, world!",
            OnStepFinish = (_, _) =>
            {
                calls.Add("method");
                return Task.CompletedTask;
            },
        }).Text;
        Assert.Equal(new[] { "constructor", "method" }, calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onStepFinish > stream::should pass stepResult to onStepFinish callback",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Stream_passes_the_step_result_to_on_step_finish()
    {
        StepResult? captured = null;
        var agent = new Agent(new AgentOptions { Model = Reply() }, Client());
        await agent.Stream(new AgentCall
        {
            Prompt = "Hello, world!",
            OnStepFinish = (step, _) =>
            {
                captured = step;
                return Task.CompletedTask;
            },
        }).Text;
        Assert.Equal("reply", captured!.Text);
        Assert.Equal(0, captured.StepNumber);
        Assert.Equal(FinishReason.Stop, captured.FinishReason);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onEnd > generate::should call onFinish from constructor",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_calls_constructor_on_finish()
    {
        var calls = new List<string>();
        var agent = new Agent(new AgentOptions
        {
            Model = Reply(),
            OnFinish = (_, _) =>
            {
                calls.Add("constructor");
                return Task.CompletedTask;
            },
        }, Client());
        await agent.GenerateAsync("test");
        Assert.Equal(new[] { "constructor" }, calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onEnd > generate::should call onFinish from generate method",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_calls_method_on_finish()
    {
        var calls = new List<string>();
        var agent = new Agent(new AgentOptions { Model = Reply() }, Client());
        await agent.GenerateAsync(new AgentCall
        {
            Prompt = "test",
            OnFinish = (_, _) =>
            {
                calls.Add("method");
                return Task.CompletedTask;
            },
        });
        Assert.Equal(new[] { "method" }, calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onEnd > generate::should call both constructor and method in correct order",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_calls_both_on_finish_callbacks_in_order()
    {
        var calls = new List<string>();
        var agent = new Agent(new AgentOptions
        {
            Model = Reply(),
            OnFinish = (_, _) =>
            {
                calls.Add("constructor");
                return Task.CompletedTask;
            },
        }, Client());
        await agent.GenerateAsync(new AgentCall
        {
            Prompt = "test",
            OnFinish = (_, _) =>
            {
                calls.Add("method");
                return Task.CompletedTask;
            },
        });
        Assert.Equal(new[] { "constructor", "method" }, calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > onEnd > generate::should pass correct event information",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_on_finish_receives_the_result()
    {
        GenerateTextResult? captured = null;
        var agent = new Agent(new AgentOptions { Model = Reply() }, Client());
        await agent.GenerateAsync(new AgentCall
        {
            Prompt = "test",
            OnFinish = (result, _) =>
            {
                captured = result;
                return Task.CompletedTask;
            },
        });
        Assert.Equal("reply", captured!.Text);
        Assert.Equal(FinishReason.Stop, captured.FinishReason);
        Assert.Single(captured.Steps);
        Assert.Equal(3, captured.Usage.InputTokens);
        Assert.Equal(10, captured.Usage.OutputTokens);
    }

    private static ScriptedLanguageModel Reply()
    {
        return new ScriptedLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText("reply") },
                FinishReason.Stop,
                new LanguageModelUsage(3, 10, 13),
                "stop"),
            OnStream = _ => new LanguageModelStreamPart[]
            {
                new TextDeltaStreamPart("1", "reply"),
                new FinishStreamPart(FinishReason.Stop, new LanguageModelUsage(3, 10, 13), "stop"),
            },
        };
    }

    private static AiClient Client()
    {
        return new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));
    }
}
