// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>A task that is completed from outside. Maps to <c>DelayedPromise</c>.</summary>
public sealed class DelayedPromise<T>
{
    private readonly TaskCompletionSource<T> _source = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>The task that completes on <see cref="Resolve"/> or faults on <see cref="Reject"/>.</summary>
    public Task<T> Promise
    {
        get { return _source.Task; }
    }

    /// <summary>Completes <see cref="Promise"/> with <paramref name="value"/>.</summary>
    public void Resolve(T value)
    {
        _source.TrySetResult(value);
    }

    /// <summary>Faults <see cref="Promise"/> with <paramref name="error"/>.</summary>
    public void Reject(Exception error)
    {
        _source.TrySetException(error ?? throw new ArgumentNullException(nameof(error)));
    }

    /// <summary>True after <see cref="Resolve"/>.</summary>
    public bool IsResolved()
    {
        return _source.Task.Status == TaskStatus.RanToCompletion;
    }

    /// <summary>True after <see cref="Reject"/>.</summary>
    public bool IsRejected()
    {
        return _source.Task.IsFaulted;
    }

    /// <summary>True before resolve or reject.</summary>
    public bool IsPending()
    {
        return !_source.Task.IsCompleted;
    }
}
