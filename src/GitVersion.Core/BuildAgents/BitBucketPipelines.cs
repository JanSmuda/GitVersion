﻿using GitVersion.Logging;
using GitVersion.OutputVariables;

namespace GitVersion.BuildAgents;

public class BitBucketPipelines : BuildAgentBase
{
    public const string EnvironmentVariableName = "BITBUCKET_WORKSPACE";
    public const string BranchEnvironmentVariableName = "BITBUCKET_BRANCH";
    public const string TagEnvironmentVariableName = "BITBUCKET_TAG";
    public const string PullRequestEnvironmentVariableName = "BITBUCKET_PR_ID";
    private string? file;

    public BitBucketPipelines(IEnvironment environment, ILog log) : base(environment, log) => WithPropertyFile("gitversion.env");

    protected override string EnvironmentVariable => EnvironmentVariableName;

    public override string? GenerateSetVersionMessage(VersionVariables variables) => variables.FullSemVer;

    public void WithPropertyFile(string propertiesFileName) => this.file = propertiesFileName;

    public override string[] GenerateSetParameterMessage(string name, string value) => new[]
    {
        $"GITVERSION_{name.ToUpperInvariant()}={value}"
    };

    public override string? GetCurrentBranch(bool usingDynamicRepos)
    {
        var branchName = EvaluateEnvironmentVariable(BranchEnvironmentVariableName);
        if (branchName != null && branchName.StartsWith("refs/heads/"))
        {
            return branchName;
        }

        return null;
    }

    public override void WriteIntegration(Action<string?> writer, VersionVariables variables, bool updateBuildNumber = true)
    {
        if (this.file is null)
            return;

        base.WriteIntegration(writer, variables, updateBuildNumber);
        writer($"Outputting variables to '{this.file}' ... ");

        var exports = variables
            .Select(variable => $"export GITVERSION_{variable.Key.ToUpperInvariant()}={variable.Value}")
            .ToList();

        File.WriteAllLines(this.file, exports);
    }


    private string? EvaluateEnvironmentVariable(string variableName)
    {
        var branchName = Environment.GetEnvironmentVariable(variableName);
        Log.Info("Evaluating environment variable {0} : {1}", variableName, branchName!);
        return branchName;
    }
}
