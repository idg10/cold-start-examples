using Endjin.Badger;
using Endjin.Imm.Contracts;
using Endjin.Imm.Domain;
using Endjin.Imm.Processing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Endjin.Imm.Services;

public class ImmBadgeService
{
    private readonly IRuleDefinitionRepositorySource ruleDefinitionRepositorySource;
    private readonly IIpMaturityMatrixSource immSource;

    public ImmBadgeService(
        IRuleDefinitionRepositorySource ruleDefinitionRepositorySource,
        IIpMaturityMatrixSource immSource)
    {
        this.ruleDefinitionRepositorySource = ruleDefinitionRepositorySource;
        this.immSource = immSource;
    }
    public async Task<string> GitHubImmTotalScore(
        string rulesObjectName,
        string projectObjectName,
        string org,
        string project)
    {
        (IRuleDefinitionRepository ruleSet, IpMaturityMatrix ruleAssertions) =
            await this.GetImmRulesFromGitHubAsync(org, project, rulesObjectName, projectObjectName).ConfigureAwait(false);
        var evaluationEngine = new EvaluationEngine(ruleSet);

        ImmEvaluation evaluationResult = evaluationEngine.Evaluate(ruleAssertions);

        string svg = BadgePainter.DrawSVG(
            "IMM",
            $"{evaluationResult.TotalScore} / {evaluationResult.MaximumPossibleTotalScore}",
            GetColourSchemeForPercentage(100M * evaluationResult.TotalScore / evaluationResult.MaximumPossibleTotalScore),
            Style.Flat);

        return svg;
    }


    private async Task<(IRuleDefinitionRepository RuleSet, IpMaturityMatrix Rules)> GetImmRulesFromGitHubAsync(
        string org,
        string project,
        string ruleDefinitionsObjectName,
        string projectObjectName)
    {
        Task<IRuleDefinitionRepository> rulesetTask = this.ruleDefinitionRepositorySource.GetRuleDefinitionRepositoryAsync(ruleDefinitionsObjectName);
        Task<IpMaturityMatrix> immTask = this.immSource.GetIpMaturityMatrixAsync(org, project, projectObjectName);

        await Task.WhenAll(new Task[] { rulesetTask, immTask }).ConfigureAwait(false);

        return (rulesetTask.Result, immTask.Result);
    }

    private static string GetColourSchemeForPercentage(decimal percentage)
    {
        return percentage switch
        {
            var _ when percentage < 33 => ColorScheme.Red,
            var _ when percentage < 66 => ColorScheme.Yellow,
            var _ when percentage > 66
                    && percentage < 99 => ColorScheme.YellowGreen,
            100 => ColorScheme.Green,
            _ => ColorScheme.Red,
        };
    }
}