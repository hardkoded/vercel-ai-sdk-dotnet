// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Util;

namespace Vercel.AI.AspNetCore;

/// <summary>A writer for UI message chunks. Maps to <c>UIMessageStreamWriter</c>.</summary>
public interface IUIMessageStreamWriter
{
    /// <summary>Writes a chunk to the stream.</summary>
    /// <param name="part">The UI message chunk.</param>
    void Write(JsonObject part);

    /// <summary>Merges the contents of another stream into this stream.</summary>
    /// <param name="stream">The stream of UI message chunks to merge.</param>
    void Merge(ReadableStream<JsonObject> stream);

    /// <summary>The error handler that is used by the stream. Maps to <c>onError</c>.</summary>
    Func<Exception, string>? OnError { get; }
}

/// <summary>A <see cref="IUIMessageStreamWriter"/> that can declare the outcome of the operation. Maps to <c>UIMessageStreamWriterWithOutcome</c>.</summary>
public interface IUIMessageStreamWriterWithOutcome : IUIMessageStreamWriter
{
    /// <summary>
    /// Declares the operation outcome. The first declared outcome wins, and a fatal stream failure overrides it.
    /// </summary>
    /// <param name="outcome">The outcome of the operation.</param>
    void SetOutcome(UIMessageStreamOutcome outcome);
}
