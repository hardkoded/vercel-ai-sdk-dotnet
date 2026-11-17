// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AspNetCore;
using Vercel.AI.GenerateText;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;
using Vercel.AI.Tests.Upstream.Steering;
using Vercel.AI.Ui;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateTextSteering.SteeringTestSupport;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgentSteering;

/// <summary>
/// Port of <c>tool-loop-agent-steering.test.ts</c> &gt; <c>ToolLoopAgent experimental_steering</c>.
/// </summary>
public sealed class ToolLoopAgentExperimentalSteeringTests
{
    private const string Prefix =
        "packages/ai/src/agent/tool-loop-agent-steering.test.ts::ToolLoopAgent experimental_steering::";
    private const string Commit = "b8c01eb49d82fd32efe8a2a1e4cd02fbb2b7ae97";
    private const string Prompt = "What is the weather in Paris?";

    [Fact]
    [UpstreamTest(
        Prefix + "should deliver messages in generate() with the default stop condition",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
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
            _ => TextResponse("Tokyo is rainy.")
        );

        var agent = new Agent(new AgentOptions { Model = model }, Client());

        var result = await agent.GenerateAsync(Prompt, experimentalSteering: steering);

        Assert.Equal("Tokyo is rainy.", result.Text);
        SteeringAssert.Delivered(await outcome!, 1);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should deliver queued messages one task at a time in generate()",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_deliver_queued_messages_one_task_at_a_time_in_generate()
    {
        var steering = CreateSteering();
        var first = steering.QueueAsync("Then tell me about Tokyo.");
        var second = steering.QueueAsync("Then tell me about Lagos.");
        var model = SteeringTestModel.Generate(
            _ => TextResponse("It is sunny in Paris."),
            _ => TextResponse("Tokyo is rainy."),
            _ => TextResponse("Lagos is hot.")
        );

        var agent = new Agent(new AgentOptions { Model = model }, Client());

        var result = await agent.GenerateAsync(Prompt, experimentalSteering: steering);

        Assert.Equal("Lagos is hot.", result.Text);
        SteeringAssert.Delivered(await first, 1);
        SteeringAssert.Delivered(await second, 2);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should deliver messages in stream()",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
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
            () => TextStream("Tokyo is rainy.")
        );

        var agent = new Agent(
            new AgentOptions { Model = model, Tools = [Weather(() => "sunny")] },
            Client()
        );

        var result = agent.Stream(Prompt, experimentalSteering: steering);

        Assert.Equal("Tokyo is rainy.", await result.Text);
        SteeringAssert.Delivered(await outcome!, 1);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should stream delivered messages in stream()",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_stream_delivered_messages_in_stream()
    {
        var steering = CreateSteering();
        var model = SteeringTestModel.Stream(
            () =>
            {
                Ignore(
                    steering.SteerAsync(
                        "Also tell me about Tokyo.",
                        new SteerOptions { Id = "steer-1" }
                    )
                );
                return TextStream("Sunny.");
            },
            () => TextStream("Tokyo is rainy.")
        );

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
    [UpstreamTest(
        Prefix + "should not pass the steering object to prepareCall",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
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
            Client()
        );

        await agent.GenerateAsync("hello", experimentalSteering: steering);

        Assert.Null(Assert.Single(prepareCalls).ExperimentalSteering);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should deliver messages through createAgentUIStream",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_deliver_messages_through_createAgentUIStream()
    {
        var steering = CreateSteering();
        Task<SteeringOutcome>? outcome = null;
        var model = SteeringTestModel.Stream(
            () =>
            {
                outcome = steering.SteerAsync("Also tell me about Tokyo.");
                return TextStream("Sunny.");
            },
            () => TextStream("Tokyo is rainy.")
        );

        var stream = await UIMessageStreamConverter.CreateAgentUIStreamAsync(
            new CreateAgentUIStreamOptions
            {
                Agent = new Agent(new AgentOptions { Model = model }, Client()),
                UIMessages =
                [
                    new UIMessage(UIMessageRole.User, [new TextUIPart(Prompt)])
                    {
                        Id = "message-1",
                    },
                ],
                ExperimentalSteering = steering,
            }
        );

        await foreach (var _ in stream) { }

        Assert.Equal(2, model.DoStreamCalls.Count);
        SteeringAssert.Delivered(await outcome!, 1);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should throw when createAgentUIStream gets a steering object with an agent that is not a ToolLoopAgent",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_throw_when_createAgentUIStream_gets_a_steering_object_with_an_agent_that_is_not_a_ToolLoopAgent()
    {
        var agent = new NotAToolLoopAgent();

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            UIMessageStreamConverter.CreateAgentUIStreamAsync(
                new CreateAgentUIStreamOptions
                {
                    Agent = agent,
                    UIMessages =
                    [
                        new UIMessage(UIMessageRole.User, [new TextUIPart("hello")])
                        {
                            Id = "message-1",
                        },
                    ],
                    ExperimentalSteering = CreateSteering(),
                }
            )
        );

        Assert.Equal(
            "Invalid argument for parameter experimental_steering: Steering is only supported by ToolLoopAgent.",
            error.Message
        );
        Assert.Equal(0, agent.StreamCalls);
    }

    /// <summary>Maps to the <c>Agent</c> object literal of the upstream test, whose <c>stream</c> is a mock.</summary>
    private sealed class NotAToolLoopAgent : IAgent
    {
        public int StreamCalls { get; private set; }

        public IReadOnlyList<Tool>? Tools => null;

        public StreamTextResult Stream(
            IReadOnlyList<ModelMessage> messages,
            CancellationToken cancellationToken = default,
            AbortSignal? abortSignal = null,
            TimeoutConfiguration? timeout = null,
            ISteering? experimentalSteering = null,
            Func<StepResult, CancellationToken, Task>? onStepEnd = null
        )
        {
            StreamCalls++;
            throw new InvalidOperationException("stream must not be called");
        }
    }
}
