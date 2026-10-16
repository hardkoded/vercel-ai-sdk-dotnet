// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>
/// Releases a resource when an enumerator of the sequence is disposed, even before the first <c>MoveNextAsync</c>.
/// An async iterator never runs its own <c>finally</c> or <c>using</c> in that case.
/// </summary>
internal sealed class ReleasingAsyncEnumerable<T> : IAsyncEnumerable<T>
{
    private readonly IAsyncEnumerable<T> _source;
    private readonly IDisposable _resource;

    public ReleasingAsyncEnumerable(IAsyncEnumerable<T> source, IDisposable resource)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _resource = resource ?? throw new ArgumentNullException(nameof(resource));
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        IAsyncEnumerator<T> inner;
        try
        {
            inner = _source.GetAsyncEnumerator(cancellationToken);
        }
        catch
        {
            _resource.Dispose();
            throw;
        }

        return new Enumerator(inner, _resource);
    }

    private sealed class Enumerator : IAsyncEnumerator<T>
    {
        private readonly IAsyncEnumerator<T> _inner;
        private readonly IDisposable _resource;

        public Enumerator(IAsyncEnumerator<T> inner, IDisposable resource)
        {
            _inner = inner;
            _resource = resource;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return _inner.MoveNextAsync();
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await _inner.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                _resource.Dispose();
            }
        }
    }
}
