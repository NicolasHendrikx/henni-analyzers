using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Be.HenNi.Analyzers.Analyzers;

/// <summary>
/// Reports the catch clauses that add nothing to the exception they catch (PMD AvoidRethrowingException and
/// AvoidThrowingNewInstanceOfSameException):
/// HE0013, a catch clause whose only statement rethrows the exception (<c>throw;</c> or <c>throw e;</c>);
/// HE0014, a new exception of the very type just caught thrown from a catch clause.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ExceptionRethrowAnalyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(ParseCatch, SyntaxKind.CatchClause);
        context.RegisterSyntaxNodeAction(ParseThrow, SyntaxKind.ThrowStatement, SyntaxKind.ThrowExpression);
    }

    private static void ParseCatch(SyntaxNodeAnalysisContext context)
    {
        var clause = (CatchClauseSyntax)context.Node;
        if (clause.Filter is null
            && clause.Block.Statements.Count == 1
            && clause.Block.Statements[0] is ThrowStatementSyntax @throw
            && RethrowsCaughtException(context.SemanticModel, clause, @throw, context.CancellationToken)
            && !IsInterceptingLaterCatch(context.SemanticModel, clause, context.CancellationToken))
        {
            context.ReportDiagnostic(Diagnostic.Create(RuleUselessRethrow, clause.GetLocation()));
        }
    }

    /// <summary>
    /// <c>throw;</c>, or <c>throw e;</c> with <c>e</c> the exception just caught.
    /// </summary>
    private static bool RethrowsCaughtException(SemanticModel model, CatchClauseSyntax clause, ThrowStatementSyntax @throw, CancellationToken cancellationToken)
    {
        if (@throw.Expression is null)
        {
            return true;
        }

        return clause.Declaration is { } declaration
               && !declaration.Identifier.IsKind(SyntaxKind.None)
               && SymbolEqualityComparer.Default.Equals(
                   model.GetSymbolInfo(@throw.Expression, cancellationToken).Symbol,
                   model.GetDeclaredSymbol(declaration, cancellationToken));
    }

    /// <summary>
    /// A rethrowing clause is meaningful when a following clause of the same try would otherwise catch the exception:
    /// removing it would change which handler runs.
    /// </summary>
    private static bool IsInterceptingLaterCatch(SemanticModel model, CatchClauseSyntax clause, CancellationToken cancellationToken)
    {
        if (clause.Parent is not TryStatementSyntax tryStatement)
        {
            return false;
        }

        var caughtType = clause.Declaration is null ? null : model.GetTypeInfo(clause.Declaration.Type, cancellationToken).Type;
        return tryStatement.Catches
            .SkipWhile(candidate => candidate != clause)
            .Skip(1)
            .Any(later => later.Declaration is null
                          || IsSameOrBaseOf(model.GetTypeInfo(later.Declaration.Type, cancellationToken).Type, caughtType));
    }

    private static bool IsSameOrBaseOf(ITypeSymbol? candidate, ITypeSymbol? type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, candidate))
            {
                return true;
            }
        }

        return false;
    }

    private static void ParseThrow(SyntaxNodeAnalysisContext context)
    {
        var thrown = context.Node switch
        {
            ThrowStatementSyntax statement => statement.Expression,
            ThrowExpressionSyntax expression => expression.Expression,
            _ => null
        };
        if (thrown is not BaseObjectCreationExpressionSyntax creation
            || EnclosingCatchClause(context.Node) is not { Declaration: { } declaration })
        {
            return;
        }

        var caughtType = context.SemanticModel.GetTypeInfo(declaration.Type, context.CancellationToken).Type;
        var createdType = context.SemanticModel.GetTypeInfo(creation, context.CancellationToken).Type;
        if (caughtType is not null && SymbolEqualityComparer.Default.Equals(caughtType, createdType))
        {
            context.ReportDiagnostic(Diagnostic.Create(RuleSameExceptionType, context.Node.GetLocation(), caughtType.ToDisplayString()));
        }
    }

    /// <summary>
    /// The catch clause whose block directly contains <paramref name="node"/>, if the node is not in a nested function.
    /// </summary>
    private static CatchClauseSyntax? EnclosingCatchClause(SyntaxNode node)
    {
        for (var current = node.Parent; current is not null; current = current.Parent)
        {
            switch (current)
            {
                case CatchClauseSyntax clause:
                    return clause.Block.Span.Contains(node.Span) ? clause : null;
                case AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax or MemberDeclarationSyntax:
                    return null;
            }
        }

        return null;
    }

    private const string Category = "Security";

    private static readonly DiagnosticDescriptor RuleUselessRethrow = new("HE0013",
        "Useless rethrow", "This catch clause only rethrows the exception. Remove it or add context.", Category,
        DiagnosticSeverity.Warning, isEnabledByDefault: true,
        description: "A catch clause that only rethrows the exception has the same effect as no catch clause at all: it adds noise and hides the intent of the code, "
                     + "and 'throw e;' even resets the stack trace. Remove it, or make it useful: handle or log the exception, "
                     + "or wrap it in an exception that adds context (keeping the original one as inner exception).");

    private static readonly DiagnosticDescriptor RuleSameExceptionType = new("HE0014",
        "Throwing the caught exception type", "Avoid to throw a new instance of the caught exception type {0}. Rethrow the exception or throw a more meaningful type.", Category,
        DiagnosticSeverity.Warning, isEnabledByDefault: true,
        description: "Throwing a new exception of the type just caught tells nothing more to the callers that already handle this type, "
                     + "and makes the origin of the failure harder to find. Use 'throw;' to keep the exception, or throw a type that carries the meaning of the failure "
                     + "at this level of abstraction, keeping the original exception as inner exception.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(RuleUselessRethrow, RuleSameExceptionType);
}
