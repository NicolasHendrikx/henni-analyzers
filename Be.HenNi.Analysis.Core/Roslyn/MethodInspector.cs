using System;
using System.Collections.Generic;
using System.Linq;
using Be.HenNi.Analysis.Core.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Be.HenNi.Analysis.Core.Roslyn;

/// <summary>
/// Walks the operations of one method body to collect what metrics need:
/// complexity, own attributes accessed, own methods called and foreign data accessed.
/// </summary>
internal sealed class MethodInspector
{
    private readonly INamedTypeSymbol _type;
    private readonly AttributeTable _attributes;
    private readonly HashSet<string> _accessedAttributes = new();
    private readonly HashSet<string> _calledMethods = new();
    private readonly HashSet<ForeignDataAccess> _foreignData = new();
    private int _decisionPoints;

    private MethodInspector(INamedTypeSymbol type, AttributeTable attributes)
    {
        _type = type;
        _attributes = attributes;
    }

    public static MethodModel Inspect(
        IMethodSymbol method,
        INamedTypeSymbol type,
        AttributeTable attributes,
        Func<SyntaxTree, SemanticModel> semanticModelOf)
    {
        var inspector = new MethodInspector(type, attributes);
        var body = MethodBodies.Of(method);
        var accessedAttribute = Accessors.AccessedAttribute(method);

        if (body is null && accessedAttribute is not null && attributes.Find(accessedAttribute) is { } autoProperty)
        {
            inspector._accessedAttributes.Add(autoProperty.Id);
        }

        if (body is not null && semanticModelOf(body.SyntaxTree).GetOperation(body) is { } operation)
        {
            foreach (var descendant in operation.DescendantsAndSelf())
            {
                inspector.Visit(descendant);
            }
        }

        return new MethodModel(
            Ids.Of(method),
            method.Name,
            KindOf(method),
            Ids.VisibilityOf(method.DeclaredAccessibility),
            method.IsStatic,
            body is not null,
            accessedAttribute is not null,
            1 + inspector._decisionPoints,
            MethodBodies.StatementCount(body),
            inspector._accessedAttributes,
            inspector._calledMethods,
            inspector._foreignData);
    }

    private void Visit(IOperation operation)
    {
        _decisionPoints += DecisionPoints.Of(operation);

        switch (operation)
        {
            case IFieldReferenceOperation field:
                VisitField(field.Field.OriginalDefinition);
                break;
            case IPropertyReferenceOperation property:
                VisitProperty(property.Property.OriginalDefinition, property);
                break;
            case IInvocationOperation invocation:
                VisitCall((invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod).OriginalDefinition);
                break;
            case IMethodReferenceOperation methodGroup when IsOwn(methodGroup.Method.ContainingType):
                _calledMethods.Add(Ids.Of(methodGroup.Method));
                break;
            case IParameterReferenceOperation parameter:
                VisitParameter(parameter.Parameter);
                break;
        }
    }

    private void VisitField(IFieldSymbol field)
    {
        if (_attributes.Find(field) is { } attribute)
        {
            _accessedAttributes.Add(attribute.Id);
            return;
        }

        if (!field.IsConst && field.ContainingType.TypeKind != TypeKind.Enum && IsForeign(field.ContainingType))
        {
            _foreignData.Add(new ForeignDataAccess(Ids.Of(field), Ids.Of(field.ContainingType)));
        }
    }

    private void VisitProperty(IPropertySymbol property, IOperation reference)
    {
        if (_attributes.Find(property) is { } attribute)
        {
            _accessedAttributes.Add(attribute.Id);
            return;
        }

        if (IsOwn(property.ContainingType))
        {
            if (IsRead(reference) && property.GetMethod is { } getter)
            {
                _calledMethods.Add(Ids.Of(getter));
            }

            if (IsWritten(reference) && property.SetMethod is { } setter)
            {
                _calledMethods.Add(Ids.Of(setter));
            }

            return;
        }

        if (!property.IsIndexer && IsForeign(property.ContainingType))
        {
            _foreignData.Add(new ForeignDataAccess(Ids.Of(property), Ids.Of(property.ContainingType)));
        }
    }

    private void VisitCall(IMethodSymbol method)
    {
        if (IsOwn(method.ContainingType))
        {
            _calledMethods.Add(Ids.Of(method));
            return;
        }

        if (IsForeign(method.ContainingType) && Accessors.AccessedAttribute(method) is { } data)
        {
            _foreignData.Add(new ForeignDataAccess(Ids.Of(data), Ids.Of(method.ContainingType)));
        }
    }

    private void VisitParameter(IParameterSymbol parameter)
    {
        if (!_type.IsRecord
            && parameter.ContainingSymbol is IMethodSymbol { MethodKind: MethodKind.Constructor } constructor
            && IsOwn(constructor.ContainingType)
            && constructor.DeclaringSyntaxReferences.Any(r => r.GetSyntax() is TypeDeclarationSyntax))
        {
            _accessedAttributes.Add(_attributes.Captured(parameter).Id);
        }
    }

    private bool IsOwn(INamedTypeSymbol? type)
        => SymbolEqualityComparer.Default.Equals(type?.OriginalDefinition, _type.OriginalDefinition);

    /// <summary>
    /// Foreign types are the types of the analyzed code base that are neither the type itself nor one of its ancestors.
    /// Library types (BCL, packages) are not data providers.
    /// </summary>
    private bool IsForeign(INamedTypeSymbol? type)
        => type is not null
           && !IsOwn(type)
           && !IsAncestor(type)
           && type.Locations.Any(location => location.IsInSource);

    private bool IsAncestor(INamedTypeSymbol type)
    {
        for (var ancestor = _type.BaseType; ancestor is not null; ancestor = ancestor.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(ancestor.OriginalDefinition, type.OriginalDefinition))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsRead(IOperation reference)
        => !(reference.Parent is ISimpleAssignmentOperation assignment && assignment.Target == reference);

    private static bool IsWritten(IOperation reference)
        => reference.Parent is IAssignmentOperation assignment && assignment.Target == reference
           || reference.Parent is IIncrementOrDecrementOperation;

    private static MethodModelKind KindOf(IMethodSymbol method)
        => method.MethodKind switch
        {
            MethodKind.Constructor => MethodModelKind.Constructor,
            MethodKind.StaticConstructor => MethodModelKind.StaticConstructor,
            MethodKind.Destructor => MethodModelKind.Finalizer,
            MethodKind.UserDefinedOperator or MethodKind.Conversion => MethodModelKind.Operator,
            MethodKind.PropertyGet => MethodModelKind.Getter,
            MethodKind.PropertySet => MethodModelKind.Setter,
            MethodKind.EventAdd or MethodKind.EventRemove or MethodKind.EventRaise => MethodModelKind.EventAccessor,
            _ => MethodModelKind.Method
        };
}
