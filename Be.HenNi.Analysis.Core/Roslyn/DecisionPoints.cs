using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Be.HenNi.Analysis.Core.Roslyn;

/// <summary>
/// Decision points counted by McCabe's cyclomatic complexity.
/// </summary>
internal static class DecisionPoints
{
    public static int Of(IOperation operation)
        => operation switch
        {
            IConditionalOperation => 1,
            ILoopOperation => 1,
            ICatchClauseOperation => 1,
            ISwitchCaseOperation section => section.Clauses.Count(clause => clause.CaseKind != CaseKind.Default),
            ISwitchExpressionArmOperation arm => arm.Pattern is IDiscardPatternOperation ? 0 : 1,
            IBinaryOperation { OperatorKind: BinaryOperatorKind.ConditionalAnd or BinaryOperatorKind.ConditionalOr } => 1,
            ICoalesceOperation => 1,
            ICoalesceAssignmentOperation => 1,
            IConditionalAccessOperation => 1,
            _ => 0
        };
}
