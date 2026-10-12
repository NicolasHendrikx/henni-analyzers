namespace Be.HenNi.Analysis.Core.Model;

public enum AttributeKind
{
    /// <summary>An explicitly declared instance field.</summary>
    Field,

    /// <summary>The compiler-generated backing field of an auto-property (including record positional properties).</summary>
    AutoProperty,

    /// <summary>A primary constructor parameter captured by a member body, hence stored as hidden state.</summary>
    PrimaryConstructorParameter
}
