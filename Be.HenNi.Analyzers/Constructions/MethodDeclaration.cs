using System;
using System.Collections.Generic;

namespace Be.HenNi.Analyzers.Constructions;

public class MethodDeclaration : IMemberDeclaration
{
    public string Identifier { get; internal set; } = string.Empty;
    public string Type { get; internal set; } = string.Empty;
    public IEnumerable<string> Modifiers { get; internal set; } = Array.Empty<string>();

    public string Body { get; internal set; } = string.Empty;

    public bool DependsOn(FieldDeclaration field)
        => Body.Contains(field.Identifier);
}