using System.Collections.Generic;
using System.Linq;

namespace Be.HenNi.Analysis.Core.Model;

/// <summary>
/// Language-neutral view of a type, built once and shared by every metric.
/// </summary>
public sealed class TypeModel
{
    public TypeModel(
        string id,
        string name,
        TypeModelKind kind,
        IEnumerable<AttributeModel> attributes,
        IEnumerable<MethodModel> methods)
    {
        Id = id;
        Name = name;
        Kind = kind;
        Attributes = attributes.ToList();
        Methods = methods.ToList();
    }

    public string Id { get; }

    public string Name { get; }

    public TypeModelKind Kind { get; }

    public IReadOnlyList<AttributeModel> Attributes { get; }

    public IReadOnlyList<MethodModel> Methods { get; }

    public override string ToString()
        => Id;
}
