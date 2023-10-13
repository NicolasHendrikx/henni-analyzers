using System;
using System.Collections.Generic;

namespace Be.HenNi.Analyzers.Constructions;

public class EventDeclaration : IMemberDeclaration
{
    public string Identifier { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public IEnumerable<string> Modifiers { get; set; } = Array.Empty<string>();
    
    public string Body { get; set; } = string.Empty;
}