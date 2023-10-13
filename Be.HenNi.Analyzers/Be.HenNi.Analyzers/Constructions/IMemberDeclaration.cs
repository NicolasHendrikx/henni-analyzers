using System.Collections.Generic;

namespace Be.HenNi.Analyzers.Constructions;

public interface IMemberDeclaration
{
    string Identifier { get; }
    string Type { get; }
    IEnumerable<string> Modifiers { get; }
}