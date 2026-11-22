// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>
/// A value, a task, or a synchronous or asynchronous callback that produces the value. Maps to the upstream
/// <c>Resolvable&lt;T&gt;</c> of <c>@ai-sdk/provider-utils</c>. A callback is called every time the value is
/// resolved, so a provider setting such as an API key is resolved per request.
/// </summary>
/// <typeparam name="T">The resolved value type.</typeparam>
public sealed class Resolvable<T>
{
    private readonly T? _value;
    private readonly Task<T>? _task;
    private readonly Func<CancellationToken, Task<T>>? _callback;

    private Resolvable(T? value, Task<T>? task, Func<CancellationToken, Task<T>>? callback)
    {
        _value = value;
        _task = task;
        _callback = callback;
    }

    /// <summary>True when the value comes from a callback rather than a static value or a task.</summary>
    public bool IsCallback => _callback is not null;

    // Upstream checks `typeof apiKey !== 'string'`: only a task or callback is held to the non-empty rule.
    internal bool IsDynamic => _callback is not null || _task is not null;

    /// <summary>Wraps a value.</summary>
    public static implicit operator Resolvable<T>(T value) => From(value);

    /// <summary>Wraps a task.</summary>
    public static implicit operator Resolvable<T>(Task<T> task) => From(task);

    /// <summary>Wraps a value.</summary>
    public static Resolvable<T> From(T value) => new(value, null, null);

    /// <summary>Wraps a task, awaited every time the value is resolved.</summary>
    public static Resolvable<T> From(Task<T> task) =>
        new(default, task ?? throw new ArgumentNullException(nameof(task)), null);

    /// <summary>Wraps a synchronous callback, called every time the value is resolved.</summary>
    public static Resolvable<T> From(Func<T> callback)
    {
        if (callback is null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        return new Resolvable<T>(default, null, _ => Task.FromResult(callback()));
    }

    /// <summary>Wraps an asynchronous callback, called every time the value is resolved.</summary>
    public static Resolvable<T> From(Func<Task<T>> callback)
    {
        if (callback is null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        return new Resolvable<T>(default, null, _ => callback());
    }

    /// <summary>Wraps an asynchronous callback that receives the request's cancellation token.</summary>
    public static Resolvable<T> From(Func<CancellationToken, Task<T>> callback) =>
        new(default, null, callback ?? throw new ArgumentNullException(nameof(callback)));

    /// <summary>Resolves the value. Maps to <c>resolve</c>.</summary>
    public async Task<T> ResolveAsync(CancellationToken cancellationToken = default)
    {
        if (_callback is not null)
        {
            return await _callback(cancellationToken).ConfigureAwait(false);
        }

        if (_task is not null)
        {
            return await _task.ConfigureAwait(false);
        }

        return _value!;
    }

    /// <summary>Resolves the value and transforms it. Maps to <c>resolveWith</c>.</summary>
    public async Task<TResult> ResolveWithAsync<TResult>(
        Func<T, TResult> transform,
        CancellationToken cancellationToken = default
    )
    {
        if (transform is null)
        {
            throw new ArgumentNullException(nameof(transform));
        }

        return transform(await ResolveAsync(cancellationToken).ConfigureAwait(false));
    }
}
