// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.AnthropicLanguageModelTests.AnthropicLanguageModelTestSupport;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; agent skills</c>.</summary>
public sealed class DoGenerateAgentSkillsTests
{
    private const string Fixture = "anthropic-code-execution-20250825.pptx-skill";

    private const string TwoSkills =
        """{"container":{"id":"test-container-id","skills":[{"type":"anthropic","skillId":"pptx","version":"latest"},{"type":"custom","providerReference":{"anthropic":"skill_01Xud7kLMsjLfc7Aa6RvigZf"},"version":"1.0"}]}}""";

    private const string PptxSkill =
        """{"container":{"skills":[{"type":"anthropic","skillId":"pptx","version":"latest"}]}}""";

    private static Vercel.AI.Provider.LanguageModelTool CodeExecution()
    {
        return ProviderTool("anthropic.code_execution_20250825", "code_execution");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "doGenerate > agent skills::should send request body with skills in container",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_request_body_with_skills_in_container()
    {
        var handler = JsonFixture(Fixture);

        var result = await DoGenerate(handler, Options(TwoSkills, [CodeExecution()]));

        AssertJson(
            HelloBody(
                """
                {
                  "container": {
                    "id": "test-container-id",
                    "skills": [
                      {"skill_id":"pptx","type":"anthropic","version":"latest"},
                      {"skill_id":"skill_01Xud7kLMsjLfc7Aa6RvigZf","type":"custom","version":"1.0"}
                    ]
                  },
                  "tools": [{"name":"code_execution","type":"code_execution_20250825"}]
                }
                """
            ),
            RequestBody(handler)
        );
        AssertJson("[]", WarningsJson(result.Warnings));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > agent skills::should add a warning when the code execution tool is not present",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_add_a_warning_when_the_code_execution_tool_is_not_present()
    {
        var handler = JsonFixture(Fixture);

        var result = await DoGenerate(handler, Options(TwoSkills));

        AssertJson(
            HelloBody(
                """
                {
                  "container": {
                    "id": "test-container-id",
                    "skills": [
                      {"skill_id":"pptx","type":"anthropic","version":"latest"},
                      {"skill_id":"skill_01Xud7kLMsjLfc7Aa6RvigZf","type":"custom","version":"1.0"}
                    ]
                  }
                }
                """
            ),
            RequestBody(handler)
        );
        AssertJson(
            """[{"message":"code execution tool is required when using skills","type":"other"}]""",
            WarningsJson(result.Warnings)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > agent skills::should include beta headers when skills are configured",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_beta_headers_when_skills_are_configured()
    {
        var handler = JsonFixture(Fixture);

        await DoGenerate(handler, Options(PptxSkill, [CodeExecution()]));

        AssertRequestHeaders(
            handler,
            ("anthropic-beta", "code-execution-2025-08-25,skills-2025-10-02,files-api-2025-04-14"),
            ("anthropic-version", "2023-06-01"),
            ("content-type", "application/json"),
            ("x-api-key", "test-api-key")
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > agent skills::should expose container information as provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_expose_container_information_as_provider_metadata()
    {
        var result = await DoGenerate(JsonFixture(Fixture), Options(PptxSkill, [CodeExecution()]));

        AssertJson(
            Snapshot(
                "doGenerate > agent skills > should expose container information as provider metadata"
            ),
            JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > agent skills::should resolve custom skill provider references at the Anthropic boundary",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_resolve_custom_skill_provider_references_at_the_Anthropic_boundary()
    {
        var handler = JsonFixture(Fixture);

        await DoGenerate(
            handler,
            Options(
                """{"container":{"skills":[{"type":"custom","providerReference":{"anthropic":"skill_01Xud7kLMsjLfc7Aa6RvigZf"}}]}}""",
                [CodeExecution()]
            )
        );

        AssertJson(
            HelloBody(
                """
                {
                  "container": {"skills":[{"skill_id":"skill_01Xud7kLMsjLfc7Aa6RvigZf","type":"custom"}]},
                  "tools": [{"name":"code_execution","type":"code_execution_20250825"}]
                }
                """
            ),
            RequestBody(handler)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "doGenerate > agent skills::should throw when a custom skill provider reference does not include anthropic",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_when_a_custom_skill_provider_reference_does_not_include_anthropic()
    {
        await Assert.ThrowsAsync<NoSuchProviderReferenceError>(() =>
            DoGenerate(
                JsonFixture(Fixture),
                Options(
                    """{"container":{"skills":[{"type":"custom","providerReference":{"openai":"skill_abc"}}]}}"""
                )
            )
        );
    }
}
