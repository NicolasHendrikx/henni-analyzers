using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Be.HenNi.Analysis.Core.Roslyn;

/// <summary>
/// Recognizes accessor methods (getters / setters) structurally, without a semantic model,
/// so that accessors of other types are recognized too.
/// </summary>
internal static class Accessors
{
    /// <summary>
    /// The field or auto-property read or written by <paramref name="method"/> when it is an accessor; null otherwise.
    /// </summary>
    public static ISymbol? AccessedAttribute(IMethodSymbol method)
    {
        if (method.IsStatic)
        {
            return null;
        }

        var body = MethodBodies.Of(method);
        if (body is null)
        {
            return method.AssociatedSymbol is IPropertySymbol property && HasBackingField(property) ? property : null;
        }

        var expression = SingleExpressionOf(body);
        if (expression is null)
        {
            return null;
        }

        if (IsGetterShaped(method))
        {
            return AttributeDesignatedBy(expression, method);
        }

        if (IsSetterShaped(method)
            && expression is AssignmentExpressionSyntax assignment
            && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression)
            && assignment.Right is IdentifierNameSyntax assigned
            && assigned.Identifier.ValueText == method.Parameters[0].Name
            && !(assignment.Left is IdentifierNameSyntax target && target.Identifier.ValueText == assigned.Identifier.ValueText))
        {
            return AttributeDesignatedBy(assignment.Left, method);
        }

        return null;
    }

    public static bool HasBackingField(IPropertySymbol property)
        => property.ContainingType
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Any(field => SymbolEqualityComparer.Default.Equals(field.AssociatedSymbol, property));

    private static bool IsGetterShaped(IMethodSymbol method)
        => method.Parameters.Length == 0
           && (method.MethodKind == MethodKind.PropertyGet
               || method.MethodKind == MethodKind.Ordinary && !method.ReturnsVoid);

    private static bool IsSetterShaped(IMethodSymbol method)
        => method.Parameters.Length == 1
           && (method.MethodKind == MethodKind.PropertySet
               || method.MethodKind == MethodKind.Ordinary && method.ReturnsVoid);

    private static ExpressionSyntax? SingleExpressionOf(SyntaxNode body)
        => body switch
        {
            ArrowExpressionClauseSyntax arrow => arrow.Expression,
            BlockSyntax { Statements.Count: 1 } block => block.Statements[0] switch
            {
                ReturnStatementSyntax returned => returned.Expression,
                ExpressionStatementSyntax statement => statement.Expression,
                _ => null
            },
            _ => null
        };

    private static ISymbol? AttributeDesignatedBy(ExpressionSyntax expression, IMethodSymbol method)
        => expression switch
        {
            FieldExpressionSyntax => method.AssociatedSymbol as IPropertySymbol,
            IdentifierNameSyntax name => AttributeNamed(name.Identifier.ValueText, method.ContainingType),
            MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } access
                => AttributeNamed(access.Name.Identifier.ValueText, method.ContainingType),
            _ => null
        };

    private static ISymbol? AttributeNamed(string name, INamedTypeSymbol type)
        => type.GetMembers(name).FirstOrDefault(member => member switch
        {
            IFieldSymbol field => !field.IsStatic && !field.IsConst,
            IPropertySymbol property => !property.IsStatic && HasBackingField(property),
            _ => false
        });
}
