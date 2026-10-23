using FsCheck.Xunit;
using PayeeMatch.Decisions;

namespace PayeeMatch.Core.Tests.Decisions;

public class DecisionEngineProperties
{
    private static readonly DecisionEngine Engine = new();

    [Property]
    public bool Every_decision_carries_at_least_one_reason(string provided, string onRecord) =>
        provided is null || onRecord is null || Engine.Decide(provided, onRecord).Reasons.Count > 0;

    [Property]
    public bool The_outcome_always_agrees_with_the_score(string provided, string onRecord)
    {
        if (provided is null || onRecord is null)
        {
            return true;
        }

        var decision = Engine.Decide(provided, onRecord);
        return decision.Outcome == Engine.Thresholds.Classify(decision.Score);
    }
}
