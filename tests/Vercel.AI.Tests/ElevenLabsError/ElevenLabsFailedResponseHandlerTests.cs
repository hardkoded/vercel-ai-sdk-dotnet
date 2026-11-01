// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.ElevenLabs;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ElevenLabsError;

public sealed class ElevenLabsFailedResponseHandlerTests
{
    private const string Prefix = "packages/elevenlabs/src/elevenlabs-error.test.ts::elevenlabsFailedResponseHandler::";
    private const string Url = "https://api.elevenlabs.io/v1/text-to-speech/test-voice";
    private const string UpstreamCommit = "7062ab6818c90e135d068433ce99b11ac0dfa081";

    [Fact]
    [UpstreamTest(Prefix + "should use the message and code of an object detail", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_use_the_message_and_code_of_an_object_detail()
    {
        const string body = """
            {"detail":{"type":"not_found","code":"voice_not_found","message":"A voice with voice_id 'x' was not found.","status":"voice_not_found","request_id":"req_test_404"}}
            """;

        var error = Handle(body, 404);

        Assert.Equal("A voice with voice_id 'x' was not found. (voice_not_found)", error.Message);
        Assert.Equal(404, error.StatusCode);
        var data = Assert.IsType<ElevenLabsErrorData>(error.ErrorData);
        Assert.Equal("not_found", data.Detail!.Type);
        Assert.Equal("voice_not_found", data.Detail.Code);
        Assert.Equal("A voice with voice_id 'x' was not found.", data.Detail.Message);
        Assert.Equal("voice_not_found", data.Detail.Status);
        Assert.Equal("req_test_404", data.Detail.RequestId);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use the message of an object detail without a code", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_use_the_message_of_an_object_detail_without_a_code()
    {
        const string body = """
            {"detail":{"code":null,"status":"invalid_content","message":"File is corrupted. Please ensure it is playable audio.","param":null}}
            """;

        var error = Handle(body, 400);

        Assert.Equal("File is corrupted. Please ensure it is playable audio.", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should join the location and message of each array detail issue", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_join_the_location_and_message_of_each_array_detail_issue()
    {
        const string body = """
            {"detail":[{"type":"missing","loc":["body","text"],"msg":"Field required","input":null},{"type":"less_than_equal","loc":["body","voice_settings","stability",0],"msg":"Input should be less than or equal to 1"},{"msg":"Request is invalid"}]}
            """;

        var error = Handle(body, 422);

        Assert.Equal(
            "body.text: Field required; body.voice_settings.stability.0: Input should be less than or equal to 1; Request is invalid",
            error.Message);
        var data = Assert.IsType<ElevenLabsErrorData>(error.ErrorData);
        Assert.Collection(
            data.Issues!,
            issue =>
            {
                Assert.Equal("missing", issue.Type);
                Assert.Equal(new[] { "body", "text" }, issue.Location!.Select(value => value.ToString()));
                Assert.Equal("Field required", issue.Message);
                Assert.Equal(JsonValueKind.Null, issue.AdditionalProperties["input"].ValueKind);
            },
            issue =>
            {
                Assert.Equal("less_than_equal", issue.Type);
                Assert.Equal(new[] { "body", "voice_settings", "stability", "0" }, issue.Location!.Select(value => value.ToString()));
                Assert.Equal("Input should be less than or equal to 1", issue.Message);
            },
            issue =>
            {
                Assert.Null(issue.Type);
                Assert.Null(issue.Location);
                Assert.Equal("Request is invalid", issue.Message);
            });
    }

    [Fact]
    [UpstreamTest(Prefix + "should use a string detail", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_use_a_string_detail()
    {
        const string body = """{"detail":"Not authenticated"}""";

        var error = Handle(body, 401);

        Assert.Equal("Not authenticated", error.Message);
        var data = Assert.IsType<ElevenLabsErrorData>(error.ErrorData);
        Assert.Equal("Not authenticated", data.DetailMessage);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use the message of a legacy error object", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_use_the_message_of_a_legacy_error_object()
    {
        const string body = """{"error":{"message":"Quota exceeded","code":429}}""";

        var error = Handle(body, 429);

        Assert.Equal("Quota exceeded", error.Message);
        var data = Assert.IsType<ElevenLabsErrorData>(error.ErrorData);
        Assert.Equal("Quota exceeded", data.Error!.Message);
        Assert.Equal(429d, data.Error.Code);
    }

    private static ApiException Handle(string body, int statusCode)
    {
        return ElevenLabsFailedResponseHandler.Handle(body, statusCode, Url, new Dictionary<string, string>());
    }
}
