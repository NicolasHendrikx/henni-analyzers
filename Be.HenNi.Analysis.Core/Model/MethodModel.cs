using System.Collections.Generic;

namespace Be.HenNi.Analysis.Core.Model;

/// <summary>
/// An operation declared by a type: method, constructor, operator or accessor.
/// Compiler-synthesized members (record equality, implicit constructors...) are not modelled.
/// </summary>
public sealed class MethodModel
{
    public MethodModel(
        string id,
        string name,
        MethodModelKind kind,
        Visibility visibility,
        bool isStatic,
        bool hasBody,
        bool isAccessor,
        int cyclomaticComplexity,
        int statementCount,
        IEnumerable<string> accessedAttributes,
        IEnumerable<string> calledMethods,
        IEnumerable<ForeignDataAccess> foreignDataAccesses)
    {
        Id = id;
        Name = name;
        Kind = kind;
        Visibility = visibility;
        IsStatic = isStatic;
        HasBody = hasBody;
        IsAccessor = isAccessor;
        CyclomaticComplexity = cyclomaticComplexity;
        StatementCount = statementCount;
        AccessedAttributes = new HashSet<string>(accessedAttributes);
        CalledMethods = new HashSet<string>(calledMethods);
        ForeignDataAccesses = new HashSet<ForeignDataAccess>(foreignDataAccesses);
    }

    public string Id { get; }

    public string Name { get; }

    public MethodModelKind Kind { get; }

    public Visibility Visibility { get; }

    public bool IsStatic { get; }

    /// <summary>False for abstract members, auto-property accessors and primary constructors.</summary>
    public bool HasBody { get; }

    /// <summary>
    /// True when the method only reads or writes one attribute: auto-property accessor,
    /// <c>get => _x;</c>, <c>set => _x = value;</c>, or Java-style <c>GetX()</c> / <c>SetX(x)</c>.
    /// </summary>
    public bool IsAccessor { get; }

    public int CyclomaticComplexity { get; }

    public int StatementCount { get; }

    /// <summary>Ids of the attributes of the declaring type that this method accesses directly.</summary>
    public IReadOnlyCollection<string> AccessedAttributes { get; }

    /// <summary>Ids of the methods of the declaring type called by this method (property accessors included).</summary>
    public IReadOnlyCollection<string> CalledMethods { get; }

    public IReadOnlyCollection<ForeignDataAccess> ForeignDataAccesses { get; }

    public bool IsVisible
        => Visibility != Visibility.Private;

    public bool IsConstructorLike
        => Kind is MethodModelKind.Constructor or MethodModelKind.StaticConstructor or MethodModelKind.Finalizer;

    public bool IsAutoAccessor
        => IsAccessor && !HasBody;

    public override string ToString()
        => Id;
}
