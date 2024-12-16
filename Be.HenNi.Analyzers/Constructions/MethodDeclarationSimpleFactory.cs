using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Be.HenNi.Analyzers.Constructions;

public class MethodDeclarationSimpleFactory
{
    public static Operation FromNode(CSharpSyntaxNode node)
        => node switch
        {
            MethodDeclarationSyntax method => FromMethod(method),
            AccessorDeclarationSyntax accessor => FromAccessor(accessor),
            PropertyDeclarationSyntax expressionProp => FromExpressionProperty(expressionProp),
            IndexerDeclarationSyntax expressionIndexer => FromExpressionIndexer(expressionIndexer),
            _ => throw new ArgumentException(nameof(node))
        };

    private static Operation FromMethod(MethodDeclarationSyntax arg)
        => new Operation
        {
            Identifier = arg.Identifier.ToString(),
            ReturnType = arg.ReturnType.ToString(),
            Modifiers = arg.Modifiers.Select(token => token.ToString()),
            Body = arg.Body?.ToString() ?? arg.ExpressionBody?.ToString() ?? ""
        };
    
    private static Operation FromAccessor(AccessorDeclarationSyntax arg)
        => new Operation
        {
            Identifier = (arg.Parent as PropertyDeclarationSyntax)?.Identifier.ToString() ?? string.Empty,
            ReturnType = (arg.Parent as PropertyDeclarationSyntax)?.Type.ToString() ?? string.Empty,
            Modifiers = arg.Modifiers.Select(token => token.ToString()),
            Body = arg.Body?.ToString() ?? arg.ExpressionBody?.ToString() ?? ""
        };
    
    private static Operation FromExpressionProperty(PropertyDeclarationSyntax arg)
        => new Operation
        {
            Identifier = arg.Identifier.ToString() ?? string.Empty,
            ReturnType = arg.Type.ToString() ?? string.Empty,
            Modifiers = arg.Modifiers.Select(token => token.ToString()),
            Body = arg.ExpressionBody?.ToString() ?? ""
        };
    
    private static Operation FromExpressionIndexer(IndexerDeclarationSyntax arg)
        => new Operation
        {
            Identifier = $"this[{arg.ParameterList.ToString()}]",
            ReturnType = arg.Type.ToString() ?? string.Empty,
            Modifiers = arg.Modifiers.Select(token => token.ToString()),
            Body = arg.ExpressionBody?.ToString() ?? ""
        };
    
}