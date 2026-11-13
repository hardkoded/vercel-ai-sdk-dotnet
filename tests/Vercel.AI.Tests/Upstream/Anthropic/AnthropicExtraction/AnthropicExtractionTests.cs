// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.ExtractionProviderFixtures;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicExtraction;

/// <summary>Port of <c>anthropic-extraction.test.ts</c>.</summary>
public sealed class AnthropicExtractionTests
{
    private const string Prefix = "packages/anthropic/src/anthropic-extraction.test.ts::";

    private static ExtractionLanguageModel Model(HttpMessageHandler handler)
    {
        return AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-key", BaseUrl = "https://example.com/v1" }, handler)
            .ExtractionModel("claude-sonnet-5-5");
    }

    [Fact]
    [UpstreamTest(Prefix + "extracts typed fields with Messages structured output", Coverage = UpstreamCoverage.Covered, Commit = "6d5094711e57f0ccf62fd60270865a74863bbb08")]
    public async Task Extracts_typed_fields_with_Messages_structured_output()
    {
        var handler = new RespondingHandler(_ => Fixture("anthropic-extraction.json").ToJsonString());
        var model = Model(handler);

        var result = await model.DoExtractAsync(InvoiceCall());

        Assert.Equal("anthropic.extraction", model.Provider);
        Assert.Equal("claude-sonnet-5-5", model.ModelId);
        AssertInvoiceAnswers(result.Answers);
        var (url, body) = Assert.Single(handler.Requests);
        Assert.Equal("https://example.com/v1/messages", url);
        var format = JsonNode.Parse(body)!["output_config"]!["format"]!;
        Assert.Equal("json_schema", (string?)format["type"]);
        Assert.Equal(["f0", "f1", "f2", "f3", "f4"], format["schema"]!["required"]!.AsArray().Select(node => (string)node!));
    }

    [Fact]
    [UpstreamTest(Prefix + "splits more than 16 fields into parallel calls under the union parameter limit", Coverage = UpstreamCoverage.Covered, Commit = "6d5094711e57f0ccf62fd60270865a74863bbb08")]
    public async Task Splits_more_than_16_fields_into_parallel_calls_under_the_union_parameter_limit()
    {
        var handler = new RespondingHandler(requestBody =>
        {
            var body = JsonNode.Parse(requestBody)!;
            var fieldsText = (string)body["messages"]![0]!["content"]![0]!["text"]!;
            var descriptions = JsonNode.Parse(fieldsText)!["fields"]!.AsObject();
            var answers = new JsonObject();
            foreach (var pair in descriptions)
            {
                answers[pair.Key] = double.Parse(((string)pair.Value!["id"]!).Split(' ')[1], CultureInfo.InvariantCulture);
            }

            var response = Fixture("anthropic-extraction.json");
            response["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = answers.ToJsonString() });
            return response.ToJsonString();
        });
        var model = Model(handler);

        var fields = new Dictionary<string, ExtractionField>();
        for (var i = 0; i < 17; i++)
        {
            fields["price " + i.ToString(CultureInfo.InvariantCulture)] = new ExtractionField("number");
        }

        var result = await model.DoExtractAsync(Call([new DecisionModelTextPart("Prices.")], fields));

        Assert.Equal(2, handler.Requests.Count);

        // Upstream reads the counts in call order; the port sends the groups concurrently, so order them by first field.
        var unionCounts = handler.Requests
            .Select(request => JsonNode.Parse(request.Body)!)
            .OrderBy(body => (string)JsonNode.Parse((string)body["messages"]![0]!["content"]![0]!["text"]!)!["fields"]!["f0"]!["id"]!, StringComparer.Ordinal)
            .Select(body => body["output_config"]!["format"]!["schema"]!["properties"]!.AsObject().Count(property => property.Value!["anyOf"] != null))
            .ToList();
        Assert.Equal([16, 1], unionCounts);
        AssertAnswer(result.Answers["price 0"], "number", number: 0);
        AssertAnswer(result.Answers["price 16"], "number", number: 16);
    }

    /// <summary>Records each request and answers with a body built from it. Safe for concurrent calls.</summary>
    private sealed class RespondingHandler(Func<string, string> respond) : HttpMessageHandler
    {
        private readonly List<(string Url, string Body)> _requests = [];

        public IReadOnlyList<(string Url, string Body)> Requests
        {
            get
            {
                lock (_requests)
                {
                    return _requests.ToList();
                }
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            lock (_requests)
            {
                _requests.Add((request.RequestUri?.AbsoluteUri ?? string.Empty, body));
            }

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(respond(body), Encoding.UTF8, "application/json") };
        }
    }
}
