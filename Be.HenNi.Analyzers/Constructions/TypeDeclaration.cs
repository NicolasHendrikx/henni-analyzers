using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Be.HenNi.Analyzers.Constructions;

public class TypeDeclaration
{
    private readonly TypeDeclarationSyntax _typeDeclarationSyntax;
    
    public TypeDeclaration(TypeDeclarationSyntax typeDeclaration)
    {
        _typeDeclarationSyntax = typeDeclaration;
    }

    public IEnumerable<Data> Data
        => ParametersMembers.Concat(ExplicitInstanceFields).Concat(BakedInstanceFields);

    public IEnumerable<Data> ParametersMembers 
        => (_typeDeclarationSyntax as RecordDeclarationSyntax)
               ?.ParameterList
               ?.Parameters
               .Select(Constructions.Data.FromPropertyParameter) ??
                                                              new Data[] {};

    public IEnumerable<Data> ExplicitInstanceFields 
        => _typeDeclarationSyntax
        .Members
        .OfType<FieldDeclarationSyntax>()
        .Where(fds => !fds.Modifiers.Any(t => IsTokenAnyOf(t, SyntaxKind.StaticKeyword,SyntaxKind.ConstKeyword)))
        .SelectMany(fds => fds.Declaration.Variables)
        .Select(Constructions.Data.FromVariable);

    public IEnumerable<Data> BakedInstanceFields 
        => _typeDeclarationSyntax
        .Members
        .OfType<PropertyDeclarationSyntax>()
        .Where(pds => !pds.Modifiers.Any(t => IsTokenAnyOf(t, SyntaxKind.StaticKeyword,SyntaxKind.ConstKeyword)))
        .Where(pds => pds.AccessorList != null && IsAutoImplemented(pds.AccessorList.Accessors))
        .Select(Constructions.Data.FromProperty);
    
    public IEnumerable<Operation> Operations
        => Methods.Concat(Properties).Concat(Indexers).Concat(Events);
    
    public IEnumerable<Operation> Methods 
        => _typeDeclarationSyntax.ChildNodes()
            .OfType<MethodDeclarationSyntax>()
            .Select(MethodDeclarationSimpleFactory.FromNode);

    public IEnumerable<Operation> Properties
        => _typeDeclarationSyntax.ChildNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Where(pds => pds.AccessorList != null)
            .SelectMany<PropertyDeclarationSyntax,AccessorDeclarationSyntax>(pds => pds.AccessorList?.Accessors ?? new SyntaxList<AccessorDeclarationSyntax>())
            .Where(accessor => accessor.Body != null || accessor.ExpressionBody != null)
            .Select(MethodDeclarationSimpleFactory.FromNode)
            .Concat
            (
                _typeDeclarationSyntax.ChildNodes()
                    .OfType<PropertyDeclarationSyntax>()
                    .Where(pds => pds.ExpressionBody != null)
                    .Select(MethodDeclarationSimpleFactory.FromNode)
            );
    
    public IEnumerable<Operation> Indexers
        =>_typeDeclarationSyntax.ChildNodes()
            .OfType<IndexerDeclarationSyntax>()
            .Where(ids => ids.AccessorList != null)
            .SelectMany<IndexerDeclarationSyntax,AccessorDeclarationSyntax>(pds => pds.AccessorList?.Accessors ?? new SyntaxList<AccessorDeclarationSyntax>())
            .Select(MethodDeclarationSimpleFactory.FromNode)
            .Concat
            (
                _typeDeclarationSyntax.ChildNodes()
                    .OfType<IndexerDeclarationSyntax>()
                    .Where(ids => ids.ExpressionBody != null)
                    .Select(MethodDeclarationSimpleFactory.FromNode)
            );

    public IEnumerable<Operation> Events
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