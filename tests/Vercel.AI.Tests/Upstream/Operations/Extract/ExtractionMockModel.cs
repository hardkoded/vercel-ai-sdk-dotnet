// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Operations.Extract;

/// <summary>Records calls and returns a configured result. Maps to <c>ExtractionMockModelV4</c> of <c>ai/test</c>.</summary>
internal sealed class ExtractionMockModel : IExtractionCaller
{
    public string Provider { get; set; } = "mock-provider";

    public string ModelId { get; set; } = "mock-model-id";

    public string SpecificationVersion { get; set; } = "v4";

    public IReadOnlyList<string> SupportedFieldTypes { get; set; } = new[] { "text", "number", "date", "time", "datetime", "duration", "list" };

    /// <summary>Handler for <c>doExtract</c>. Replace it to return a result, throw, or inspect the call.</summary>
    public Func<ExtractionModelCall, Task<ExtractionModelResult>> OnExtract { get; set; } = _ => throw new NotImplementedException("Not implemented");

    public List<ExtractionModelCall> Calls { get; } = new List<ExtractionModelCall>();

    public Task<ExtractionModelResult> DoExtractAsync(ExtractionModelCall call, CancellationToken cancellationToken)
    {
        Calls.Add(call);
        return OnExtract(call);
    }
}
