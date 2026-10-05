// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class CallbackTests
{
    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > callback invocation::should call a single callback with the event", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Notifies_one_callback()
    {
        var calls = new List<string>();
        await Callbacks.NotifyAsync(new EventValue("hello"), delegate(EventValue value)
        {
            calls.Add(value.Value);
            return Task.CompletedTask;
        });
        Assert.Equal(new[] { "hello" }, calls);
    }

    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > callback invocation::should call all callbacks when given an array", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Notifies_every_callback()
    {
        var calls = new List<string>();
        await Callbacks.NotifyAsync(new EventValue("hello"), new Func<EventValue, Task>[]
        {
            delegate(EventValue value)
            {
                calls.Add("first: " + value.Value);
                return Task.CompletedTask;
            },
            delegate(EventValue value)
            {
                calls.Add("second: " + value.Value);
                return Task.CompletedTask;
            },
        });
        Assert.Equal(new[] { "first: hello", "second: hello" }, calls);
    }

    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > callback invocation::should handle undefined callbacks", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Ignores_a_null_callback()
    {
        await Callbacks.NotifyAsync(new EventValue("hello"), (Func<EventValue, Task>?)null);
    }

    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > callback invocation::should handle omitted callbacks", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Ignores_omitted_callbacks()
    {
        await Callbacks.NotifyAsync(new EventValue("hello"), (IEnumerable<Func<EventValue, Task>?>?)null);
    }

    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > async support::should await async callbacks before continuing", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Waits_for_an_async_callback()
    {
        var calls = new List<string>();
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var notify = Callbacks.NotifyAsync("test", delegate
        {
            return WaitThen(gate.Task, delegate { calls.Add("async done"); });
        });
        Assert.Empty(calls);
        gate.SetResult(true);
        await notify;
        calls.Add("after notify");
        Assert.Equal(new[] { "async done", "after notify" }, calls);
    }

    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > async support::should run async callbacks in parallel and await all of them", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Runs_callbacks_in_parallel()
    {
        var calls = new List<string>();
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var notify = Callbacks.NotifyAsync("test", new Func<string, Task>[]
        {
            delegate
            {
                calls.Add("slow start");
                return WaitThen(gate.Task, delegate { calls.Add("slow end"); });
            },
            delegate
            {
                calls.Add("fast start");
                calls.Add("fast end");
                return Task.CompletedTask;
            },
        });
        Assert.Equal(new[] { "slow start", "fast start", "fast end" }, calls);
        gate.SetResult(true);
        await notify;
        Assert.Equal(new[] { "slow start", "fast start", "fast end", "slow end" }, calls);
    }

    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > error handling::should catch errors in a single callback without breaking", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Swallows_a_callback_exception()
    {
        var calls = new List<string>();
        await Callbacks.NotifyAsync("test", delegate
        {
            calls.Add("before throw");
            throw new InvalidOperationException("callback error");
        });
        calls.Add("after notify");
        Assert.Equal(new[] { "before throw", "after notify" }, calls);
    }

    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > error handling::should catch errors in array callbacks and continue to next", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Continues_after_a_callback_exception()
    {
        var calls = new List<string>();
        await Callbacks.NotifyAsync("test", new Func<string, Task>[]
        {
            delegate
            {
                calls.Add("first before throw");
                throw new InvalidOperationException("first error");
            },
            delegate
            {
                calls.Add("second runs");
                return Task.CompletedTask;
            },
        });
        Assert.Equal(new[] { "first before throw", "second runs" }, calls);
    }

    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > error handling::should catch async rejection without breaking", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Swallows_an_async_rejection()
    {
        var calls = new List<string>();
        await Callbacks.NotifyAsync("test", delegate
        {
            calls.Add("async before reject");
            return Task.FromException(new InvalidOperationException("async error"));
        });
        calls.Add("after notify");
        Assert.Equal(new[] { "async before reject", "after notify" }, calls);
    }

    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > type safety::should preserve event type through to callback", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Preserves_the_event_shape()
    {
        WeatherEvent? received = null;
        var callback = new Func<WeatherEvent, Task>(delegate(WeatherEvent value)
        {
            received = value;
            return Task.CompletedTask;
        });
        var input = new WeatherEvent
        {
            ToolName = "getWeather",
            Input = new WeatherInput { Location = "San Francisco" },
            StepNumber = 2,
        };
        await Callbacks.NotifyAsync(input, callback);
        Assert.Same(input, received);
        Assert.Equal("getWeather", received!.ToolName);
        Assert.Equal("San Francisco", received.Input.Location);
        Assert.Equal(2, received.StepNumber);
    }

    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > type safety::should work with complex nested event types", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Preserves_nested_event_fields()
    {
        string? provider = null;
        var steps = 0;
        await Callbacks.NotifyAsync(new ModelEvent
        {
            Model = new ModelRef { Provider = "openai", ModelId = "gpt-4o" },
            Usage = new Usage { InputTokens = 100, OutputTokens = 50 },
            Steps = new[] { new Step { StepNumber = 0 }, new Step { StepNumber = 1 } },
        }, delegate(ModelEvent value)
        {
            provider = value.Model.Provider;
            steps = value.Steps.Length;
            return Task.CompletedTask;
        });
        Assert.Equal("openai", provider);
        Assert.Equal(2, steps);
    }

    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > multiple sequential notifications::should handle repeated calls with the same callback", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Reuses_a_callback()
    {
        var events = new List<string>();
        Func<string, Task> callback = delegate(string value)
        {
            events.Add(value);
            return Task.CompletedTask;
        };
        await Callbacks.NotifyAsync("first", callback);
        await Callbacks.NotifyAsync("second", callback);
        await Callbacks.NotifyAsync("third", callback);
        Assert.Equal(new[] { "first", "second", "third" }, events);
    }

    [UpstreamTest("packages/ai/src/util/merge-callbacks.test.ts::mergeCallbacks::should invoke callbacks in parallel, wait for them to settle, and continue after errors", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Merged_callbacks_run_together_and_ignore_errors()
    {
        var calls = new List<string>();
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var merged = Callbacks.MergeCallbacks<EventValue>(
            delegate(EventValue value)
            {
                calls.Add("first start: " + value.Value);
                return WaitThen(gate.Task, delegate { calls.Add("first end"); });
            },
            null,
            delegate
            {
                calls.Add("second before throw");
                throw new InvalidOperationException("callback error");
            },
            delegate(EventValue value)
            {
                calls.Add("third: " + value.Value);
                return Task.CompletedTask;
            });
        var pending = merged(new EventValue("hello"));
        calls.Add("after call");
        Assert.False(pending.IsCompleted);
        Assert.Equal(new[] { "first start: hello", "second before throw", "third: hello", "after call" }, calls);
        gate.SetResult(true);
        await pending;
        Assert.Equal(new[] { "first start: hello", "second before throw", "third: hello", "after call", "first end" }, calls);
    }

    [UpstreamTest("packages/ai/src/util/merge-callbacks.test.ts::mergeCallbacks::should ignore rejected callbacks", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Merged_callbacks_ignore_rejections()
    {
        var calls = new List<string>();
        var merged = Callbacks.MergeCallbacks<EventValue>(
            delegate(EventValue value)
            {
                calls.Add("first before reject: " + value.Value);
                return Task.FromException(new InvalidOperationException("callback error"));
            },
            delegate(EventValue value)
            {
                calls.Add("second: " + value.Value);
                return Task.CompletedTask;
            });
        await merged(new EventValue("hello"));
        Assert.Equal(new[] { "first before reject: hello", "second: hello" }, calls);
    }

    [UpstreamTest("packages/ai/src/util/merge-callbacks.test.ts::mergeCallbacks::should ignore undefined callbacks", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Merged_callbacks_skip_nulls()
    {
        var calls = new List<string>();
        var merged = Callbacks.MergeCallbacks<EventValue>(
            null,
            delegate(EventValue value)
            {
                calls.Add(value.Value);
                return Task.CompletedTask;
            },
            null);
        await merged(new EventValue("hello"));
        Assert.Equal(new[] { "hello" }, calls);
    }

    private static async Task WaitThen(Task gate, Action after)
    {
        await gate;
        after();
    }

    private sealed class EventValue
    {
        public EventValue(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    private sealed class WeatherEvent
    {
        public string ToolName { get; set; } = string.Empty;

        public WeatherInput Input { get; set; } = null!;

        public int StepNumber { get; set; }
    }

    private sealed class WeatherInput
    {
        public string Location { get; set; } = string.Empty;
    }

    private sealed class ModelEvent
    {
        public ModelRef Model { get; set; } = null!;

        public Usage Usage { get; set; } = null!;

        public Step[] Steps { get; set; } = Array.Empty<Step>();
    }

    private sealed class ModelRef
    {
        public string Provider { get; set; } = string.Empty;

        public string ModelId { get; set; } = string.Empty;
    }

    private sealed class Usage
    {
        public int InputTokens { get; set; }

        public int OutputTokens { get; set; }
    }

    private sealed class Step
    {
        public int StepNumber { get; set; }
    }
}
