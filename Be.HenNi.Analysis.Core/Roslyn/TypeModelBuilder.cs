using System;
using System.Collections.Generic;
using System.Linq;
using Be.HenNi.Analysis.Core.Model;
using Microsoft.CodeAnalysis;

namespace Be.HenNi.Analysis.Core.Roslyn;

/// <summary>
/// Builds a <see cref="TypeModel"/> from Roslyn symbols and operations.
/// </summary>
public static class TypeModelBuilder
{
    public static TypeModel Build(INamedTypeSymbol type, Compilation compilation)
        => Build(type, tree => compilation.GetSemanticModel(tree));

    public static TypeModel Build(INamedTypeSymbol type, Func<SyntaxTree, SemanticModel> semanticModelOf)
    {
        var attributes = new AttributeTable(type);
        var methods = MethodsOf(type)
            .Select(method => MethodInspector.Inspect(method, type, attributes, semanticModelOf))
            .ToList();

        return new TypeModel(Ids.Of(type), type.Name, KindOf(type), attributes.All, methods);
    }

    private static IEnumerable<IMethodSymbol> MethodsOf(INamedTypeSymbol type)
        => type.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(method => !method.IsImplicitlyDeclared
                             || method.AssociatedSymbol is IPropertySymbol { IsImplicitlyDeclared: false });

    private static TypeModelKind KindOf(INamedTypeSymbol type)
        => type switch
        {
            { IsRecord: true, TypeKind: TypeKind.Struct } => TypeModelKind.RecordStruct,
            { IsRecord: true } => TypeModelKind.Record,
            { TypeKind: TypeKind.Class } => TypeModelKind.Class,
            { TypeKind: TypeKind.Struct } => TypeModelKind.Struct,
            { TypeKind: TypeKind.Interface } => TypeModelKind.Interface,
            _ => TypeModelKind.Other
        };
}
