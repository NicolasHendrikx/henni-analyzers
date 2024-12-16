using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Be.HenNi.Analyzers.Constructions;

public class Operation : IMember
{
    public string Identifier { get; internal set; } = string.Empty;
    public string ReturnType { get; internal set; } = string.Empty;
    public IEnumerable<string> Modifiers { get; internal set; } = Array.Empty<string>();

    public string Body { get; internal set; } = string.Empty;

    public virtual bool DependsOn(IMember member)
        => Body.Contains(member.Identifier);

    internal virtual bool Matches(ArgumentListSyntax? argumentsList)
        => false;
}