namespace Be.HenNi.Analysis.Core.Model;

/// <summary>
/// An attribute, in the object-oriented sense: a piece of instance state owned by a type.
/// Static fields and constants are not attributes.
/// </summary>
public sealed class AttributeModel
{
    public AttributeModel(string id, string name, string typeName, AttributeKind kind, Visibility visibility)
    {
        Id = id;
        Name = name;
        TypeName = typeName;
        Kind = kind;
        Visibility = visibility;
    }

    public string Id { get; }

    public string Name { get; }

    public string TypeName { get; }

    public AttributeKind Kind { get; }

    /// <summary>
    /// Visibility of the storage itself. Auto-properties and captured parameters are always private:
    /// they are exposed, if ever, through accessor methods.
    /// </summary>
    public Visibility Visibility { get; }

    public bool IsVisible
        => Visibility != Visibility.Private;
}
