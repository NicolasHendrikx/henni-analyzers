using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Be.HenNi.Analyzers.Constructions;

public class FieldDeclaration : IMemberDeclaration
{
    public string Identifier { get; private set; } = string.Empty;
    public string Type { get; private set; } = String.Empty;
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

    private FieldDeclaration() {}
    
    public static FieldDeclaration FromPropertyParameter(ParameterSyntax parameterSyntax)
        => new FieldDeclaration
        {
            Identifier = parameterSyntax.Identifier.ToString(),
            Type = parameterSyntax.Type?.ToString() ?? "",
            Modifiers = parameterSyntax
                .Modifiers
                .Select(t => t.ToString())
        };

    public static FieldDeclaration FromVariable(VariableDeclaratorSyntax vds)
        => new FieldDeclaration
        {
            Identifier = vds.Identifier.ToString(),
            Type = (vds.Parent as VariableDeclarationSyntax)?.Type?.ToString() ?? "",
            Modifiers = (vds?.Parent?.Parent as FieldDeclarationSyntax)
                ?.Modifiers
                .Select(t => t.ToString()) ?? Array.Empty<string>()
        };

    public static FieldDeclaration FromProperty(PropertyDeclarationSyntax pds)
        => new FieldDeclaration
        {
            Identifier = pds.Identifier.ToString(),
            Type = pds.Type.ToString() ?? "",
            Modifiers = pds.Modifiers
                .Select(t => t.ToString())
        };
}