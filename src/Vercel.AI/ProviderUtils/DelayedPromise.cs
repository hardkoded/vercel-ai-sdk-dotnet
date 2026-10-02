// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>
/// A promise that is constructed the first time it is observed.
/// Maps to <c>DelayedPromise</c>.
/// </summary>
public sealed class DelayedPromise<T>
{
    private enum StatusKind
    {
        Pending,
        Resolved,
        Rejected,
    }

    private StatusKind _status = StatusKind.Pending;
    private T? _value;
    private Exception? _error;
    private TaskCompletionSource<T>? _source;

    /// <summary>The promise. Created on first access.</summary>
    public Task<T> Promise
    {
        get
        {
            if (_source != null)
            {
                return _source.Task;
            }

            var source = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (_status == StatusKind.Resolved)
            {
                source.TrySetResult(_value!);
            }
            else if (_status == StatusKind.Rejected)
            {
                source.TrySetException(_error ?? new Exception("failure"));
            }

            _source = source;
            return source.Task;
        }
    }

    /// <summary>Resolves the promise.</summary>
    public void Resolve(T value)
    {
        _status = StatusKind.Resolved;
        _value = value;
        _source?.TrySetResult(value);
    }

    /// <summary>Rejects the promise.</summary>
    public void Reject(Exception error)
    {
        _status = StatusKind.Rejected;
        _error = error ?? throw new ArgumentNullException(nameof(error));
        _source?.TrySetException(error);
    }

    /// <summary>True after <see cref="Resolve"/>.</summary>
    public bool IsResolved()
    {
        return _status == StatusKind.Resolved;
    }

    /// <summary>True after <see cref="Reject"/>.</summary>
    public bool IsRejected()
    {
        return _status == StatusKind.Rejected;
    }

    /// <summary>True before resolve or reject.</summary>
    public bool IsPending()
    {
        return _status == StatusKind.Pending;
    }
}
