using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Be.HenNi.Analyzers.Constructions;

public class MethodDeclarationSimpleFactory
{
    public static MethodDeclaration FromNode(CSharpSyntaxNode node)
        => node switch
        {
            MethodDeclarationSyntax method => FromMethod(method),
            AccessorDeclarationSyntax accessor => FromAccessor(accessor),
            PropertyDeclarationSyntax expressionProp => FromExpressionProperty(expressionProp),
            IndexerDeclarationSyntax expressionIndexer => FromExpressionIndexer(expressionIndexer),
            _ => throw new ArgumentException(nameof(node))
        };

    private static MethodDeclaration FromMethod(MethodDeclarationSyntax arg)
        => new MethodDeclaration
        {
            Identifier = arg.Identifier.ToString(),
            Type = arg.ReturnType.ToString(),
            Modifiers = arg.Modifiers.Select(token => token.ToString()),
            Body = arg.Body?.ToString() ?? arg.ExpressionBody?.ToString() ?? ""
        };
    
    private static MethodDeclaration FromAccessor(AccessorDeclarationSyntax arg)
        => new MethodDeclaration
        {
            Identifier = (arg.Parent as PropertyDeclarationSyntax)?.Identifier.ToString() ?? string.Empty,
            Type = (arg.Parent as PropertyDeclarationSyntax)?.Type.ToString() ?? string.Empty,
            Modifiers = arg.Modifiers.Select(token => token.ToString()),
            Body = arg.Body?.ToString() ?? arg.ExpressionBody?.ToString() ?? ""
        };
    
    private static MethodDeclaration FromExpressionProperty(PropertyDeclarationSyntax arg)
        => new MethodDeclaration
        {
            Identifier = arg.Identifier.ToString() ?? string.Empty,
            Type = arg.Type.ToString() ?? string.Empty,
            Modifiers = arg.Modifiers.Select(token => token.ToString()),
            Body = arg.ExpressionBody?.ToString() ?? ""
        };
    
    private static MethodDeclaration FromExpressionIndexer(IndexerDeclarationSyntax arg)
        => new MethodDeclaration
        {
            Identifier = $"this[{arg.ParameterList.ToString()}]",
            Type = arg.Type.ToString() ?? string.Empty,
            Modifiers = arg.Modifiers.Select(token => token.ToString()),
            Body = arg.ExpressionBody?.ToString() ?? ""
        };
    
}