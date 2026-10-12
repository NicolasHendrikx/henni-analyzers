using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Be.HenNi.Analysis.Core.Roslyn;

internal static class MethodBodies
{
    /// <summary>
    /// The block or arrow expression holding the code of <paramref name="method"/>, if any.
    /// </summary>
    public static SyntaxNode? Of(IMethodSymbol method)
        => (method.PartialImplementationPart ?? method)
            .DeclaringSyntaxReferences
            .Select(reference => BodyOf(reference.GetSyntax()))
            .FirstOrDefault(body => body is not null);

    /// <summary>
    /// Non commenting source statements: every statement except blocks and empty statements.
    /// An expression body counts as one statement.
    /// </summary>
    public static int StatementCount(SyntaxNode? body)
        => body switch
        {
            null => 0,
            ArrowExpressionClauseSyntax => 1,
            _ => body.DescendantNodes()
                .OfType<StatementSyntax>()
                .Count(statement => statement is not BlockSyntax and not EmptyStatementSyntax)
        };

    private static SyntaxNode? BodyOf(SyntaxNode declaration)
        => declaration switch
        {
            BaseMethodDeclarationSyntax method => (SyntaxNode?)method.Body ?? method.ExpressionBody,
            AccessorDeclarationSyntax accessor => (SyntaxNode?)accessor.Body ?? accessor.ExpressionBody,
            ArrowExpressionClauseSyntax expressionBodiedProperty => expressionBodiedProperty,
            _ => null
        };
}
