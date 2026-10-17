// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools.OpenAIResponsesPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class ShellTests
{
    private const string Id = Suite + " > shell::";

    private static PreparedResponsesTools PrepareShell(string argsJson) => Prepare(null, ProviderTool("openai.shell", "shell", argsJson));

    private static string Shell(string environmentJson) => """[{"type":"shell","environment":""" + environmentJson + "}]";

    [Fact]
    [UpstreamTest(Id + "should prepare shell tool without environment args", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_shell_tool_without_environment_args()
    {
        var result = PrepareShell("{}");

        AssertTools(result, """[{"type":"shell"}]""");
        AssertNoToolChoice(result);
    }

    [Fact]
    [UpstreamTest(Id + "should prepare shell tool with containerAuto without skills", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_shell_tool_with_containerAuto_without_skills()
    {
        var result = PrepareShell("""{"environment":{"type":"containerAuto"}}""");

        AssertTools(result, Shell("""{"type":"container_auto"}"""));
        AssertNoToolChoice(result);
    }

    [Fact]
    [UpstreamTest(Id + "should prepare shell tool with containerAuto and providerReference skills", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_shell_tool_with_containerAuto_and_providerReference_skills()
    {
        var result = PrepareShell("""{"environment":{"type":"containerAuto","skills":[{"type":"skillReference","providerReference":{"openai":"skill_abc"},"version":"1.0.0"}]}}""");

        AssertTools(result, Shell("""{"type":"container_auto","skills":[{"type":"skill_reference","skill_id":"skill_abc","version":"1.0.0"}]}"""));
        AssertNoToolChoice(result);
    }

    [Fact]
    [UpstreamTest(Id + "should default shell skillReference version to latest when omitted", Coverage = UpstreamCoverage.Covered)]
    public void Should_default_shell_skillReference_version_to_latest_when_omitted()
    {
        var result = PrepareShell("""{"environment":{"type":"containerAuto","skills":[{"type":"skillReference","providerReference":{"openai":"skill_abc"}}]}}""");

        AssertTools(result, Shell("""{"type":"container_auto","skills":[{"type":"skill_reference","skill_id":"skill_abc","version":"latest"}]}"""));
        AssertNoToolChoice(result);
    }

    [Fact]
    [UpstreamTest(Id + "should throw when a providerReference cannot be resolved for openai", Coverage = UpstreamCoverage.Covered)]
    public void Should_throw_when_a_providerReference_cannot_be_resolved_for_openai()
    {
        var error = Assert.Throws<NoSuchProviderReferenceError>(() =>
            PrepareShell("""{"environment":{"type":"containerAuto","skills":[{"type":"skillReference","providerReference":{"anthropic":"skill_abc"}}]}}"""));

        Assert.True(NoSuchProviderReferenceError.IsInstance(error));
        Assert.Equal("openai", error.Provider);
        Assert.Equal(new Dictionary<string, string> { ["anthropic"] = "skill_abc" }, error.Reference);
    }

    [Fact]
    [UpstreamTest(Id + "should prepare shell tool with containerAuto and inline skill", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_shell_tool_with_containerAuto_and_inline_skill()
    {
        var result = PrepareShell("""{"environment":{"type":"containerAuto","skills":[{"type":"inline","name":"my-skill","description":"A test skill","source":{"type":"base64","mediaType":"application/zip","data":"dGVzdA=="}}]}}""");

        AssertTools(result, Shell("""{"type":"container_auto","skills":[{"type":"inline","name":"my-skill","description":"A test skill","source":{"type":"base64","media_type":"application/zip","data":"dGVzdA=="}}]}"""));
        AssertNoToolChoice(result);
    }

    [Fact]
    [UpstreamTest(Id + "should prepare shell tool with containerAuto and networkPolicy disabled", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_shell_tool_with_containerAuto_and_networkPolicy_disabled()
    {
        var result = PrepareShell("""{"environment":{"type":"containerAuto","networkPolicy":{"type":"disabled"}}}""");

        AssertTools(result, Shell("""{"type":"container_auto","network_policy":{"type":"disabled"}}"""));
        AssertNoToolChoice(result);
    }

    [Fact]
    [UpstreamTest(Id + "should prepare shell tool with containerAuto and networkPolicy allowlist with domain secrets", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_shell_tool_with_containerAuto_and_networkPolicy_allowlist_with_domain_secrets()
    {
        var result = PrepareShell("""{"environment":{"type":"containerAuto","networkPolicy":{"type":"allowlist","allowedDomains":["example.com","api.test.org"],"domainSecrets":[{"domain":"api.test.org","name":"API_KEY","value":"secret123"}]}}}""");

        AssertTools(result, Shell("""{"type":"container_auto","network_policy":{"type":"allowlist","allowed_domains":["example.com","api.test.org"],"domain_secrets":[{"domain":"api.test.org","name":"API_KEY","value":"secret123"}]}}"""));
        AssertNoToolChoice(result);
    }

    [Fact]
    [UpstreamTest(Id + "should prepare shell tool with containerAuto, fileIds, and memoryLimit", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_shell_tool_with_containerAuto_fileIds_and_memoryLimit()
    {
        var result = PrepareShell("""{"environment":{"type":"containerAuto","fileIds":["file-1","file-2"],"memoryLimit":"16g"}}""");

        AssertTools(result, Shell("""{"type":"container_auto","file_ids":["file-1","file-2"],"memory_limit":"16g"}"""));
        AssertNoToolChoice(result);
    }

    [Fact]
    [UpstreamTest(Id + "should prepare shell tool with containerReference", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_shell_tool_with_containerReference()
    {
        var result = PrepareShell("""{"environment":{"type":"containerReference","containerId":"ctr_abc123"}}""");

        AssertTools(result, Shell("""{"type":"container_reference","container_id":"ctr_abc123"}"""));
        AssertNoToolChoice(result);
    }

    [Fact]
    [UpstreamTest(Id + "should prepare shell tool with local environment and skills", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_shell_tool_with_local_environment_and_skills()
    {
        var result = PrepareShell("""{"environment":{"type":"local","skills":[{"name":"calculator","description":"Perform math calculations","path":"/path/to/calculator"}]}}""");

        AssertTools(result, Shell("""{"type":"local","skills":[{"name":"calculator","description":"Perform math calculations","path":"/path/to/calculator"}]}"""));
        AssertNoToolChoice(result);
    }

    [Fact]
    [UpstreamTest(Id + "should prepare shell tool with local environment without explicit type", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_shell_tool_with_local_environment_without_explicit_type()
    {
        var result = PrepareShell("""{"environment":{"skills":[{"name":"calculator","description":"Perform math calculations","path":"/path/to/calculator"}]}}""");

        AssertTools(result, Shell("""{"type":"local","skills":[{"name":"calculator","description":"Perform math calculations","path":"/path/to/calculator"}]}"""));
        AssertNoToolChoice(result);
    }

    [Fact]
    [UpstreamTest(Id + "should prepare shell tool with local environment without skills", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_shell_tool_with_local_environment_without_skills()
    {
        var result = PrepareShell("""{"environment":{"type":"local"}}""");

        AssertTools(result, Shell("""{"type":"local"}"""));
        AssertNoToolChoice(result);
    }
}
