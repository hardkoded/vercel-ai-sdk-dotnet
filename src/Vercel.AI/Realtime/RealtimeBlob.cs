// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;

namespace Vercel.AI;

/// <summary>
/// A binary WebSocket message whose text is decoded on demand. Stands in for the JavaScript <c>Blob</c> that a
/// browser WebSocket delivers.
/// </summary>
public class RealtimeBlob
{
    private readonly byte[] _bytes;

    /// <summary>Creates a blob over <paramref name="bytes"/>.</summary>
    public RealtimeBlob(byte[]? bytes = null)
    {
        _bytes = bytes ?? [];
    }

    /// <summary>The size in bytes. Maps to <c>size</c>.</summary>
    public virtual long Size => _bytes.Length;

    /// <summary>Decodes the content as UTF-8 text. Maps to <c>text()</c>.</summary>
    public virtual Task<string> TextAsync()
    {
        return Task.FromResult(Encoding.UTF8.GetString(_bytes));
    }
}
