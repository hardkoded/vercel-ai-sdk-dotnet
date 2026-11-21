// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.ProviderUtils;

/// <summary>A body stream whose every read returns at most <c>chunkSize</c> bytes, like a body that arrives in chunks.</summary>
internal sealed class ChunkedStream(byte[] body, int chunkSize) : MemoryStream(body)
{
    public override int Read(byte[] buffer, int offset, int count) =>
        base.Read(buffer, offset, Math.Min(count, chunkSize));

    public override Task<int> ReadAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken
    ) => Task.FromResult(Read(buffer, offset, count));

    public override ValueTask<int> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default
    ) => new(base.Read(buffer.Span[..Math.Min(buffer.Length, chunkSize)]));
}
