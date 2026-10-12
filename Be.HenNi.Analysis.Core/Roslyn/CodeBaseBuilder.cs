using System;
using System.Collections.Generic;
using System.Linq;
using Be.HenNi.Analysis.Core.Model;
using Microsoft.CodeAnalysis;

namespace Be.HenNi.Analysis.Core.Roslyn;

/// <summary>
/// Builds the model of every class, struct and record declared in the sources of a compilation.
/// </summary>
public static class CodeBaseBuilder
{
    public static CodeBase Build(Compilation compilation)
        => Build(compilation, tree => compilation.GetSemanticModel(tree));

    /// <param name="semanticModelOf">
    /// Provides the semantic models, so that an analyzer can reuse the ones the compiler already computed.
    /// </param>
    public static CodeBase Build(Compilation compilation, Func<SyntaxTree, SemanticModel> semanticModelOf)
        => new(TypesOf(compilation).Select(type => TypeModelBuilder.Build(type, semanticModelOf)));

    /// <summary>
    /// The modelled types, in the order of <see cref="CodeBase.Types"/>.
    /// </summary>
    public static IEnumerable<INamedTypeSymbol> TypesOf(Compilation compilation)
        => TypesOf(compilation.Assembly.GlobalNamespace);

    private static IEnumerable<INamedTypeSymbol> TypesOf(INamespaceOrTypeSymbol container)
    {
        foreach (var member in container.GetMembers())
        {
            switch (member)
            {
                case INamespaceSymbol nested:
                    foreach (var type in TypesOf(nested))
                    {
                        yield return type;
                    }
                    break;
                case INamedTypeSymbol type when IsModelled(type):
                    yield return type;
                    foreach (var inner in TypesOf(type))
                    {
                        yield return inner;
                    }
                    break;
            }
        }
    }

    private static bool IsModelled(INamedTypeSymbol type)
        => type.TypeKind is TypeKind.Class or TypeKind.Struct
           && !type.IsImplicitlyDeclared
           && type.Locations.Any(location => location.IsInSource);
}
