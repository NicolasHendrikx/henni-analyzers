using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Be.HenNi.Analyzers.Constructions;

public class Data : IMember
{
    public string Identifier { get; private set; } = string.Empty;
    public string ReturnType { get; private set; } = String.Empty;
    public IEnumerable<string> Modifiers { get; private set; } = Array.Empty<string>();

    public bool IsPrivate
    {
        get
        {
            var nonPrivateModifiers = new string[] { "public", "protected", "internal" };
            var hasNonPrivateModifiers = Modifiers.Any(m => nonPrivateModifiers.Contains(m));
            return !hasNonPrivateModifiers;
        }
    }

    private Data() {}
    
    public static Data FromPropertyParameter(ParameterSyntax parameterSyntax)
        => new Data
        {
            Identifier = parameterSyntax.Identifier.ToString(),
            ReturnType = parameterSyntax.Type?.ToString() ?? "",
            Modifiers = parameterSyntax
                .Modifiers
                .Select(t => t.ToString())
        };

    public static Data FromVariable(VariableDeclaratorSyntax vds)
        => new Data
        {
            Identifier = vds.Identifier.ToString(),
            ReturnType = (vds.Parent as VariableDeclarationSyntax)?.Type?.ToString() ?? "",
            Modifiers = (vds?.Parent?.Parent as FieldDeclarationSyntax)
                ?.Modifiers
                .Select(t => t.ToString()) ?? Array.Empty<string>()
        };

    public static Data FromProperty(PropertyDeclarationSyntax pds)
        => new Data
        {
            Identifier = pds.Identifier.ToString(),
            ReturnType = pds.Type.ToString() ?? "",
            Modifiers = pds.Modifiers
                .Select(t => t.ToString())
        };
}