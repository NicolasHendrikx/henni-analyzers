using System.Collections.Generic;
using System.Linq;
using Be.HenNi.Analysis.Core.Model;
using Microsoft.CodeAnalysis;

namespace Be.HenNi.Analysis.Core.Roslyn;

/// <summary>
/// Maps the symbols designating the state of a type (fields, auto-properties, backing fields,
/// captured primary constructor parameters) to their attribute.
/// </summary>
internal sealed class AttributeTable
{
    private readonly INamedTypeSymbol _type;
    private readonly Dictionary<ISymbol, AttributeModel> _bySymbol = new(SymbolEqualityComparer.Default);
    private readonly List<AttributeModel> _all = new();

    public AttributeTable(INamedTypeSymbol type)
    {
        _type = type;

        foreach (var field in type.GetMembers().OfType<IFieldSymbol>().Where(f => !f.IsStatic && !f.IsConst))
        {
            switch (field.AssociatedSymbol)
            {
                case IPropertySymbol property:
                    var autoProperty = Add(property, new AttributeModel(
                        Ids.Of(property), property.Name, property.Type.ToDisplayString(),
                        AttributeKind.AutoProperty, Visibility.Private));
                    _bySymbol[field] = autoProperty;
                    break;
                case null when !field.IsImplicitlyDeclared:
                    Add(field, new AttributeModel(
                        Ids.Of(field), field.Name, field.Type.ToDisplayString(),
                        AttributeKind.Field, Ids.VisibilityOf(field.DeclaredAccessibility)));
                    break;
            }
        }
    }

    public IReadOnlyList<AttributeModel> All
        => _all;

    public AttributeModel? Find(ISymbol symbol)
        => _bySymbol.TryGetValue(symbol.OriginalDefinition, out var attribute) ? attribute : null;

    public AttributeModel Captured(IParameterSymbol parameter)
        => Find(parameter) ?? Add(parameter, new AttributeModel(
            $"A:{Ids.Of(_type).Substring(2)}.{parameter.Name}", parameter.Name, parameter.Type.ToDisplayString(),
            AttributeKind.PrimaryConstructorParameter, Visibility.Private));

    private AttributeModel Add(ISymbol symbol, AttributeModel attribute)
    {
        _bySymbol[symbol.OriginalDefinition] = attribute;
        _all.Add(attribute);
        return attribute;
    }
}
