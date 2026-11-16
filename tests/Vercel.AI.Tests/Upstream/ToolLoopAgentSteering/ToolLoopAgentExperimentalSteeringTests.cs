// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;
using Vercel.AI.Tests.Upstream.Steering;
using static Vercel.AI.Tests.Upstream.GenerateTextSteering.SteeringTestSupport;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgentSteering;

/// <summary>
/// Port of <c>tool-loop-agent-steering.test.ts</c> &gt; <c>ToolLoopAgent experimental_steering</c>. The port has no <c>createAgentUIStream</c>,
/// so the two <c>createAgentUIStream</c> tests are not ported.
/// </summary>
public sealed class ToolLoopAgentExperimentalSteeringTests
{
    private const string Prefix = "packages/ai/src/agent/tool-loop-agent-steering.test.ts::ToolLoopAgent experimental_steering::";
    private const string Commit = "b8c01eb49d82fd32efe8a2a1e4cd02fbb2b7ae97";
    private const string Prompt = "What is the weather in Paris?";

    [Fact]
    [UpstreamTest(Prefix + "should deliver messages in generate() with the default stop condition", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_deliver_messages_in_generate_with_the_default_stop_condition()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var model = SteeringTestModel.Generate(
            _ =>
            {
                outcome = steering.SteerAsync("Also tell me about Tokyo.");
                return TextResponse("It is sunny in Paris.");
            },
            _ => TextResponse("Tokyo is rainy."));

        var agent = new Agent(new AgentOptions { Model = model }, Client());

        var result = await agent.GenerateAsync(Prompt, experimentalSteering: steering);

        Assert.Equal("Tokyo is rainy.", result.Text);
        SteeringAssert.Delivered(await outcome!, 1);
    }

    [Fact]
    [UpstreamTest(Prefix + "should deliver queued messages one task at a time in generate()", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_deliver_queued_messages_one_task_at_a_time_in_generate()
    {
        var steering = CreateSteering();
        var first = steering.QueueAsync("Then tell me about Tokyo.");
        var second = steering.QueueAsync("Then tell me about Lagos.");
        var model = SteeringTestModel.Generate(
            _ => TextResponse("It is sunny in Paris."),
            _ => TextResponse("Tokyo is rainy."),
            _ => TextResponse("Lagos is hot."));

        var agent = new Agent(new AgentOptions { Model = model }, Client());

        var result = await agent.GenerateAsync(Prompt, experimentalSteering: steering);

        Assert.Equal("Lagos is hot.", result.Text);
        SteeringAssert.Delivered(await first, 1);
        SteeringAssert.Delivered(await second, 2);
    }

    [Fact]
    [UpstreamTest(Prefix + "should deliver messages in stream()", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_deliver_messages_in_stream()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var model = SteeringTestModel.Stream(
            () =>
            {
                outcome = steering.SteerAsync("Also tell me about Tokyo.");
                return TextStream("Sunny.");
            },
            () => TextStream("Tokyo is rainy."));

        var agent = new Agent(new AgentOptions { Model = model, Tools = [Weather(() => "sunny")] }, Client());

        var result = agent.Stream(Prompt, experimentalSteering: steering);

        Assert.Equal("Tokyo is rainy.", await result.Text);
        SteeringAssert.Delivered(await outcome!, 1);
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream delivered messages in stream()", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_stream_delivered_messages_in_stream()
    {
        var steering = CreateSteering();
        var model = SteeringTestModel.Stream(
            () =>
            {
                Ignore(steering.SteerAsync("Also tell me about Tokyo.", new SteerOptions { Id = "steer-1" }));
                return TextStream("Sunny.");
            },
            () => TextStream("Tokyo is rainy."));

        var agent = new Agent(new AgentOptions { Model = model }, Client());

        var result = agent.Stream(Prompt, experimentalSteering: steering);

        var steeringParts = new List<SteeringMessagePart>();
        await foreach (var part in result.Stream())
        {
            if (part is SteeringMessagePart steeringPart)
            {
                steeringParts.Add(steeringPart);
            }
        }

        var delivered = Assert.Single(steeringParts);
        Assert.Equal("steer-1", delivered.Id);
        Assert.Equal("Also tell me about Tokyo.", Text(delivered.Message));
        Assert.Equal(1, delivered.StepNumber);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not pass the steering object to prepareCall", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_not_pass_the_steering_object_to_prepareCall()
    {
        var steering = CreateSteering();
        var prepareCalls = new List<GenerateTextOptions>();

        var agent = new Agent(
            new AgentOptions
            {
                Model = SteeringTestModel.Generate(_ => TextResponse("done")),
                PrepareCall = options =>
                {
                    prepareCalls.Add(options);
                    return options;
                },
            },
            Client());

        await agent.GenerateAsync("hello", experimentalSteering: steering);

        Assert.Null(Assert.Single(prepareCalls).ExperimentalSteering);
    }
}
