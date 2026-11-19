// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.GenerateText;

/// <summary>
/// Why a steering message was not delivered. Maps to <c>SteeringNotDeliveredReason</c>.
/// </summary>
public enum SteeringNotDeliveredReason
{
    /// <summary>The run ended before the message could be delivered (<c>finished</c>).</summary>
    Finished,

    /// <summary><c>prepareStep</c> returned messages without it (<c>removed-by-prepare-step</c>).</summary>
    RemovedByPrepareStep,

    /// <summary>A stop condition ended the run (<c>stop-condition</c>).</summary>
    StopCondition,

    /// <summary>The run ended to wait for tool results, e.g. a tool approval or a tool without an <c>execute</c> function (<c>waiting-for-tool-results</c>).</summary>
    WaitingForToolResults,

    /// <summary>The run was aborted (<c>aborted</c>).</summary>
    Aborted,

    /// <summary>The run failed (<c>error</c>).</summary>
    Error,
}

/// <summary>The outcome of a steering message. Maps to <c>SteeringOutcome</c>.</summary>
public abstract class SteeringOutcome
{
    private protected SteeringOutcome(string type, string id, UserModelMessage message)
    {
        Type = type;
        Id = id;
        Message = message;
    }

    /// <summary><c>delivered</c> or <c>not-delivered</c>.</summary>
    public string Type { get; }

    /// <summary>The id of the steering message.</summary>
    public string Id { get; }

    /// <summary>The user message that was, or was not, added to the run.</summary>
    public UserModelMessage Message { get; }
}

/// <summary>The message was added to the run.</summary>
public sealed class DeliveredSteeringOutcome : SteeringOutcome
{
    /// <summary>Creates the outcome.</summary>
    public DeliveredSteeringOutcome(string id, UserModelMessage message, int stepNumber)
        : base("delivered", id, message)
    {
        StepNumber = stepNumber;
    }

    /// <summary>The number of the step whose model call received the message.</summary>
    public int StepNumber { get; }
}

/// <summary>The message was not added to the run.</summary>
public sealed class NotDeliveredSteeringOutcome : SteeringOutcome
{
    /// <summary>Creates the outcome.</summary>
    public NotDeliveredSteeringOutcome(
        string id,
        UserModelMessage message,
        SteeringNotDeliveredReason reason
    )
        : base("not-delivered", id, message)
    {
        Reason = reason;
    }

    /// <summary>Why the message was not delivered.</summary>
    public SteeringNotDeliveredReason Reason { get; }
}

/// <summary>Options of <c>SteerAsync</c>.</summary>
public sealed class SteerOptions
{
    /// <summary>The id of the message. Sending a message again with the same id returns the outcome of the first message.</summary>
    public string? Id { get; set; }

    /// <summary>
    /// Cancels the current model call when it has not produced any output yet, and makes it again in the same step with the message.
    /// Otherwise the message is added at the next step boundary. Running tools are never interrupted. A cancelled model call can still be billed
    /// by the provider, and it is not included in the usage. The step is not counted again, so stop conditions do not limit how often a step is redone.
    /// </summary>
    public bool Interrupt { get; set; }
}

/// <summary>Options of <c>QueueAsync</c>. Queued messages wait for the task, so they cannot interrupt.</summary>
public sealed class QueueOptions
{
    /// <summary>The id of the message. Sending a message again with the same id returns the outcome of the first message.</summary>
    public string? Id { get; set; }
}

/// <summary>
/// Sends user messages to a running generation. Pass it to <c>GenerateTextAsync</c>, <c>StreamText</c>, or an agent call
/// with the <c>ExperimentalSteering</c> option. Maps to <c>Steering</c>.
/// </summary>
public interface ISteering
{
    /// <summary>
    /// Adds a user message to the run at the next step boundary, after the current step and its tool calls are done. If the run
    /// would end because the model finished its answer, it continues for another step unless a stop condition is met.
    /// Messages that are sent before the run starts are delivered in the first step. A steering object can only be used for one generation.
    /// The task never faults.
    /// </summary>
    Task<SteeringOutcome> SteerAsync(string message, SteerOptions? options = null);

    /// <inheritdoc cref="SteerAsync(string, SteerOptions?)"/>
    Task<SteeringOutcome> SteerAsync(UserModelMessage message, SteerOptions? options = null);

    /// <summary>
    /// Adds a user message to the run once the model has finished its current task, i.e. when it gives a final answer without tool calls.
    /// The run then continues with the message unless a stop condition is met. Queued messages are delivered one per task, in order,
    /// after any waiting steering messages. The task never faults.
    /// </summary>
    Task<SteeringOutcome> QueueAsync(string message, QueueOptions? options = null);

    /// <inheritdoc cref="QueueAsync(string, QueueOptions?)"/>
    Task<SteeringOutcome> QueueAsync(UserModelMessage message, QueueOptions? options = null);
}

/// <summary>A message that <see cref="SteeringRun.ResolveTakenMessages"/> delivered.</summary>
internal sealed record DeliveredSteeringMessage(string Id, UserModelMessage Message);

/// <summary>The run side of a steering object. Maps to <c>SteeringRun</c>.</summary>
internal abstract class SteeringRun
{
    public abstract bool HasPendingMessages();

    /// <summary>Moves the oldest queued message to the waiting messages for the next step. Returns <c>false</c> when no message is queued.</summary>
    public abstract bool PromoteQueuedMessage();

    /// <summary>Takes the waiting messages for the next step. They are resolved by <see cref="ResolveTakenMessages"/> once the prompt of the step is known.</summary>
    public abstract IReadOnlyList<UserModelMessage> TakeMessages();

    /// <summary>Resolves the taken messages as delivered when they are part of the messages sent to the model, and returns the delivered messages with their ids.</summary>
    public abstract IReadOnlyList<DeliveredSteeringMessage> ResolveTakenMessages(
        int stepNumber,
        IReadOnlyList<ModelMessage> sentMessages
    );

    /// <summary>
    /// Watches for interrupting messages while a model call has not produced any output yet. Calls <paramref name="onInterrupt"/> at most once,
    /// right away when an interrupting message is already waiting. Returns a function that stops watching.
    /// </summary>
    public abstract Action WatchInterrupt(Action onInterrupt);

    public abstract void End(SteeringNotDeliveredReason reason);
}

/// <summary>Creates steering objects and starts their runs. Maps to <c>steering.ts</c>.</summary>
public static class Steering
{
    /// <summary>
    /// Creates a steering object that sends user messages to a running generation. Maps to <c>experimental_createSteering</c>.
    /// </summary>
    public static ISteering ExperimentalCreateSteering() => new DefaultSteering();

    /// <summary>Starts a run for a steering object that was passed to a generation.</summary>
    internal static SteeringRun? StartSteeringRun(ISteering? steering)
    {
        if (steering == null)
        {
            return null;
        }

        if (steering is not DefaultSteering defaultSteering)
        {
            throw new InvalidArgumentError(
                "experimental_steering",
                steering,
                "The steering object must be created with experimental_createSteering()."
            );
        }

        return defaultSteering.StartRun();
    }

    internal static string GenerateSteeringId() => JsonValues.GenerateId("steer-");

    private sealed class PendingSteer(
        string id,
        UserModelMessage message,
        TaskCompletionSource<SteeringOutcome> completion
    )
    {
        public string Id { get; } = id;

        public UserModelMessage Message { get; } = message;

        public TaskCompletionSource<SteeringOutcome> Completion { get; } = completion;
    }

    private enum RunState
    {
        Idle,
        Running,
        Ended,
    }

    private sealed class DefaultSteering : ISteering
    {
        // Steering messages can be sent from tool executions on other threads.
        private readonly object _gate = new();
        private readonly Dictionary<string, Task<SteeringOutcome>> _outcomes = new(
            StringComparer.Ordinal
        );
        private readonly List<PendingSteer> _queued = [];
        private RunState _state = RunState.Idle;
        private List<PendingSteer> _pending = [];
        private List<PendingSteer> _taken = [];

        // An interrupt that arrived while no model call was watching, e.g. during prepareStep or tool execution.
        // It is dropped when the messages are taken at a step boundary, because they are then delivered without cancelling a model call.
        private bool _interruptRequested;
        private Action? _onInterrupt;

        public Task<SteeringOutcome> SteerAsync(string message, SteerOptions? options = null) =>
            Add(ToUserMessage(message), options?.Id, options?.Interrupt == true, queue: false);

        public Task<SteeringOutcome> SteerAsync(
            UserModelMessage message,
            SteerOptions? options = null
        ) => Add(message, options?.Id, options?.Interrupt == true, queue: false);

        public Task<SteeringOutcome> QueueAsync(string message, QueueOptions? options = null) =>
            Add(ToUserMessage(message), options?.Id, interrupt: false, queue: true);

        public Task<SteeringOutcome> QueueAsync(
            UserModelMessage message,
            QueueOptions? options = null
        ) => Add(message, options?.Id, interrupt: false, queue: true);

        internal SteeringRun StartRun()
        {
            lock (_gate)
            {
                if (_state != RunState.Idle)
                {
                    throw new InvalidArgumentError(
                        "experimental_steering",
                        this,
                        "The steering object was already used by a generation. Create a new one with experimental_createSteering() for each call."
                    );
                }

                _state = RunState.Running;
            }

            return new Run(this);
        }

        private static UserModelMessage ToUserMessage(string message) =>
            new(message ?? throw new ArgumentNullException(nameof(message)));

        private Task<SteeringOutcome> Add(
            UserModelMessage message,
            string? id,
            bool interrupt,
            bool queue
        )
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            id ??= GenerateSteeringId();
            Action? onInterrupt = null;
            Task<SteeringOutcome> result;
            lock (_gate)
            {
                if (_outcomes.TryGetValue(id, out var existing))
                {
                    return existing;
                }

                Task<SteeringOutcome> outcome;
                if (_state == RunState.Ended)
                {
                    outcome = Task.FromResult<SteeringOutcome>(
                        new NotDeliveredSteeringOutcome(
                            id,
                            message,
                            SteeringNotDeliveredReason.Finished
                        )
                    );
                }
                else
                {
                    var completion = new TaskCompletionSource<SteeringOutcome>(
                        TaskCreationOptions.RunContinuationsAsynchronously
                    );
                    (queue ? _queued : _pending).Add(new PendingSteer(id, message, completion));
                    outcome = completion.Task;
                }

                _outcomes[id] = outcome;
                if (interrupt && _state != RunState.Ended)
                {
                    onInterrupt = RequestInterrupt();
                }

                result = outcome;
            }

            // The watcher runs outside the lock, so it can call back into the steering object.
            onInterrupt?.Invoke();
            return result;
        }

        // Returns the watcher to call, or null when no model call is watching and the interrupt waits for one.
        private Action? RequestInterrupt()
        {
            var onInterrupt = _onInterrupt;
            if (onInterrupt == null)
            {
                _interruptRequested = true;
                return null;
            }

            _onInterrupt = null;
            return onInterrupt;
        }

        private sealed class Run(DefaultSteering owner) : SteeringRun
        {
            public override bool HasPendingMessages()
            {
                lock (owner._gate)
                {
                    return owner._state == RunState.Running && owner._pending.Count > 0;
                }
            }

            public override bool PromoteQueuedMessage()
            {
                lock (owner._gate)
                {
                    if (owner._state != RunState.Running || owner._queued.Count == 0)
                    {
                        return false;
                    }

                    owner._pending.Add(owner._queued[0]);
                    owner._queued.RemoveAt(0);
                    return true;
                }
            }

            public override IReadOnlyList<UserModelMessage> TakeMessages()
            {
                lock (owner._gate)
                {
                    if (owner._state != RunState.Running)
                    {
                        return [];
                    }

                    owner._taken = [.. owner._taken, .. owner._pending];
                    owner._pending = [];
                    owner._interruptRequested = false;
                    return owner._taken.ConvertAll(taken => taken.Message);
                }
            }

            public override Action WatchInterrupt(Action onInterrupt)
            {
                lock (owner._gate)
                {
                    if (owner._state != RunState.Running)
                    {
                        return static () => { };
                    }

                    if (!owner._interruptRequested)
                    {
                        owner._onInterrupt = onInterrupt;
                        return () =>
                        {
                            lock (owner._gate)
                            {
                                if (owner._onInterrupt == onInterrupt)
                                {
                                    owner._onInterrupt = null;
                                }
                            }
                        };
                    }

                    owner._interruptRequested = false;
                }

                onInterrupt();
                return static () => { };
            }

            public override IReadOnlyList<DeliveredSteeringMessage> ResolveTakenMessages(
                int stepNumber,
                IReadOnlyList<ModelMessage> sentMessages
            )
            {
                List<PendingSteer> taken;
                lock (owner._gate)
                {
                    taken = owner._taken;
                    owner._taken = [];
                }

                var delivered = new List<DeliveredSteeringMessage>();
                foreach (var steer in taken)
                {
                    if (IsSent(steer.Message, sentMessages))
                    {
                        delivered.Add(new DeliveredSteeringMessage(steer.Id, steer.Message));
                        steer.Completion.TrySetResult(
                            new DeliveredSteeringOutcome(steer.Id, steer.Message, stepNumber)
                        );
                    }
                    else
                    {
                        steer.Completion.TrySetResult(
                            new NotDeliveredSteeringOutcome(
                                steer.Id,
                                steer.Message,
                                SteeringNotDeliveredReason.RemovedByPrepareStep
                            )
                        );
                    }
                }

                return delivered;
            }

            public override void End(SteeringNotDeliveredReason reason)
            {
                List<PendingSteer> notDelivered;
                lock (owner._gate)
                {
                    if (owner._state != RunState.Running)
                    {
                        return;
                    }

                    notDelivered = [.. owner._taken, .. owner._pending, .. owner._queued];
                    owner._taken = [];
                    owner._pending = [];
                    owner._queued.Clear();
                    owner._state = RunState.Ended;
                    owner._interruptRequested = false;
                    owner._onInterrupt = null;
                }

                foreach (var steer in notDelivered)
                {
                    steer.Completion.TrySetResult(
                        new NotDeliveredSteeringOutcome(steer.Id, steer.Message, reason)
                    );
                }
            }

            // prepareStep can map messages to new objects, e.g. to add provider options,
            // so a message also counts as sent when a user message has the same content:
            private static bool IsSent(
                UserModelMessage message,
                IReadOnlyList<ModelMessage> sentMessages
            )
            {
                string? content = null;
                foreach (var sent in sentMessages)
                {
                    if (ReferenceEquals(sent, message))
                    {
                        return true;
                    }

                    if (sent is UserModelMessage sentUser)
                    {
                        content ??= SerializeContent(message);
                        if (
                            string.Equals(
                                SerializeContent(sentUser),
                                content,
                                StringComparison.Ordinal
                            )
                        )
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            private static string SerializeContent(UserModelMessage message)
            {
                var parts = new object[message.Content.Count];
                for (var i = 0; i < parts.Length; i++)
                {
                    parts[i] = message.Content[i];
                }

                return JsonSerializer.Serialize(parts);
            }
        }
    }
}
