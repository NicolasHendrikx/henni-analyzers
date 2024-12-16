using System.Collections.Generic;

namespace Be.HenNi.Analyzers.Constructions;

public interface IMember
{
    string Identifier { get; }
    string ReturnType { get; }
    IEnumerable<string> Modifiers { get; }
}