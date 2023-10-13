using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Be.HenNi.Analyzers.Constructions;

public class TypeConstruction
{
    private readonly TypeDeclarationSyntax _typeDeclarationSyntax;
    
    public TypeConstruction(TypeDeclarationSyntax typeDeclaration)
    {
        _typeDeclarationSyntax = typeDeclaration ?? throw new ArgumentException(nameof(typeDeclaration));
    }

    public IEnumerable<FieldDeclaration> Fields
        => ParametersMembers.Concat(ExplicitInstanceFields).Concat(BakedInstanceFields);

    public IEnumerable<FieldDeclaration> ParametersMembers 
        => _typeDeclarationSyntax
               .ParameterList?
               .Parameters
               .Select(FieldDeclaration.FromPropertyParameter) ??
                                                              new FieldDeclaration[] {};

    public IEnumerable<FieldDeclaration> ExplicitInstanceFields => _typeDeclarationSyntax
        .Members
        .OfType<FieldDeclarationSyntax>()
        .Where(fds => !fds.Modifiers.Any(t => IsTokenAnyOf(t, SyntaxKind.StaticKeyword,SyntaxKind.ConstKeyword)))
        .SelectMany(fds => fds.Declaration.Variables)
        .Select(FieldDeclaration.FromVariable);

    public IEnumerable<FieldDeclaration> BakedInstanceFields => _typeDeclarationSyntax
        .Members
        .OfType<PropertyDeclarationSyntax>()
        .Where(pds => !pds.Modifiers.Any(t => IsTokenAnyOf(t, SyntaxKind.StaticKeyword,SyntaxKind.ConstKeyword)))
        .Where(pds => pds.AccessorList != null && IsAutoImplemented(pds.AccessorList.Accessors))
        .Select(FieldDeclaration.FromProperty);
    
    public IEnumerable<MethodDeclaration> Computed
        => Methods.Concat(Properties).Concat(Indexers).Concat(Events);
    
    public IEnumerable<MethodDeclaration> Methods 
        => _typeDeclarationSyntax.ChildNodes()
            .OfType<MethodDeclarationSyntax>()
            .Select(MethodDeclarationSimpleFactory.FromNode);

    public IEnumerable<MethodDeclaration> Properties
        => _typeDeclarationSyntax.ChildNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Where(pds => pds.AccessorList != null)
            .SelectMany<PropertyDeclarationSyntax,AccessorDeclarationSyntax>(pds => pds.AccessorList?.Accessors)
            .Where(accessor => accessor.Body != null || accessor.ExpressionBody != null)
            .Select(MethodDeclarationSimpleFactory.FromNode)
            .Concat
            (
                _typeDeclarationSyntax.ChildNodes()
                    .OfType<PropertyDeclarationSyntax>()
                    .Where(pds => pds.ExpressionBody != null)
                    .Select(MethodDeclarationSimpleFactory.FromNode)
            );
    
    public IEnumerable<MethodDeclaration> Indexers
        =>_typeDeclarationSyntax.ChildNodes()
            .OfType<IndexerDeclarationSyntax>()
            .Where(ids => ids.AccessorList != null)
            .SelectMany<IndexerDeclarationSyntax,AccessorDeclarationSyntax>(pds => pds.AccessorList?.Accessors)
            .Select(MethodDeclarationSimpleFactory.FromNode)
            .Concat
            (
                _typeDeclarationSyntax.ChildNodes()
                    .OfType<IndexerDeclarationSyntax>()
                    .Where(ids => ids.ExpressionBody != null)
                    .Select(MethodDeclarationSimpleFactory.FromNode)
            );

    public IEnumerable<MethodDeclaration> Events
        => _typeDeclarationSyntax.ChildNodes()
            .OfType<EventDeclarationSyntax>()
            .SelectMany(eds => eds?.AccessorList?.Accessors.ToArray() ?? Array.Empty<AccessorDeclarationSyntax>())
            .Select(MethodDeclarationSimpleFactory.FromNode);

    public string SimpleName
        => _typeDeclarationSyntax.Identifier.ToString();

    private bool IsAutoImplemented(SyntaxList<AccessorDeclarationSyntax> accessors)
        => accessors.Any(a => a.Body == null && a.ExpressionBody == null);

    private bool IsTokenAnyOf(SyntaxToken token, params SyntaxKind[] kinds)
        => kinds.Any(k => token.IsKind(k));
}