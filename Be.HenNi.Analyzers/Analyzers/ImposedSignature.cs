using System.Linq;
using Microsoft.CodeAnalysis;

namespace Be.HenNi.Analyzers.Analyzers;

internal static class ImposedSignature
{
    /// <summary>
    /// The signature of an override or of an implementation of an interface is not the choice of the developer.
    /// A parameter is imposed when its method is.
    /// </summary>
    public static bool IsImposed(ISymbol symbol)
    {
        if (symbol is IParameterSymbol parameter)
        {
            symbol = parameter.ContainingSymbol;
        }

        return symbol.IsOverride || ImplementsExplicitly(symbol) || ImplementsImplicitly(symbol);
    }

    private static bool ImplementsExplicitly(ISymbol symbol)
        => symbol switch
        {
            IMethodSymbol method => !method.ExplicitInterfaceImplementations.IsDefaultOrEmpty,
            IPropertySymbol property => !property.ExplicitInterfaceImplementations.IsDefaultOrEmpty,
            _ => false
        };

    private static bool ImplementsImplicitly(ISymbol symbol)
    {
        var type = symbol.ContainingType;
        return type is { TypeKind: not TypeKind.Interface }
               && type.AllInterfaces
                   .SelectMany(contract => contract.GetMembers(symbol.Name))
                   .Any(member => SymbolEqualityComparer.Default.Equals(type.FindImplementationForInterfaceMember(member), symbol));
    }
}
