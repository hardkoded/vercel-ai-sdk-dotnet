// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class CallbackTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > callback invocation::should call a single callback with the event", Coverage = UpstreamCoverage.Covered)]
    public async Task Calls_a_single_callback()
    {
        var calls = new List<string>();
        await Callbacks.NotifyAsync(new EventValue("hello"), value =>
        {
            calls.Add(value.Value);
            return Task.CompletedTask;
        });
        Assert.Equal(new[] { "hello" }, calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > callback invocation::should call all callbacks when given an array", Coverage = UpstreamCoverage.Covered)]
    public async Task Calls_every_callback()
    {
        var calls = new List<string>();
        await Callbacks.NotifyAsync(
            new EventValue("hello"),
            value =>
            {
                calls.Add("first: " + value.Value);
                return Task.CompletedTask;
            },
            value =>
            {
                calls.Add("second: " + value.Value);
                return Task.CompletedTask;
            });
        Assert.Equal(new[] { "first: hello", "second: hello" }, calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > callback invocation::should handle undefined callbacks", Coverage = UpstreamCoverage.Covered)]
    public async Task Ignores_a_null_callback_list()
    {
        await Callbacks.NotifyAsync("hello", (IEnumerable<Func<string, Task>?>?)null);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > callback invocation::should handle omitted callbacks", Coverage = UpstreamCoverage.Covered)]
    public async Task Completes_when_no_callbacks_are_given()
    {
        await Callbacks.NotifyAsync("hello");
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > async support::should await async callbacks before continuing", Coverage = UpstreamCoverage.Covered)]
    public async Task Awaits_an_async_callback()
    {
        var calls = new List<string>();
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var notify = Callbacks.NotifyAsync("test", async _ =>
        {
            await gate.Task.ConfigureAwait(false);
            calls.Add("async done");
        });
        Assert.Empty(calls);
        gate.TrySetResult(true);
        await notify;
        calls.Add("after notify");
        Assert.Equal(new[] { "async done", "after notify" }, calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/notify.test.ts::notify > async support::should run async callbacks in parallel and await all of them",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Runs_callbacks_in_parallel()
    {
        var calls = new List<string>();
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var notify = Callbacks.NotifyAsync(
            "test",
            async _ =>
            {
                calls.Add("slow start");
                await gate.Task.ConfigureAwait(false);
                calls.Add("slow end");
            },
            _ =>
            {
                calls.Add("fast start");
                calls.Add("fast end");
                return Task.CompletedTask;
            });
        Assert.Equal(new[] { "slow start", "fast start", "fast end" }, calls);
        gate.TrySetResult(true);
        await notify;
        Assert.Equal(new[] { "slow start", "fast start", "fast end", "slow end" }, calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > error handling::should catch errors in a single callback without breaking", Coverage = UpstreamCoverage.Covered)]
    public async Task Swallows_a_callback_exception()
    {
        var calls = new List<string>();
        await Callbacks.NotifyAsync<string>("test", _ =>
        {
            calls.Add("before throw");
            throw new InvalidOperationException("callback error");
        });
        calls.Add("after notify");
        Assert.Equal(new[] { "before throw", "after notify" }, calls);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/notify.test.ts::notify > error handling::should catch errors in array callbacks and continue to next",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Continues_after_a_callback_exception()
    {
        var calls = new List<string>();
        await Callbacks.NotifyAsync<string>(
            "test",
            _ =>
            {
                calls.Add("first before throw");
                throw new InvalidOperationException("first error");
            },
            _ =>
            {
                calls.Add("second runs");
                return Task.CompletedTask;
            });
        Assert.Equal(new[] { "first before throw", "second runs" }, calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > error handling::should catch async rejection without breaking", Coverage = UpstreamCoverage.Covered)]
    public async Task Swallows_an_async_callback_exception()
    {
        var calls = new List<string>();
        await Callbacks.NotifyAsync<string>("test", _ =>
        {
            calls.Add("async before reject");
            throw new InvalidOperationException("async error");
        });
        calls.Add("after notify");
        Assert.Equal(new[] { "async before reject", "after notify" }, calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > type safety::should preserve event type through to callback", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_the_event_object_through()
    {
        ToolEvent? received = null;
        var callback = new Func<ToolEvent, Task>(value =>
        {
            received = value;
            return Task.CompletedTask;
        });
        var input = new ToolEvent("getWeather", "San Francisco", 2);
        await Callbacks.NotifyAsync(input, callback);
        Assert.Same(input, received);
        Assert.Equal("getWeather", received!.ToolName);
        Assert.Equal("San Francisco", received.Location);
        Assert.Equal(2, received.StepNumber);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/notify.test.ts::notify > type safety::should work with complex nested event types", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_a_nested_event()
    {
        string? provider = null;
        var steps = 0;
        var input = new NestedEvent("openai", "gpt-4o", 2);
        await Callbacks.NotifyAsync(input, value =>
        {
            provider = value.Provider;
            steps = value.StepCount;
            return Task.CompletedTask;
        });
        Assert.Equal("openai", provider);
        Assert.Equal(2, steps);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/notify.test.ts::notify > multiple sequential notifications::should handle repeated calls with the same callback",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Reuses_one_callback()
    {
        var events = new List<string>();
        Func<string, Task> callback = value =>
        {
            events.Add(value);
            return Task.CompletedTask;
        };
        await Callbacks.NotifyAsync("first", callback);
        await Callbacks.NotifyAsync("second", callback);
        await Callbacks.NotifyAsync("third", callback);
        Assert.Equal(new[] { "first", "second", "third" }, events);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/merge-callbacks.test.ts::mergeCallbacks::should invoke callbacks in parallel, wait for them to settle, and continue after errors",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Merged_callbacks_run_together_and_swallow_errors()
    {
        var calls = new List<string>();
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var merged = Callbacks.MergeCallbacks<EventValue>(
            async value =>
            {
                calls.Add("first start: " + value.Value);
                await gate.Task.ConfigureAwait(false);
                calls.Add("first end");
            },
            null,
            _ =>
            {
                calls.Add("second before throw");
                throw new InvalidOperationException("callback error");
            },
            value =>
            {
                calls.Add("third: " + value.Value);
                return Task.CompletedTask;
            });
        var pending = merged(new EventValue("hello"));
        calls.Add("after call");
        Assert.Equal(new[] { "first start: hello", "second before throw", "third: hello", "after call" }, calls);
        gate.TrySetResult(true);
        await pending;
        Assert.Equal(new[] { "first start: hello", "second before throw", "third: hello", "after call", "first end" }, calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-callbacks.test.ts::mergeCallbacks::should ignore rejected callbacks", Coverage = UpstreamCoverage.Covered)]
    public async Task Merged_callbacks_ignore_rejections()
    {
        var calls = new List<string>();
        var merged = Callbacks.MergeCallbacks<EventValue>(
            value =>
            {
                calls.Add("first before reject: " + value.Value);
                return Task.FromException(new InvalidOperationException("callback error"));
            },
            value =>
            {
                calls.Add("second: " + value.Value);
                return Task.CompletedTask;
            });
        await merged(new EventValue("hello"));
        Assert.Equal(new[] { "first before reject: hello", "second: hello" }, calls);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/util/merge-callbacks.test.ts::mergeCallbacks::should ignore undefined callbacks", Coverage = UpstreamCoverage.Covered)]
    public async Task Merged_callbacks_ignore_null_entries()
    {
        var calls = new List<string>();
        var merged = Callbacks.MergeCallbacks<EventValue>(
            null,
            value =>
            {
                calls.Add(value.Value);
                return Task.CompletedTask;
            },
            null);
        await merged(new EventValue("hello"));
        Assert.Equal(new[] { "hello" }, calls);
    }

    private sealed class EventValue
    {
        public EventValue(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    private sealed class ToolEvent
    {
        public ToolEvent(string toolName, string location, int stepNumber)
        {
            ToolName = toolName;
            Location = location;
            StepNumber = stepNumber;
        }

        public string ToolName { get; }

        public string Location { get; }

        public int StepNumber { get; }
    }

    private sealed class NestedEvent
    {
        public NestedEvent(string provider, string modelId, int stepCount)
        {
            Provider = provider;
            ModelId = modelId;
            StepCount = stepCount;
        }

        public string Provider { get; }

        public string ModelId { get; }

        public int StepCount { get; }
    }
}
